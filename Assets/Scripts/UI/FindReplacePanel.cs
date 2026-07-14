using TMPro;
using UnityEngine;
namespace GlitchCompiler.UI { public sealed class FindReplacePanel:MonoBehaviour { [SerializeField] IDEEditorController editor; [SerializeField] TMP_InputField find; [SerializeField] TMP_InputField replace; public void ReplaceAll(){editor.Code=editor.Code.Replace(find.text,replace.text);editor.NotifyChanged();} } }
