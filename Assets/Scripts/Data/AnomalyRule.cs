using System;
using GlitchCompiler.Level;
using UnityEngine;
namespace GlitchCompiler.Data
{
    public enum AnomalyType { GhostComment, SyntaxShift, CanvasMask, ControlInversion }

    [Serializable]
    public sealed class AnomalyRule
    {
        public bool Enabled = true;
        public AnomalyType Type;
        public LevelPhase EarliestPhase = LevelPhase.Flow;
        [Range(0f, 1f)] public float TriggerChance = .5f;
        [Min(0f)] public float CooldownSeconds = 20f;
        public bool TriggerOnce = true;
        [Min(0f)] public float ResolveBonusSeconds = 10f;
        [Min(0f)] public float TimerMultiplier = 2f;
    }
}
