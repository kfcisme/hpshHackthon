using GlitchCompiler.Data;
using UnityEngine;

namespace GlitchCompiler.Level
{
    public sealed class LevelLoader : MonoBehaviour
    {
        [SerializeField] private LevelCatalog catalog;

        public LevelDefinition Current { get; private set; }
        public int CurrentIndex { get; private set; } = -1;

        public bool Load(int index)
        {
            Current = catalog == null ? null : catalog.Get(index);
            CurrentIndex = Current == null ? -1 : index;
            return Current != null;
        }
    }
}
