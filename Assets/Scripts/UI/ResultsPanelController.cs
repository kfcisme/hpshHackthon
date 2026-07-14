using TMPro;
using UnityEngine;
namespace GlitchCompiler.UI { public sealed class ResultsPanelController:MonoBehaviour { [SerializeField] GameObject panel; [SerializeField] TMP_Text summary; public void Show(bool win,float match){panel.SetActive(true);summary.text=win?$"完成！重合度 {match:0.0}%":$"時間到。最後重合度 {match:0.0}%";} } }
