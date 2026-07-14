namespace GlitchCompiler.Anomalies
{
    public interface IAnomaly
    {
        string Id { get; }
        bool CanTrigger(AnomalyContext context);
        void OnTrigger(AnomalyContext context);
        bool CheckResolved();
        void OnResolve();
        void OnCleanup();
    }
}
