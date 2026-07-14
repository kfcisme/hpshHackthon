using System;
using GlitchCompiler.Level;
using UnityEngine;
namespace GlitchCompiler.Data { public enum AnomalyType { GhostComment, SyntaxShift, CanvasMask, ControlInversion } [Serializable] public sealed class AnomalyRule { public AnomalyType Type; public LevelPhase EarliestPhase=LevelPhase.Flow; [Range(0,1)] public float TriggerChance=.5f; public float CooldownSeconds=20; public bool TriggerOnce=true; } }
