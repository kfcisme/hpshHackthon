using TMPro;
using UnityEngine;
namespace GlitchCompiler.UI { public sealed class AnomalyOverlayController:MonoBehaviour { [SerializeField] GameObject panel; [SerializeField] TMP_Text title; [SerializeField] TMP_Text instruction; public void Show(string value,string help){panel.SetActive(true);title.text=value;instruction.text=help;} public void Hide()=>panel.SetActive(false); } }
