using GlitchCompiler.Core;
using GlitchCompiler.Level;
using GlitchCompiler.VCode;
using TMPro;
using UnityEngine;
using GameCanvasRenderer = GlitchCompiler.Rendering.CanvasRenderer;

namespace GlitchCompiler.UI
{
    public sealed class IDEEditorController : MonoBehaviour
    {
        [SerializeField] private TMP_InputField input;
        [SerializeField] private GameCanvasRenderer renderer;
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
                levelSession?.SubmitCompilation(CompilationSubmission.Failed());
                return;
            }

            var executed = new VCodeInterpreter().Execute(parsed.Program);
            ApplicationBootstrap.Events?.Publish(new CompilationFinished(executed.Diagnostics));
            if (!executed.Success)
            {
                levelSession?.SubmitCompilation(CompilationSubmission.Failed());
                return;
            }

            if (renderer == null)
            {
                levelSession?.SubmitCompilation(CompilationSubmission.Successful(null, executed.SystemCommands));
                return;
            }

            renderer.Render(executed.DrawCommands);
            levelSession?.SubmitCompilation(CompilationSubmission.Successful(renderer.CanvasTexture, executed.SystemCommands));
        }

        public void NotifyChanged()
        {
            ApplicationBootstrap.Events?.Publish(new CodeChanged(Code));
        }
    }
}
