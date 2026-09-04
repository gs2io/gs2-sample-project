using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Gs2.Sample
{
    public class UIManager : SingletonMonoBehaviour<UIManager>
    {
        [SerializeField]
        public TextMeshProUGUI saveSlotText = null;
        [SerializeField]
        public TextMeshProUGUI acountText = null;
        
        [SerializeField]
        public LogWindow logWindow = null;

        [SerializeField]
        public Dialog1 dialog1 = null;
        
        [SerializeField]
        public Dialog2 dialog2 = null;
        
        [SerializeField]
        public WebViewDialog webViewDialog = null;
        
        [SerializeField]
        public GameObject processing = null;
        
        [SerializeField]
        public Button startButton = null;
        [SerializeField]
        public Button finishButton = null;
        [SerializeField]
        public Button selectAccountButton = null;
        [SerializeField]
        public Button removeAccountButton = null;
        [SerializeField]
        public GameObject startToTitle = null;
        [SerializeField]
        public Button TapToStart = null;
        [SerializeField]
        public Button TakeOver = null;
        [SerializeField]
        public Button News = null;
        [SerializeField]
        public GameObject titleToGame = null;
        [SerializeField]
        public GameObject gameMask = null;
        
        [SerializeField]
        public GameObject[] tabObject;
        
        [SerializeField]
        public TextMeshProUGUI questStateText = null;
        
        public enum Language
        {
            ja = 0,
            en = 1,
            ko = 2,
        }

        /// <summary>
        /// 言語ごとに使用するフォント
        /// 日本語フォントにはハングルの字形が含まれないため、言語ごとに差し替える
        ///
        /// Font asset used for each language.
        /// A Japanese font asset does not contain Hangul glyphs, so it is swapped per language.
        /// </summary>
        [Serializable]
        public class LocaleFont
        {
            public Language lang;
            public TMP_FontAsset font;
        }

        private const string LanguagePrefsKey = "Gs2.Sample.Language";

        [SerializeField] private Language lang = Language.ja;
        public Language Lang
        {
            get { return lang; }
        }

        /// <summary>
        /// 端末の言語設定と保存された設定から言語を決定する
        ///
        /// 有効な場合、Awake で lang が上書きされる。保存された設定（PlayerPrefs）が
        /// あればそれが優先されるため、実行時に言語を変えるときは SetLanguage を使う。
        /// Inspector の lang を切り替えて確認したい場合は一時的に false にする。
        ///
        /// Determine the language from the device setting and the saved preference.
        /// When enabled, lang is overwritten in Awake and a saved preference takes priority,
        /// so use SetLanguage to change the language at runtime.
        /// </summary>
        [SerializeField] private bool autoDetectLanguage = true;

        [SerializeField] private LocaleFont[] localeFonts = null;

        /// <summary>
        /// 言語が切り替わったときに呼ばれる
        /// Raised when the language has been changed
        /// </summary>
        public static event Action OnLanguageChanged;

        private LocalizationTable _localizationTable;

        private LocalizationTable Localization
        {
            get
            {
                if (_localizationTable == null)
                {
                    _localizationTable = new LocalizationTable();
                    _localizationTable.Load(lang);
                }

                return _localizationTable;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            if (this != Instance)
                return;

            if (autoDetectLanguage)
                lang = DetectLanguage();
        }

        /// <summary>
        /// 保存された設定を優先し、無ければ端末の言語設定から決定する
        /// </summary>
        private static Language DetectLanguage()
        {
            if (PlayerPrefs.HasKey(LanguagePrefsKey))
            {
                var saved = PlayerPrefs.GetInt(LanguagePrefsKey);
                if (Enum.IsDefined(typeof(Language), saved))
                    return (Language)saved;
            }

            switch (Application.systemLanguage)
            {
                case SystemLanguage.Japanese:
                    return Language.ja;
                case SystemLanguage.Korean:
                    return Language.ko;
                default:
                    return Language.en;
            }
        }

        /// <summary>
        /// 言語を切り替える
        /// 表示中の LocalizedText はイベント経由で自動的に更新される
        ///
        /// Switch the language.
        /// Every visible LocalizedText updates itself through the event.
        /// </summary>
        public void SetLanguage(Language next, bool save = true)
        {
            if (lang == next)
                return;

            lang = next;
            Localization.Load(lang);

            if (save)
            {
                PlayerPrefs.SetInt(LanguagePrefsKey, (int)next);
                PlayerPrefs.Save();
            }

            OnLanguageChanged?.Invoke();
        }

        /// <summary>
        /// 言語に対応するフォントを取得する
        /// 対応が登録されていない場合は null（元のフォントのまま）
        /// </summary>
        public TMP_FontAsset GetLocaleFont(Language target)
        {
            if (localeFonts == null)
                return null;

            foreach (var localeFont in localeFonts)
            {
                if (localeFont != null && localeFont.lang == target)
                    return localeFont.font;
            }

            return null;
        }
        
        // Start is called before the first frame update
        void Start()
        {
            dialog1.gameObject.SetActive(true);
            dialog2.gameObject.SetActive(true);
            
            startToTitle.SetActive(false);
            TapToStart.interactable = false;
            TakeOver.interactable = false;
            News.interactable = false;
            finishButton.interactable = false;
            titleToGame.SetActive(false);

            OnTapTab(0);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetSaveSlotText(string text)
        {
            saveSlotText.SetText(text);
        }
        
        public void SetQuestStateText(string text)
        {
            questStateText.SetText(text);
           
            Debug.Log("State:" + text);
        }
        
        public void SetAccountText(string text)
        {
            acountText.SetText(text);
        }
        
        public void AddLog(string text)
        {
            logWindow.AddLog(text);
           
            Debug.Log(text);
        }

        public void OnClickLogButton()
        {
            logWindow.gameObject.SetActive(!logWindow.gameObject.activeSelf);
        }

        /// <summary>
        /// キーに対応する文言を取得する
        /// テーブルに存在しないキーはそのまま返るため、リテラルを渡しても動作する
        ///
        /// Get the localized text for the key.
        /// A key that is not in the table is returned as-is, so passing a literal also works.
        /// </summary>
        public string GetLocalizationText(string key)
        {
            return Localization.Get(key, lang);
        }

        /// <summary>
        /// キーに対応する文言を取得し、{0} {1} ... を埋める
        /// 言語によって語順が変わるため、文字列連結ではなくこちらを使用する
        ///
        /// Get the localized text and fill in the {0} {1} ... placeholders.
        /// Use this instead of string concatenation because word order differs per language.
        /// </summary>
        public string GetLocalizationText(string key, params object[] args)
        {
            return Localization.Get(key, lang, args);
        }

        public void OpenDialog1(string title, string text, string buttonText = "OK")
        {
            dialog1.Initialize(
                GetLocalizationText(title),
                GetLocalizationText(text),
                GetLocalizationText(buttonText)
            );
            dialog1.gameObject.SetActive(true);
        }

        public void AddAcceptListner(UnityAction callback)
        {
            dialog1.AddListner(callback);
        }
        
        public void CloseDialog()
        {
            dialog1.gameObject.SetActive(false);
            dialog2.gameObject.SetActive(false);
        }
        
        public void OpenDialog2(string title, string text, string yesText = "Yes",  string noText = "No")
        {
            dialog2.Initialize(
                GetLocalizationText(title),
                GetLocalizationText(text),
                GetLocalizationText(yesText),
                GetLocalizationText(noText)
            );
            dialog2.gameObject.SetActive(true);
        }
        
        public void AddPositiveListner(UnityAction callback)
        {
            dialog2.AddPositiveListener(callback);
        }
            
        public void AddNegativeListner(UnityAction callback)
        {
            dialog2.AddNegativeListener(callback);
        }

        public void InitWebViewDialog(string title)
        {
            webViewDialog.Init(title);
        }

        public bool UseUniWebView()
        {
            return webViewDialog.UseUniWebView();
        }
        
        public void SetCookie(string url, string key, string value)
        {
            webViewDialog.SetCookie(url, key, value);
        }
        
        public void ClearCookie()
        {
            webViewDialog.ClearCookie();
        }
		
        public bool IsWebViewActiveAndEnabled()
        {
            return webViewDialog.IsActiveAndEnabled();
        }
        
        public void LoadURL(string url)
        {
            webViewDialog.LoadURL(url);
        }
        
        public void LoadHTML(string html, string baseUrl)
        {
            webViewDialog.LoadHTML(html, baseUrl);
        }
        
		public bool IsWebViewLoading()
        {
            return webViewDialog.IsLoading();
        }
		
        public void SetVisibility(bool visible)
        {
            webViewDialog.SetVisibility(visible);
        }
        
        public void OpenProcessing()
        {
            processing.gameObject.SetActive(true);
        }
        
        public void CloseProcessing()
        {
            processing.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// アプリ開始ボタン
        /// </summary>
        /// <param name="enable"></param>
        public void SetStartButtonInteractable(bool enable)
        {
            startButton.interactable = enable;
        }
        
        /// <summary>
        /// アプリ終了ボタン
        /// </summary>
        /// <param name="enable"></param>
        public void SetFinishButtonInteractable(bool enable)
        {
            finishButton.interactable = enable;
        }
        
        /// <summary>
        /// アカウント選択ボタン
        /// </summary>
        /// <param name="enable"></param>
        public void SetSelectAccountButtonInteractable(bool enable)
        {
            selectAccountButton.interactable = enable;
        }
        
        /// <summary>
        /// アカウント削除ボタン
        /// </summary>
        /// <param name="enable"></param>
        public void SetRemoveAccountButtonInteractable(bool enable)
        {
            removeAccountButton.interactable = enable;
        }
        
        /// <summary>
        /// アプリ開始からタイトルまで　ローディング画面中
        /// </summary>
        /// <param name="enable"></param>
        public void SetActiveTitleProgress(bool enable)
        {
            startToTitle.SetActive(enable);
        }
        
        /// <summary>
        /// ゲーム開始ボタン
        /// </summary>
        /// <param name="enable"></param>
        public void SetActiveTTapToStartButton(bool enable)
        {
            TapToStart.gameObject.SetActive(enable);
        }
        /// <summary>
        /// ゲーム開始ボタン
        /// </summary>
        /// <param name="enable"></param>
        public void SetTapToStartInteractable(bool enable)
        {
            TapToStart.interactable = enable ;
            TakeOver.interactable = enable;
            News.interactable = enable;
        }
        
        /// <summary>
        /// アカウント連携ボタン
        /// </summary>
        /// <param name="enable"></param>
        public void SetTakeOverInteractable(bool enable)
        {
            TakeOver.interactable = enable;
        }
        
        /// <summary>
        /// お知らせボタン
        /// </summary>
        /// <param name="enable"></param>
        public void SetNewsInteractable(bool enable)
        {
            News.interactable = enable;
        }
        
        /// <summary>
        /// タイトルからゲームまで　ローディング画面中
        /// </summary>
        /// <param name="enable"></param>
        public void SetActiveGameProgress(bool enable)
        {
            titleToGame.SetActive(enable);
        }
        
        /// <summary>
        /// ゲーム中
        /// </summary>
        /// <param name="enable"></param>
        public void SetActiveGame(bool enable)
        {
            gameMask.SetActive(!enable);
        }

        public void OnTapTab(int value)
        {
            tabObject[value].GetComponent<RectTransform>().SetAsLastSibling();
        }
        
        public void OnTapServiceLink(string url)
        {
            if (url.Contains("GS2-"))
                url = url.Replace("GS2-","");
            // ドキュメントは ja / en のみ提供されているため、それ以外の言語は en を開く
            // The documentation is provided only in ja / en, so other languages open en
            var documentLang = Lang == Language.ja ? "ja" : "en";
            Application.OpenURL($"https://docs.gs2.io/{documentLang}/api_reference/{url.ToLower()}/game_engine/");
        }
    }
}