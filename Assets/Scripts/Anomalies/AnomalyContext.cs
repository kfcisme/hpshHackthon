using System;
using GlitchCompiler.Data;

namespace GlitchCompiler.Anomalies
{
    public sealed class AnomalyContext
    {
        public Func<string> ReadCode;
        public Action<string> WriteCode;
        public Action<string, string> ShowOverlay;
        public Action HideOverlay;
        public Action<float> AddTime;
        public Action<float> SetTimerMultiplier;
        public Func<bool> ShieldEnabled;
        public Func<bool> ResetReceived;
        public AnomalyRule ActiveRule { get; internal set; }
        public float TimerMultiplier => ActiveRule == null ? 1f : ActiveRule.TimerMultiplier;
    }
}
