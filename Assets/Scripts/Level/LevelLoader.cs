using GlitchCompiler.Data;
using UnityEngine;

namespace GlitchCompiler.Level
{
    public sealed class LevelLoader : MonoBehaviour
    {
        [SerializeField] private LevelCatalog catalog;

        public LevelDefinition Current { get; private set; }
        public int CurrentIndex { get; private set; } = -1;

        public bool CanLoad(int index) => TryGet(index, out _);

        public bool TryGet(int index, out LevelDefinition level)
        {
            level = null;
            return catalog != null && catalog.TryGet(index, out level);
        }

        public bool Load(int index)
        {
            if (!CanLoad(index)) return false;
            if (!TryGet(index, out var level)) return false;
            Current = level;
            CurrentIndex = index;
            return true;
        }
    }
}
