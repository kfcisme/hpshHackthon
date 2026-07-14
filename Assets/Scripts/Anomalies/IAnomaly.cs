namespace GlitchCompiler.Anomalies { public interface IAnomaly { string Id { get; } void OnTrigger(AnomalyContext context); bool CheckResolved(); void OnResolve(); void OnCleanup(); } }
