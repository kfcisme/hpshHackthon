using System.Collections.Generic;
using UnityEngine;
namespace GlitchCompiler.Data
{
    [CreateAssetMenu(menuName = "Glitch Compiler/Level Catalog")]
    public sealed class LevelCatalog : ScriptableObject
    {
        public List<LevelDefinition> Levels = new List<LevelDefinition>();
        public int Count => Levels?.Count ?? 0;

        public bool TryGet(int index, out LevelDefinition level)
        {
            level = null;
            if (Levels == null || index < 0 || index >= Levels.Count) return false;
            level = Levels[index];
            return level != null;
        }

        public LevelDefinition Get(int index) => TryGet(index, out var level) ? level : null;
    }
}
