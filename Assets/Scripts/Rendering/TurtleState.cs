using UnityEngine;
namespace GlitchCompiler.Rendering { public struct TurtleState { public Vector2 Position; public float Angle; public Color Color; public int Width; public static TurtleState Default => new TurtleState { Position=new Vector2(256,256),Angle=0,Color=Color.white,Width=2 }; } }
