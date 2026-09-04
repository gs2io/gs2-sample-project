using System;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Gs2.Sample.Editor
{
    [InitializeOnLoad]
    public static class Gs2SampleKoreanFontSetup
    {
        /// <summary>
        /// 再配布可能な韓国語フォント（SIL Open Font License 1.1）
        /// Redistributable Korean font (SIL Open Font License 1.1)
        /// </summary>
        private const string KoreanFontAssetPath = "Assets/Resources/Fonts/NotoSansKR-Medium SDF.asset";

        internal const string TmpSettingsFileName = "TMP Settings.asset";
        private const string EssentialResourcesName = "TMP Essential Resources";
        private const string FallbackPropertyName = "m_fallbackFontAssets";

        static Gs2SampleKoreanFontSetup()
        {
            // アセットの読み込みが終わってから処理する
            EditorApplication.delayCall += Setup;
        }

        /// <summary>
        /// TMP Essentials のインポート直後に呼ばれる
        /// Called right after the TMP essential resources have been imported
        /// </summary>
        internal static void ApplyOnImport()
        {
            Apply(false);
        }

        [MenuItem("GS2 Sample/Setup Korean Font Fallback")]
        private static void ApplyFromMenu()
        {
            if (FindTmpSettings() == null)
            {
                ImportEssentialResources();
                return;
            }

            Apply(true);
        }

        private static void Setup()
        {
            // TMP Essentials が無いとテキストがまったく描画できないため、まず取り込む
            if (FindTmpSettings() == null)
            {
                ImportEssentialResources();
                return;
            }

            // このプロジェクトで一度も設定していない場合にだけ、起動時に設定を試みる
            // 利用者が意図的に Fallback を外した場合に、毎回書き戻してしまわないようにする
            if (SetupCompleted)
                return;

            Apply(false);
        }

        // ------------------------------------------------------------------
        // TMP Essentials の取り込み / Importing the essential resources
        // ------------------------------------------------------------------

        /// <summary>
        /// TextMeshPro の必須リソースを取り込む
        /// Window > TextMeshPro > Import TMP Essential Resources と同じ処理
        /// </summary>
        private static void ImportEssentialResources()
        {
            Debug.Log("[GS2 Sample] Importing the " + EssentialResourcesName + "...");

            AssetDatabase.importPackageCompleted -= OnPackageImported;
            AssetDatabase.importPackageCompleted += OnPackageImported;

            TMP_PackageResourceImporter.ImportResources(true, false, false);
        }

        private static void OnPackageImported(string packageName)
        {
            if (packageName != EssentialResourcesName)
                return;

            AssetDatabase.importPackageCompleted -= OnPackageImported;

            // TMP Settings を読めずに待機しているテキストへ、読み込めるようになったことを知らせる
            // これを呼ばないと、先に開かれていたシーンのテキストが描画されないままになる
            TMPro_EventManager.ON_RESOURCES_LOADED();

            EditorApplication.delayCall += () => Apply(false);
        }

        // ------------------------------------------------------------------
        // Fallback の登録 / Registering the fallback
        // ------------------------------------------------------------------

        /// <summary>
        /// 「一度設定した」ことをプロジェクト単位で覚えておくためのフラグ
        /// </summary>
        private static bool SetupCompleted
        {
            get { return EditorPrefs.GetBool(SetupCompletedKey, false); }
            set { EditorPrefs.SetBool(SetupCompletedKey, value); }
        }

        private static string SetupCompletedKey
        {
            get { return "Gs2Sample.KoreanFontFallback." + Application.dataPath; }
        }

        /// <summary>
        /// TMP Settings の Fallback Font Assets に韓国語フォントを追加する
        /// </summary>
        /// <param name="verbose">メニューからの実行時など、何もしなかった理由もログに出す</param>
        private static void Apply(bool verbose)
        {
            var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(KoreanFontAssetPath);
            if (fontAsset == null)
            {
                if (verbose)
                    Debug.LogWarning("[GS2 Sample] Korean font asset not found: " + KoreanFontAssetPath);
                return;
            }

            var settings = FindTmpSettings();
            if (settings == null)
            {
                if (verbose)
                    Debug.LogWarning(
                        "[GS2 Sample] TMP Settings not found. " +
                        "Run Window > TextMeshPro > Import TMP Essential Resources.");
                return;
            }

            var serialized = new SerializedObject(settings);
            var fallbacks = serialized.FindProperty(FallbackPropertyName);
            if (fallbacks == null)
            {
                if (verbose)
                    Debug.LogWarning("[GS2 Sample] " + FallbackPropertyName + " not found in TMP Settings.");
                return;
            }

            for (var i = 0; i < fallbacks.arraySize; i++)
            {
                if (fallbacks.GetArrayElementAtIndex(i).objectReferenceValue != fontAsset)
                    continue;

                if (verbose)
                    Debug.Log("[GS2 Sample] " + fontAsset.name + " is already registered in the Fallback Font Assets.");

                SetupCompleted = true;
                return;
            }

            fallbacks.arraySize++;
            fallbacks.GetArrayElementAtIndex(fallbacks.arraySize - 1).objectReferenceValue = fontAsset;
            serialized.ApplyModifiedProperties();

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssetIfDirty(settings);

            SetupCompleted = true;

            Debug.Log(
                "[GS2 Sample] Registered " + fontAsset.name +
                " in the Fallback Font Assets of TMP Settings.");
        }

        private static TMP_Settings FindTmpSettings()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:TMP_Settings"))
            {
                var settings = AssetDatabase.LoadAssetAtPath<TMP_Settings>(AssetDatabase.GUIDToAssetPath(guid));
                if (settings != null)
                    return settings;
            }

            return null;
        }
    }

    internal class Gs2SampleTmpSettingsPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (var path in importedAssets)
            {
                if (!path.EndsWith(Gs2SampleKoreanFontSetup.TmpSettingsFileName, StringComparison.Ordinal))
                    continue;

                // インポート処理の最中はアセットを書き換えられないため、次のタイミングまで待つ
                EditorApplication.delayCall += Gs2SampleKoreanFontSetup.ApplyOnImport;
                return;
            }
        }
    }
}
