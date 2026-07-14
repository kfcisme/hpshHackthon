using System.Collections.Generic;
using GlitchCompiler.VCode;
using TMPro;
using UnityEngine;
namespace GlitchCompiler.UI { public sealed class VCodeErrorPanel:MonoBehaviour { [SerializeField] TMP_Text text; public void Show(IReadOnlyList<VCodeDiagnostic> diagnostics){text.text=diagnostics.Count==0?"編譯完成。":string.Join("\n",System.Linq.Enumerable.Select(diagnostics,x=>$"[{x.Line}:{x.Column}] {x.Message}"));} } }
