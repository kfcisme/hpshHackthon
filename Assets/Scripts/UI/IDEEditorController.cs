using GlitchCompiler.Core;
using GlitchCompiler.Rendering;
using GlitchCompiler.VCode;
using TMPro;
using UnityEngine;
namespace GlitchCompiler.UI { public sealed class IDEEditorController:MonoBehaviour { [SerializeField] TMP_InputField input; [SerializeField] GlitchCompiler.Rendering.CanvasRenderer renderer; public string Code { get=>input.text; set=>input.text=value; } public void Compile(){var parsed=new VCodeParser().Parse(Code);ApplicationBootstrap.Events.Publish(new CompilationFinished(parsed.Diagnostics));if(!parsed.Success)return;var executed=new VCodeInterpreter().Execute(parsed.Program);ApplicationBootstrap.Events.Publish(new CompilationFinished(executed.Diagnostics));if(executed.Success)renderer.Render(executed.DrawCommands);} public void NotifyChanged()=>ApplicationBootstrap.Events.Publish(new CodeChanged(Code)); } }
