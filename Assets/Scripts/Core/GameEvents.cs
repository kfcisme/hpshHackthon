using System.Collections.Generic;
using GlitchCompiler.Level;
using GlitchCompiler.VCode;

namespace GlitchCompiler.Core
{
    public struct CodeChanged { public string Source; public CodeChanged(string source) { Source = source; } }
    public struct LevelStarted { public string LevelId; public int Index; public LevelStarted(string levelId, int index) { LevelId = levelId; Index = index; } }
    public struct LevelStartRejected { public int Index; public LevelStartFailure Reason; public LevelStartRejected(int index, LevelStartFailure reason) { Index = index; Reason = reason; } }
    public struct LevelPhaseChanged { public LevelPhase Phase; public LevelPhaseChanged(LevelPhase phase) { Phase = phase; } }
    public struct TimerChanged { public float Remaining; public TimerChanged(float remaining) { Remaining = remaining; } }
    public struct CompilationFinished { public IReadOnlyList<VCodeDiagnostic> Diagnostics; public CompilationFinished(IReadOnlyList<VCodeDiagnostic> diagnostics) { Diagnostics = diagnostics; } }
    public struct MatchChanged { public float Percentage; public MatchChanged(float percentage) { Percentage = percentage; } }
    public struct AnomalyTriggered { public string Id; public AnomalyTriggered(string id) { Id = id; } }
    public struct AnomalyResolved { public string Id; public AnomalyResolved(string id) { Id = id; } }
    public struct LevelFinished { public bool Success; public float Match; public LevelFinished(bool success, float match) { Success = success; Match = match; } }
}
