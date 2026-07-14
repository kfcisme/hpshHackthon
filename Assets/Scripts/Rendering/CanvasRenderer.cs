using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GlitchCompiler.Rendering
{
    public sealed class CanvasRenderer : MonoBehaviour
    {
        [SerializeField] private RawImage preview;

        private TurtleRasterizer rasterizer;

        public Texture2D CanvasTexture { get; private set; }

        public void Render(IReadOnlyList<DrawCommand> commands)
        {
            EnsureSurface();
            rasterizer.Render(commands);
            CanvasTexture.SetPixels32(rasterizer.Pixels);
            CanvasTexture.Apply(false, false);

            if (preview != null)
            {
                preview.texture = CanvasTexture;
            }
        }

        private void EnsureSurface()
        {
            if (rasterizer == null)
            {
                rasterizer = new TurtleRasterizer(TargetImageLoader.CanvasResolution);
            }

            if (CanvasTexture == null || CanvasTexture.width != TargetImageLoader.CanvasResolution || CanvasTexture.height != TargetImageLoader.CanvasResolution)
            {
                if (CanvasTexture != null)
                {
                    Destroy(CanvasTexture);
                }

                CanvasTexture = new Texture2D(TargetImageLoader.CanvasResolution, TargetImageLoader.CanvasResolution, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                };
            }
        }

        private void OnDestroy()
        {
            if (CanvasTexture != null)
            {
                Destroy(CanvasTexture);
            }
        }
    }
}
