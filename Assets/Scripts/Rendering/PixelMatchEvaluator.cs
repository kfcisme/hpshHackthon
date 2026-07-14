using UnityEngine;

namespace GlitchCompiler.Rendering
{
    public static class PixelMatchEvaluator
    {
        private const int ColorTolerance = 36;

        public static float Evaluate(Texture2D rendered, Texture2D target)
        {
            if (rendered == null || !TargetImageLoader.IsValid(target) ||
                rendered.width != target.width || rendered.height != target.height)
            {
                return 0f;
            }

            var renderedPixels = rendered.GetPixels32();
            var targetPixels = target.GetPixels32();
            var targetCount = 0;
            var matchedCount = 0;
            var extraDrawnCount = 0;

            for (var index = 0; index < renderedPixels.Length; index++)
            {
                var renderedPixel = renderedPixels[index];
                var targetPixel = targetPixels[index];
                var playerDrewHere = renderedPixel.a > 0;

                if (targetPixel.a == 0)
                {
                    if (playerDrewHere) extraDrawnCount++;
                    continue;
                }

                targetCount++;
                if (!playerDrewHere) continue;

                var delta = Mathf.Abs(renderedPixel.r - targetPixel.r) +
                            Mathf.Abs(renderedPixel.g - targetPixel.g) +
                            Mathf.Abs(renderedPixel.b - targetPixel.b);
                if (delta <= ColorTolerance) matchedCount++;
            }

            // A target miss is already represented in targetCount. Extra painted
            // pixels increase the denominator, so filling the whole canvas can no
            // longer achieve a high score.
            var comparedPixels = targetCount + extraDrawnCount;
            return comparedPixels == 0 ? 0f : matchedCount / (float)comparedPixels * 100f;
        }
    }
}
