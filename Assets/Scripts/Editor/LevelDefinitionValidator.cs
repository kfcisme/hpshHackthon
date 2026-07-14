#if UNITY_EDITOR
using System.IO;
using GlitchCompiler.Data;
using GlitchCompiler.Rendering;
using GlitchCompiler.VCode;
using UnityEditor;
using UnityEngine;

namespace GlitchCompiler.Editor
{
    [CustomEditor(typeof(LevelDefinition))]
    public sealed class LevelDefinitionValidator : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var level = (LevelDefinition)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("關卡驗證", EditorStyles.boldLabel);
            if (!TargetImageLoader.IsValid(level.TargetImage))
            {
                EditorGUILayout.HelpBox("目標圖必須為可讀取（Read/Write Enabled）的 64×64 Texture2D。", MessageType.Error);
            }
            if (level.PassPercentage <= 0f)
            {
                EditorGUILayout.HelpBox("通關門檻必須大於 0，否則任何成功編譯都會自動通關。", MessageType.Error);
            }
            if (level.TimeLimitSeconds <= 0f)
            {
                EditorGUILayout.HelpBox("時限必須大於 0，否則關卡會立即失敗。", MessageType.Error);
            }
            if (string.IsNullOrWhiteSpace(level.Id))
            {
                EditorGUILayout.HelpBox("關卡需要唯一 ID。", MessageType.Warning);
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("從 Starter Code 產生 64×64 目標圖")) GenerateTarget(level);
        }

        private static void GenerateTarget(LevelDefinition level)
        {
            var parsed = new VCodeParser().Parse(level.StarterCode ?? string.Empty);
            if (!parsed.Success)
            {
                EditorUtility.DisplayDialog("無法產生目標圖", "Starter Code 有語法錯誤：\n" + Diagnostics(parsed.Diagnostics), "了解");
                return;
            }

            var executed = new VCodeInterpreter().Execute(parsed.Program);
            if (!executed.Success)
            {
                EditorUtility.DisplayDialog("無法產生目標圖", "Starter Code 執行失敗：\n" + Diagnostics(executed.Diagnostics), "了解");
                return;
            }

            const string generatedFolder = "Assets/Levels/Generated";
            if (!AssetDatabase.IsValidFolder("Assets/Levels")) AssetDatabase.CreateFolder("Assets", "Levels");
            if (!AssetDatabase.IsValidFolder(generatedFolder)) AssetDatabase.CreateFolder("Assets/Levels", "Generated");

            var safeId = string.IsNullOrWhiteSpace(level.Id) ? "level" : SanitizeFileName(level.Id);
            var path = AssetDatabase.GenerateUniqueAssetPath($"{generatedFolder}/{safeId}_Target.png");
            var rasterizer = new TurtleRasterizer(TargetImageLoader.CanvasResolution);
            rasterizer.Render(executed.DrawCommands);
            var texture = new Texture2D(TargetImageLoader.CanvasResolution, TargetImageLoader.CanvasResolution, TextureFormat.RGBA32, false);
            try
            {
                texture.SetPixels32(rasterizer.Pixels);
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.isReadable = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            level.TargetImage = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            EditorUtility.SetDirty(level);
            AssetDatabase.SaveAssets();
            Selection.activeObject = level.TargetImage;
            EditorGUIUtility.PingObject(level.TargetImage);
        }

        private static string Diagnostics(System.Collections.Generic.List<VCodeDiagnostic> diagnostics)
        {
            return string.Join("\n", diagnostics.ConvertAll(diagnostic => $"{diagnostic.Line}:{diagnostic.Column} {diagnostic.Message}"));
        }

        private static string SanitizeFileName(string value)
        {
            foreach (var invalid in Path.GetInvalidFileNameChars()) value = value.Replace(invalid, '_');
            return value;
        }
    }
}
#endif
