namespace GlitchCompiler.VCode
{
    public enum SystemCommandType { Shield, Reset }
    public readonly struct SystemCommand { public readonly SystemCommandType Type; public readonly bool BoolValue; public SystemCommand(SystemCommandType type,bool boolValue=false) { Type=type; BoolValue=boolValue; } }
}
