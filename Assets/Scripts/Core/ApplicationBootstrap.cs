using GlitchCompiler.Progression;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GlitchCompiler.Core
{
    public sealed class ApplicationBootstrap : MonoBehaviour
    {
        public static EventBus Events { get; private set; }
        public static PlayerProfileManager Profile { get; private set; }

        [SerializeField] private string mainMenuScene = "MainMenu";
        [SerializeField] private string levelScene = "Level";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Events = null;
            Profile = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeServices()
        {
            if (Events != null) return;
            Events = new EventBus();
            Profile = new PlayerProfileManager(new SaveDataRepository());
            Profile.Load();
        }

        private void Awake()
        {
            InitializeServices();
        }

        public void OpenMainMenu() => LoadScene(mainMenuScene);
        public void OpenLevel() => LoadScene(levelScene);

        private static void LoadScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError($"無法載入場景 '{sceneName}'。請在 Build Settings 加入場景，並在 Application Bootstrap 指定正確名稱。");
                return;
            }

            SceneManager.LoadScene(sceneName);
        }
    }
}
