using UnityEngine;
namespace GlitchCompiler.Data { [CreateAssetMenu(menuName="Glitch Compiler/V-Code Language Config")] public sealed class VCodeLanguageConfig:ScriptableObject { public int MaxRecursionDepth=32; public int MaxInstructions=10000; public string[] NamedColors={"white","red","green","blue"}; } }
