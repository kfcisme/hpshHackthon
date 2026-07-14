using System.Collections.Generic;
using UnityEngine;
namespace GlitchCompiler.Data { [CreateAssetMenu(menuName="Glitch Compiler/Level Definition")] public sealed class LevelDefinition:ScriptableObject { public string Id; public string Title; [TextArea] public string Tutorial; public Texture2D TargetImage; [TextArea(5,20)] public string StarterCode; public float TimeLimitSeconds=120; [Range(0,100)] public float PassPercentage=95; public List<AnomalyRule> Anomalies=new List<AnomalyRule>(); } }
