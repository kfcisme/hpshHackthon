using GlitchCompiler.Data;
using UnityEngine;
namespace GlitchCompiler.Level
{
    public sealed class LevelLoader : MonoBehaviour
    {
        [SerializeField] private LevelCatalog catalog;
        public LevelDefinition Current { get; private set; }
        public int CurrentIndex { get; private set; } = -1;
        public int Count => catalog == null ? 0 : catalog.Count;

        public bool CanLoad(int index) => catalog != null && catalog.TryGet(index, out _);

        // Invalid requests deliberately keep the current level intact.
        public bool Load(int index)
        {
            if (catalog == null || !catalog.TryGet(index, out var next)) return false;
            Current = next;
            CurrentIndex = index;
            return true;
        }
    }
}
