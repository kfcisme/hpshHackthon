using System.IO;
using UnityEngine;

namespace GlitchCompiler.Progression
{
    public sealed class SaveDataRepository
    {
        private string Path => System.IO.Path.Combine(Application.persistentDataPath, "player-profile.json");

        public PlayerData Load()
        {
            try
            {
                return File.Exists(Path) ? JsonUtility.FromJson<PlayerData>(File.ReadAllText(Path)) ?? new PlayerData() : new PlayerData();
            }
            catch
            {
                return new PlayerData();
            }
        }

        public void Save(PlayerData data)
        {
            var temporaryPath = Path + ".tmp";
            try
            {
                File.WriteAllText(temporaryPath, JsonUtility.ToJson(data, true));
                if (File.Exists(Path))
                {
                    File.Replace(temporaryPath, Path, null);
                }
                else
                {
                    File.Move(temporaryPath, Path);
                }
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"無法儲存玩家進度：{exception.Message}");
            }
            finally
            {
                if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
            }
        }
    }
}
