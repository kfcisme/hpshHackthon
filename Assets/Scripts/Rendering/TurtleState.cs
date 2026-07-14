using UnityEngine;

namespace GlitchCompiler.Rendering
{
    public struct TurtleState
    {
        public Vector2 Position;
        public float Angle;
        public Color Color;
        public int Width;

        public static TurtleState CreateDefault(int resolution) => new TurtleState
        {
            Position = new Vector2(resolution * 0.5f, resolution * 0.5f),
            Angle = 0,
            Color = Color.white,
            Width = 2
        };
    }
}
