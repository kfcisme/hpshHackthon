using System.Collections.Generic;
using GlitchCompiler.VCode;

namespace GlitchCompiler.Core
{
    public struct CodeChanged { public string Source; public CodeChanged(string source) { Source = source; } }
    public struct CompilationFinished { public IReadOnlyList<VCodeDiagnostic> Diagnostics; public CompilationFinished(IReadOnlyList<VCodeDiagnostic> diagnostics) { Diagnostics = diagnostics; } }
    public struct MatchChanged { public float Percentage; public MatchChanged(float percentage) { Percentage = percentage; } }
    public struct AnomalyTriggered { public string Id; public AnomalyTriggered(string id) { Id = id; } }
    public struct AnomalyResolved { public string Id; public AnomalyResolved(string id) { Id = id; } }
    public struct LevelFinished { public bool Success; public float Match; public LevelFinished(bool success, float match) { Success = success; Match = match; } }
}
