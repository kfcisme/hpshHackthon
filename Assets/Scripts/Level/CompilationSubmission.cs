using System.Collections.Generic;
using GlitchCompiler.VCode;
using UnityEngine;

namespace GlitchCompiler.Level
{
    /// <summary>
    /// The complete outcome of one Compile button press. This is the only payload
    /// the V-Code/rendering module sends to the level-flow module.
    /// </summary>
    public sealed class CompilationSubmission
    {
        public bool Succeeded { get; }
        public Texture2D RenderedCanvas { get; }
        public IReadOnlyList<SystemCommand> SystemCommands { get; }

        private CompilationSubmission(bool succeeded, Texture2D renderedCanvas, IReadOnlyList<SystemCommand> systemCommands)
        {
            Succeeded = succeeded;
            RenderedCanvas = renderedCanvas;
            SystemCommands = systemCommands ?? System.Array.Empty<SystemCommand>();
        }

        public static CompilationSubmission Failed() => new CompilationSubmission(false, null, System.Array.Empty<SystemCommand>());

        public static CompilationSubmission Successful(Texture2D renderedCanvas, IReadOnlyList<SystemCommand> systemCommands)
        {
            return new CompilationSubmission(true, renderedCanvas, systemCommands);
        }
    }
}
