using System.IO;
using UnityEngine;
namespace GlitchCompiler.Progression { public sealed class SaveDataRepository { private string Path => System.IO.Path.Combine(Application.persistentDataPath,"player-profile.json"); public PlayerData Load(){try{return File.Exists(Path)?JsonUtility.FromJson<PlayerData>(File.ReadAllText(Path))??new PlayerData():new PlayerData();}catch{return new PlayerData();}} public void Save(PlayerData data){var temp=Path+".tmp";File.WriteAllText(temp,JsonUtility.ToJson(data,true));if(File.Exists(Path))File.Delete(Path);File.Move(temp,Path);} } }
