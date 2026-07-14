using GlitchCompiler.Progression;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GlitchCompiler.Core
{
    public sealed class ApplicationBootstrap : MonoBehaviour
    {
        public static EventBus Events { get; private set; }
        public static PlayerProfileManager Profile { get; private set; }
        private void Awake()
        {
            if (Events != null) { Destroy(gameObject); return; }
            DontDestroyOnLoad(gameObject);
            Events = new EventBus();
            Profile = new PlayerProfileManager(new SaveDataRepository());
            Profile.Load();
        }
        public void OpenMainMenu() => SceneManager.LoadScene("MainMenu");
        public void OpenLevel() => SceneManager.LoadScene("Level");
    }
}
