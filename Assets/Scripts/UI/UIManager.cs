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
            en = 1
        }
        
        [SerializeField] private Language lang = Language.ja;
        public Language Lang
        {
            get { return lang; }
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

        public string GetLocalizationText(string text)
        {
            var EnglishDic = new Dictionary<string, List<string>>
            { 
                {"LinkAdd", new List<string>{"アカウントを連携しました。","Linked accounts."}},
                {"LinkRemove", new List<string>{"アカウント連携を解除しました。","Account linkage has been removed."}},
                {"TakeOver", new List<string>{"アカウント引継ぎを実行しました。","Account transfer has been executed."}},
                {"FriendRequestSend", new List<string>{"フレンドリクエストを送信しました。","Friend request sent."}},
                {"FriendRequestAccept", new List<string>{"フレンドリクエストを承認しました。","Friend request approved."}},
                {"FriendRequestReject", new List<string>{"フレンドリクエストを拒否しました。","Friend request denied."}},
                {"FriendRemove", new List<string>{"フレンドから削除しました。","Removed from Friends."}},
                {"FriendRequestDelete", new List<string>{"フレンドリクエストを削除しました。","Friend request deleted."}},
                {"FriendRemoveBlacklist", new List<string>{"ブラックリストから削除しました。","Removed from blacklist."}},
                {"FriendAddBlacklist", new List<string>{"ブラックリストに追加しました。","Added to blacklist."}},
                {"Follow", new List<string>{"フォローしました。","Followed."}},
                {"Unfollow", new List<string>{"フォローを解除しました。","Unfollowed."}},
                {"ProductPurchase", new List<string>{"商品を購入しました。","Products purchased."}},
                {"QuestStart", new List<string>{"クエストを開始","Start Quest."}},
                {"QuestComp", new List<string>{"クエスト完了","Quest Completed."}},
                {"QuestFailed", new List<string>{"クエスト失敗","Quest Failure."}},
                {"StaminaPurchase", new List<string>{"スタミナ回復を購入しました。","Stamina recovery was purchased."}},
                {"UnitObtain", new List<string>{"入手しました。", "obtained."}},
                {"Fire", new List<string>{"炎", "Fire"}},
                {"Water", new List<string>{"水", "Water"}},
            };

            if (EnglishDic.ContainsKey(text))
            {
                var list = EnglishDic[text];
                return list[(int)Lang];
            }

            return text;
        }
        
        public void OpenDialog1(string title, string text, string buttonText = "OK")
        {
            var localizedtext = GetLocalizationText(text);

            dialog1.Initialize(title, localizedtext, buttonText);
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
            dialog2.Initialize(title, text, yesText, noText);
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

        public void OpenWebViewDialog(string title, string url)
        {
            webViewDialog.Show(title, url);
        }
        
        public void InitWebViewDialog(string title)
        {
            webViewDialog.Init(title);
        }
        
        public void SetCookie(string key, string value)
        {
            webViewDialog.SetCookie(key, value);
        }
        
        public bool isWebViewActiveAndEnabled()
        {
            return webViewDialog.isActiveAndEnabled;
        }
        
        public void LoadURL(string url)
        {
            webViewDialog.LoadURL(url);
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
            switch (Lang)
            {
                case  Language.ja:
                    Application.OpenURL("https://app.gs2.io/docs/index.html#"+url.ToLower());
                    break;
                case  Language.en:
                    Application.OpenURL("https://app.gs2.io/docs/en/index.html#"+url.ToLower());
                    break;
            }
        }
    }
}