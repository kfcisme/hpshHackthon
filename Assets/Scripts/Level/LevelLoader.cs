using GlitchCompiler.Data;
using UnityEngine;
namespace GlitchCompiler.Level { public sealed class LevelLoader:MonoBehaviour { [SerializeField] private LevelCatalog catalog; public LevelDefinition Current { get; private set; } public bool Load(int index) { Current=catalog==null?null:catalog.Get(index); return Current!=null; } } }
