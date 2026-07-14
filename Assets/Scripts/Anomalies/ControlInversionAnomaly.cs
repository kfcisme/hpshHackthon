namespace GlitchCompiler.Anomalies
{
    // The legacy enum name is retained for existing level assets. Its implemented
    // effect is an accelerated countdown, not input inversion.
    public sealed class ControlInversionAnomaly : IAnomaly
    {
        private AnomalyContext context;
        public string Id => "control-inversion";
        public bool CanTrigger(AnomalyContext value) => value != null;
        public void OnTrigger(AnomalyContext value)
        {
            context = value;
            context.SetTimerMultiplier(context.TimerMultiplier);
            context.ShowOverlay("倒數失控", "輸入 SYSTEM.RESET(); 讓倒數恢復正常。");
        }
        public bool CheckResolved() => context.ResetReceived();
        public void OnResolve()
        {
            context.SetTimerMultiplier(1f);
            context.HideOverlay();
        }
        public void OnCleanup()
        {
            context?.SetTimerMultiplier(1f);
            context?.HideOverlay();
        }
    }
}
