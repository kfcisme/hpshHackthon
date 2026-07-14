using UnityEngine;

namespace GlitchCompiler.Rendering
{
    public enum DrawCommandType { Move, Turn, Color, Width, Circle, Rect }
    public readonly struct DrawCommand { public readonly DrawCommandType Type; public readonly float A; public readonly float B; public readonly Color Color; public DrawCommand(DrawCommandType type,float a=0,float b=0,Color color=default) { Type=type; A=a; B=b; Color=color; } }
}
