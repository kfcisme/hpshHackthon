namespace GlitchCompiler.Anomalies
{
    public sealed class GhostCommentAnomaly : IAnomaly
    {
        private const string Marker = "// [干擾註解：請移除這一行]";
        private AnomalyContext context;

        public string Id => "ghost-comment";

        public bool CanTrigger(AnomalyContext value) => value != null && !value.ReadCode().Contains(Marker);

        public void OnTrigger(AnomalyContext value)
        {
            context = value;
            context.WriteCode(context.ReadCode() + "\n" + Marker);
            context.ShowOverlay("干擾註解", "刪除含有干擾註解的文字。");
        }

        public bool CheckResolved() => !context.ReadCode().Contains(Marker);
        public void OnResolve() => context.HideOverlay();

        // Never overwrite the player's editor contents during session cleanup.
        public void OnCleanup() => context?.HideOverlay();
    }
}
