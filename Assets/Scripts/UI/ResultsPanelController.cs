using TMPro;
using UnityEngine;
namespace GlitchCompiler.UI { public sealed class ResultsPanelController:MonoBehaviour { [SerializeField] GameObject panel; [SerializeField] TMP_Text summary; public void Show(bool win,float match){if(panel!=null)panel.SetActive(true);if(summary!=null)summary.text=win?$"完成！重合度 {match:0.0}%":$"時間到。最後重合度 {match:0.0}%";} public void Hide(){if(panel!=null)panel.SetActive(false);} } }
