using GlitchCompiler.Core;
using GlitchCompiler.Data;
using UnityEngine;
namespace GlitchCompiler.Level
{
    public sealed class GameLoopController : MonoBehaviour
    {
        [SerializeField] private LevelTimer timer;
        public GameState State { get; private set; } = GameState.Preparing;
        public LevelPhase Phase { get; private set; } = LevelPhase.Safe;

        private void Awake()
        {
            if (timer != null) timer.Expired += Lose;
        }

        private void OnDestroy()
        {
            if (timer != null) timer.Expired -= Lose;
        }

        public void Begin(LevelDefinition level)
        {
            if (level == null || timer == null) return;
            State = GameState.Playing;
            Phase = LevelPhase.Safe;
            timer.StartTimer(level.TimeLimitSeconds);
            ApplicationBootstrap.Events?.Publish(new LevelPhaseChanged(Phase));
        }

        public bool Pause()
        {
            if (State != GameState.Playing || timer == null) return false;
            State = GameState.Paused;
            timer.Pause(true);
            return true;
        }

        public bool Resume()
        {
            if (State != GameState.Paused || timer == null) return false;
            State = GameState.Playing;
            timer.Pause(false);
            return true;
        }

        public void UpdatePhase(float normalizedElapsed)
        {
            if (State != GameState.Playing) return;
            var next = normalizedElapsed < .33f ? LevelPhase.Safe : normalizedElapsed < .7f ? LevelPhase.Flow : LevelPhase.Crisis;
            if (next == Phase) return;
            Phase = next;
            ApplicationBootstrap.Events?.Publish(new LevelPhaseChanged(Phase));
        }

        public void Win(float match)
        {
            if (State != GameState.Playing) return;
            State = GameState.Won;
            timer.Pause(true);
            ApplicationBootstrap.Events?.Publish(new LevelFinished(true, match));
        }

        public void Lose()
        {
            if (State != GameState.Playing) return;
            State = GameState.Lost;
            ApplicationBootstrap.Events?.Publish(new LevelFinished(false, 0));
        }
    }
}
