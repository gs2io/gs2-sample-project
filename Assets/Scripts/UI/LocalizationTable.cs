using System;
using System.Collections.Generic;
using System.Globalization;
using Gs2.Util.LitJson;
using UnityEngine;

namespace Gs2.Sample
{
    /// <summary>
    /// 言語ごとの文言テーブル
    /// Assets/Resources/Localization/{言語コード}.json を読み込む
    /// 言語を追加するときは対応する json を 1 枚追加するだけでよい
    ///
    /// Localized text table.
    /// Loads Assets/Resources/Localization/{language code}.json.
    /// To support a new language, just add one json file.
    /// </summary>
    public class LocalizationTable
    {
        private const string ResourceDirectory = "Localization/";

        /// <summary>
        /// 翻訳が見つからなかったときに使用する言語
        /// Language used when no translation is found
        /// </summary>
        public const UIManager.Language FallbackLanguage = UIManager.Language.ja;

        private readonly Dictionary<UIManager.Language, Dictionary<string, string>> _tables =
            new Dictionary<UIManager.Language, Dictionary<string, string>>();

        /// <summary>
        /// 指定した言語のテーブルを読み込む
        /// 読み込み済みの場合は何もしない
        /// </summary>
        public void Load(UIManager.Language lang)
        {
            LoadTable(lang);
            LoadTable(FallbackLanguage);
        }

        private void LoadTable(UIManager.Language lang)
        {
            if (_tables.ContainsKey(lang))
                return;

            // json が存在しない言語でもフォールバックで動作させるため、空のテーブルを登録しておく
            // Register an empty table so that a language without json still works via fallback
            var table = new Dictionary<string, string>();
            _tables[lang] = table;

            var asset = Resources.Load<TextAsset>(ResourceDirectory + lang);
            if (asset == null)
            {
                Debug.LogWarning($"[Localization] Assets/Resources/{ResourceDirectory}{lang}.json is not found.");
                return;
            }

            try
            {
                var json = JsonMapper.ToObject(asset.text);
                foreach (string key in json.Keys)
                {
                    table[key] = (string)json[key];
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[Localization] failed to parse {ResourceDirectory}{lang}.json : {e.Message}");
            }
        }

        /// <summary>
        /// キーに対応する文言を取得する
        /// 翻訳が無い場合はフォールバック言語、それも無い場合はキーをそのまま返す
        /// </summary>
        public string Get(string key, UIManager.Language lang)
        {
            if (string.IsNullOrEmpty(key))
                return key;

            Load(lang);

            if (_tables[lang].TryGetValue(key, out var text) && !string.IsNullOrEmpty(text))
                return text;

            if (_tables[FallbackLanguage].TryGetValue(key, out var fallback) && !string.IsNullOrEmpty(fallback))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[Localization] missing translation. key:{key} lang:{lang}");
#endif
                return fallback;
            }

            // テーブルに無いキーはそのまま表示する（既存のリテラル指定を壊さないため）
            // Keys that are not in the table are displayed as-is, so existing literals keep working
            return key;
        }

        /// <summary>
        /// キーに対応する文言を取得し、プレースホルダを埋める
        /// </summary>
        public string Get(string key, UIManager.Language lang, params object[] args)
        {
            var format = Get(key, lang);
            if (args == null || args.Length == 0)
                return format;

            try
            {
                // 数値表記は言語によらず固定する
                // Number formatting is fixed regardless of the language
                return string.Format(CultureInfo.InvariantCulture, format, args);
            }
            catch (FormatException e)
            {
                Debug.LogError($"[Localization] invalid format. key:{key} lang:{lang} : {e.Message}");
                return format;
            }
        }
    }
}
