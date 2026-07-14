using System;
using System.Collections.Generic;
namespace GlitchCompiler.Progression { [Serializable] public sealed class LevelRecord { public string LevelId; public float BestMatch; public float BestSeconds; } [Serializable] public sealed class PlayerData { public int Version=1; public int HighestUnlocked=0; public List<LevelRecord> Records=new List<LevelRecord>(); } }
