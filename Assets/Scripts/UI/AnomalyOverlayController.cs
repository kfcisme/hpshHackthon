using TMPro;
using UnityEngine;
namespace GlitchCompiler.UI
{
    public sealed class AnomalyOverlayController : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text instruction;

        public void Show(string value, string help)
        {
            if (panel != null) panel.SetActive(true);
            if (title != null) title.text = value;
            if (instruction != null) instruction.text = help;
        }

        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
        }
    }
}
