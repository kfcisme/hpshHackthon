using GlitchCompiler.Anomalies;
using GlitchCompiler.Core;
using GlitchCompiler.Data;
using GlitchCompiler.Rendering;
using UnityEngine;

namespace GlitchCompiler.Level
{
    public enum LevelStartFailure
    {
        None,
        MissingDependencies,
        InvalidIndex,
        InvalidDefinition,
        Locked
    }

    /// <summary>
    /// Owns the level lifecycle. UI and V-Code code only interact with a level
    /// through this component's public methods.
    /// </summary>
    public sealed class LevelSessionController : MonoBehaviour
    {
        [SerializeField] private LevelLoader loader;
        [SerializeField] private GameLoopController gameLoop;
        [SerializeField] private LevelTimer timer;
        [SerializeField] private LevelCompletionEvaluator completionEvaluator;
        [SerializeField] private AnomalyManager anomalyManager;
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
            if (gameLoop == null || timer == null || CurrentLevel == null || gameLoop.State != GameState.Playing) return;
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
            if (loader == null || gameLoop == null || timer == null)
            {
                return RejectStart(index, LevelStartFailure.MissingDependencies);
            }

            if (!loader.TryGet(index, out var candidate)) return RejectStart(index, LevelStartFailure.InvalidIndex);
            if (ApplicationBootstrap.Profile != null && !ApplicationBootstrap.Profile.IsUnlocked(index))
            {
                return RejectStart(index, LevelStartFailure.Locked);
            }
            if (!IsPlayable(candidate)) return RejectStart(index, LevelStartFailure.InvalidDefinition);

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

        /// <summary>
        /// The single gameplay-facing interface for a completed Compile action.
        /// </summary>
        public bool SubmitCompilation(CompilationSubmission submission)
        {
            if (submission == null || State != GameState.Playing) return false;
            if (!submission.Succeeded) return true;

            anomalyManager?.AcceptSystemCommands(submission.SystemCommands);
            if (submission.RenderedCanvas == null || completionEvaluator == null || CurrentLevel == null) return true;

            CurrentMatch = completionEvaluator.Evaluate(CurrentLevel, submission.RenderedCanvas);
            gameLoop.SetCurrentMatch(CurrentMatch);
            ApplicationBootstrap.Events?.Publish(new MatchChanged(CurrentMatch));
            if (completionEvaluator.IsComplete(CurrentLevel, CurrentMatch)) gameLoop.Win(CurrentMatch);
            return true;
        }

        /// <summary>
        /// UI passes live editor, overlay and timer callbacks here once the
        /// Level scene has been wired.
        /// </summary>
        public void ConfigureAnomalyContext(AnomalyContext value)
        {
            anomalyContext = value ?? CreateNoOpAnomalyContext();
            if (CurrentLevel != null) anomalyManager?.Initialize(anomalyContext);
        }

        private void OnTimerChanged(float remaining) => ApplicationBootstrap.Events?.Publish(new TimerChanged(remaining));

        private bool RejectStart(int index, LevelStartFailure reason)
        {
            LastStartFailure = reason;
            Debug.LogError($"無法啟動關卡索引 {index}：{reason}。請檢查 LevelDefinition 與場景設定。", this);
            ApplicationBootstrap.Events?.Publish(new LevelStartRejected(index, reason));
            return false;
        }

        private static bool IsPlayable(LevelDefinition level) =>
            level != null && TargetImageLoader.IsValid(level.TargetImage) &&
            level.PassPercentage > 0f && level.TimeLimitSeconds > 0f;

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
