using GlitchCompiler.Core;
using GlitchCompiler.Level;
using GlitchCompiler.Rendering;
using GlitchCompiler.VCode;
using TMPro;
using UnityEngine;

namespace GlitchCompiler.UI
{
    public sealed class IDEEditorController : MonoBehaviour
    {
        [SerializeField] private TMP_InputField input;
        [SerializeField] private CanvasRenderer renderer;
        [SerializeField] private LevelSessionController levelSession;

        public string Code
        {
            get => input == null ? string.Empty : input.text;
            set
            {
                if (input != null)
                {
                    input.text = value;
                }
            }
        }

        public void Compile()
        {
            var parsed = new VCodeParser().Parse(Code);
            ApplicationBootstrap.Events?.Publish(new CompilationFinished(parsed.Diagnostics));
            if (!parsed.Success)
            {
                return;
            }

            var executed = new VCodeInterpreter().Execute(parsed.Program);
            ApplicationBootstrap.Events?.Publish(new CompilationFinished(executed.Diagnostics));
            if (!executed.Success || renderer == null)
            {
                return;
            }

            renderer.Render(executed.DrawCommands);
            levelSession?.SubmitSystemCommands(executed.SystemCommands);
            levelSession?.SubmitRenderedCanvas(renderer.CanvasTexture);
        }

        public void NotifyChanged()
        {
            ApplicationBootstrap.Events?.Publish(new CodeChanged(Code));
        }
    }
}
