using UnityEngine;
namespace GlitchCompiler.UI { public sealed class InputRouter:MonoBehaviour { public bool Inverted { get; set; } public bool ConfirmPressed()=>Input.GetKeyDown(Inverted?KeyCode.Escape:KeyCode.Return); } }
