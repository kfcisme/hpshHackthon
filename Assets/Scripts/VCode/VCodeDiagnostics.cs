using System.Collections.Generic;

namespace GlitchCompiler.VCode
{
    public sealed class VCodeDiagnostic { public int Line; public int Column; public string Message; public VCodeDiagnostic(int line, int column, string message) { Line=line; Column=column; Message=message; } }
    public sealed class VCodeParseResult { public ProgramNode Program; public List<VCodeDiagnostic> Diagnostics = new List<VCodeDiagnostic>(); public bool Success => Diagnostics.Count == 0 && Program != null; }
}
