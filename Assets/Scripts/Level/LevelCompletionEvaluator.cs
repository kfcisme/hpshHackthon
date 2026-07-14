using GlitchCompiler.Data;
using GlitchCompiler.Rendering;
using UnityEngine;

namespace GlitchCompiler.Level
{
    public sealed class LevelCompletionEvaluator : MonoBehaviour
    {
        public float Evaluate(LevelDefinition level, Texture2D rendered) =>
            level == null ? 0f : PixelMatchEvaluator.Evaluate(rendered, level.TargetImage);

        public bool IsComplete(LevelDefinition level, float match) =>
            level != null && TargetImageLoader.IsValid(level.TargetImage) &&
            level.PassPercentage > 0f && match >= level.PassPercentage;
    }
}
