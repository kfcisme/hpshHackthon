using System.Collections.Generic;
using UnityEngine;
namespace GlitchCompiler.Data { [CreateAssetMenu(menuName="Glitch Compiler/Level Catalog")] public sealed class LevelCatalog:ScriptableObject { public List<LevelDefinition> Levels=new List<LevelDefinition>(); public LevelDefinition Get(int index)=>index>=0&&index<Levels.Count?Levels[index]:null; } }
