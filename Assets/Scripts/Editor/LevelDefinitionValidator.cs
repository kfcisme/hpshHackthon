#if UNITY_EDITOR
using GlitchCompiler.Data;
using GlitchCompiler.Rendering;
using UnityEditor;
using UnityEngine;
namespace GlitchCompiler.Editor { [CustomEditor(typeof(LevelDefinition))] public sealed class LevelDefinitionValidator:UnityEditor.Editor { public override void OnInspectorGUI(){DrawDefaultInspector();var level=(LevelDefinition)target;if(level.TargetImage!=null&&!TargetImageLoader.IsValid(level.TargetImage))EditorGUILayout.HelpBox("目標圖必須為 512×512。",MessageType.Error);if(string.IsNullOrWhiteSpace(level.Id))EditorGUILayout.HelpBox("關卡需要唯一 ID。",MessageType.Warning);} } }
#endif
