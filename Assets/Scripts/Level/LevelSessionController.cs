using System.Collections.Generic;
using GlitchCompiler.Anomalies;
using GlitchCompiler.Core;
using GlitchCompiler.Data;
using GlitchCompiler.Rendering;
using GlitchCompiler.VCode;
using UnityEngine;

namespace GlitchCompiler.Level
{
    /// <summary>
    /// The only bridge between the editor/rendering pipeline and level rules.
    /// Configure this component in the Level scene and submit completed V-Code
    /// output through its public methods.
    /// </summary>
    public sealed class LevelSessionController : MonoBehaviour
    {
        [SerializeField] private LevelLoader levelLoader;
    public enum LevelStartFailure { None, MissingDependencies, InvalidIndex, Locked }

    // Owns the level lifecycle. UI and V-Code code only need to call the public Submit methods.
    public sealed class LevelSessionController : MonoBehaviour
    {
        [SerializeField] private LevelLoader loader;
        [SerializeField] private GameLoopController gameLoop;
        [SerializeField] private LevelTimer timer;
        [SerializeField] private LevelCompletionEvaluator completionEvaluator;
        [SerializeField] private AnomalyManager anomalyManager;

        private bool completionRecorded;
        private bool anomalyContextConfigured;

        public bool BeginLevel(int levelIndex)
        {
            if (levelLoader == null || gameLoop == null || timer == null || !levelLoader.Load(levelIndex))
            {
                return false;
            }

            completionRecorded = false;
            gameLoop.Begin(levelLoader.Current);
            return true;
        }

        public void ConfigureAnomalyContext(AnomalyContext context)
        {
            if (anomalyManager != null && context != null)
            {
                anomalyManager.Initialize(context);
                anomalyContextConfigured = true;
            }
        }

        public void SubmitSystemCommands(IEnumerable<SystemCommand> commands)
        {
            if (anomalyManager != null && commands != null)
            {
                anomalyManager.AcceptSystemCommands(commands);
            }
        }

        public void SubmitRenderedCanvas(Texture2D renderedCanvas)
        {
            var level = levelLoader == null ? null : levelLoader.Current;
            if (level == null || completionEvaluator == null || gameLoop == null || gameLoop.State != GameState.Playing)
            {
                return;
            }

            var match = completionEvaluator.Evaluate(level, renderedCanvas);
            ApplicationBootstrap.Events?.Publish(new MatchChanged(match));
            if (!completionEvaluator.IsComplete(level, match))
            {
                return;
            }

            gameLoop.Win(match);
            RecordCompletion(level, match);
        }

        private void Update()
        {
            var level = levelLoader == null ? null : levelLoader.Current;
            if (level == null || gameLoop == null || timer == null || gameLoop.State != GameState.Playing)
            {
                return;
            }

            var elapsed = level.TimeLimitSeconds <= 0
                ? 1f
                : 1f - Mathf.Clamp01(timer.Remaining / level.TimeLimitSeconds);
            gameLoop.UpdatePhase(elapsed);
            if (anomalyManager != null && anomalyContextConfigured)
            {
                anomalyManager.TryTrigger(level.Anomalies, gameLoop.Phase);
            }
        }

