using System.Text.RegularExpressions;

namespace GlitchCompiler.Anomalies
{
    public sealed class SyntaxShiftAnomaly : IAnomaly
    {
        private static readonly Regex Turn = new Regex(@"\bTURN\b", RegexOptions.IgnoreCase);
        private static readonly Regex Burn = new Regex(@"\bBURN\b", RegexOptions.IgnoreCase);
        private AnomalyContext context;

        public string Id => "syntax-shift";

        public bool CanTrigger(AnomalyContext value) => value != null && Turn.IsMatch(value.ReadCode());

        public void OnTrigger(AnomalyContext value)
        {
            context = value;
            context.WriteCode(Turn.Replace(context.ReadCode(), "BURN"));
            context.ShowOverlay("語法重載", "使用搜尋與取代，將 BURN 全部改回 TURN。");
        }

        public bool CheckResolved() => !Burn.IsMatch(context.ReadCode());
        public void OnResolve() => context.HideOverlay();
        public void OnCleanup() => context?.HideOverlay();
    }
}
