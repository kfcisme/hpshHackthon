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
        }
    }
}
