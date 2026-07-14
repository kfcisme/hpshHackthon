using UnityEngine;
namespace GlitchCompiler.Rendering { public static class TargetImageLoader { public static bool IsValid(Texture2D target) => target!=null&&target.width==512&&target.height==512; } }
