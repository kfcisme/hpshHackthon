using TMPro;
using UnityEngine;
namespace GlitchCompiler.UI { public sealed class HudController:MonoBehaviour { [SerializeField] TMP_Text timer; [SerializeField] TMP_Text match; public void SetTimer(float seconds){timer.text=$"{seconds:0.0}s";} public void SetMatch(float value){match.text=$"{value:0.0}%";} } }
