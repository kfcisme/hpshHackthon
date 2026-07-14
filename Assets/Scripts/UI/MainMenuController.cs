using GlitchCompiler.Core;
using UnityEngine;
namespace GlitchCompiler.UI { public sealed class MainMenuController:MonoBehaviour { [SerializeField] ApplicationBootstrap bootstrap; public void StartGame()=>bootstrap.OpenLevel(); public void Quit()=>Application.Quit(); } }
