using UnityEngine;
namespace GlitchCompiler.Rendering
{
    public static class TargetImageLoader
    {
        public const int CanvasResolution = 64;

        public static bool IsValid(Texture2D target) =>
            target != null && target.isReadable &&
            target.width == CanvasResolution && target.height == CanvasResolution;
    }
}