        private void RecordCompletion(LevelDefinition level, float match)
        {
            if (completionRecorded || levelLoader.CurrentIndex < 0)
            {
                return;
            }

            completionRecorded = true;
            var elapsedSeconds = timer == null ? 0 : Mathf.Max(0, level.TimeLimitSeconds - timer.Remaining);
            ApplicationBootstrap.Profile?.RecordWin(level.Id, levelLoader.CurrentIndex, match, elapsedSeconds);
        [SerializeField] private int levelIndex;
        [SerializeField] private bool startOnAwake = true;

        public LevelDefinition CurrentLevel => loader == null ? null : loader.Current;
        public GameState State => gameLoop == null ? GameState.Booting : gameLoop.State;
        public float CurrentMatch { get; private set; }
        public LevelStartFailure LastStartFailure { get; private set; }

        private AnomalyContext anomalyContext;
        private bool recordedWin;

        private void Awake()
        {
            anomalyContext = CreateNoOpAnomalyContext();
            if (timer != null) timer.Changed += OnTimerChanged;
            if (ApplicationBootstrap.Events != null) ApplicationBootstrap.Events.Subscribe<LevelFinished>(OnLevelFinished);
        }

        private void Start()
        {
            if (startOnAwake) StartLevel(levelIndex);
        }

        private void Update()
        {
            if (gameLoop == null || timer == null || gameLoop.State != GameState.Playing) return;
            gameLoop.UpdatePhase(timer.NormalizedElapsed);
            anomalyManager?.TryTrigger(CurrentLevel.Anomalies, gameLoop.Phase);
        }

        private void OnDestroy()
        {
            if (timer != null) timer.Changed -= OnTimerChanged;
            if (ApplicationBootstrap.Events != null) ApplicationBootstrap.Events.Unsubscribe<LevelFinished>(OnLevelFinished);
        }

        public bool StartLevel(int index)
        {
            if (loader == null || gameLoop == null) return RejectStart(index, LevelStartFailure.MissingDependencies);
            if (!loader.CanLoad(index)) return RejectStart(index, LevelStartFailure.InvalidIndex);
            if (ApplicationBootstrap.Profile != null && !ApplicationBootstrap.Profile.IsUnlocked(index))
                return RejectStart(index, LevelStartFailure.Locked);
            if (!loader.Load(index)) return RejectStart(index, LevelStartFailure.InvalidIndex);
            levelIndex = index;
            CurrentMatch = 0f;
            recordedWin = false;
            anomalyManager?.Initialize(anomalyContext);
            gameLoop.Begin(loader.Current);
            LastStartFailure = LevelStartFailure.None;
            ApplicationBootstrap.Events?.Publish(new LevelStarted(loader.Current.Id, levelIndex));
            return true;
        }

        public bool PauseLevel()
        {
            if (gameLoop == null || !gameLoop.Pause()) return false;
            anomalyManager?.SetPaused(true);
            return true;
        }

        public bool ResumeLevel()
        {
            if (gameLoop == null || !gameLoop.Resume()) return false;
            anomalyManager?.SetPaused(false);
            return true;
        }

        public bool RestartCurrentLevel() => CurrentLevel != null && StartLevel(levelIndex);

        // The only gameplay-facing interface the V-Code/rendering module may call after Compile.
        public bool SubmitCompilation(CompilationSubmission submission)
        {
            if (submission == null || State != GameState.Playing) return false;
            if (!submission.Succeeded) return true;

            anomalyManager?.AcceptSystemCommands(submission.SystemCommands);
            if (submission.RenderedCanvas == null || completionEvaluator == null) return true;

            CurrentMatch = completionEvaluator.Evaluate(CurrentLevel, submission.RenderedCanvas);
            ApplicationBootstrap.Events?.Publish(new MatchChanged(CurrentMatch));
            if (completionEvaluator.IsComplete(CurrentLevel, CurrentMatch)) gameLoop.Win(CurrentMatch);
            return true;
        }

        // Interface for the UI owner: pass real editor/overlay/timer callbacks when its Prefab is ready.
        public void ConfigureAnomalyContext(AnomalyContext value)
        {
            anomalyContext = value ?? CreateNoOpAnomalyContext();
            if (CurrentLevel != null) anomalyManager?.Initialize(anomalyContext);
        }

        private void OnTimerChanged(float remaining) => ApplicationBootstrap.Events?.Publish(new TimerChanged(remaining));

        private bool RejectStart(int index, LevelStartFailure reason)
        {
            LastStartFailure = reason;
            ApplicationBootstrap.Events?.Publish(new LevelStartRejected(index, reason));
            return false;
        }

        private void OnLevelFinished(LevelFinished result)
        {
            anomalyManager?.EndSession();
            if (!result.Success || recordedWin || CurrentLevel == null || ApplicationBootstrap.Profile == null) return;
            recordedWin = true;
            var elapsed = timer == null ? 0f : timer.Duration - timer.Remaining;
            ApplicationBootstrap.Profile.RecordWin(CurrentLevel.Id, levelIndex, result.Match, elapsed);
        }

        private AnomalyContext CreateNoOpAnomalyContext()
        {
            return new AnomalyContext
            {
                ReadCode = () => string.Empty,
                WriteCode = _ => { },
                ShowOverlay = (_, __) => { },
                HideOverlay = () => { },
                AddTime = seconds => timer?.Add(seconds),
                SetTimerMultiplier = value => timer?.SetMultiplier(value),
                ShieldEnabled = () => false,
                ResetReceived = () => false
            };
        }
    }
}
