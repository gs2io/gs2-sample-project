using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Gs2.Sample
{
    public class UIManager : SingletonMonoBehaviour<UIManager>
    {
        [SerializeField]
        public TextMeshProUGUI stateText = null;
        [SerializeField]
        public TextMeshProUGUI acountText = null;
        
        [SerializeField]
        public LogWindow logWindow = null;

        [SerializeField]
        public Dialog1 dialog1 = null;
        
        [SerializeField]
        public Dialog2 dialog2 = null;
        
        [SerializeField]
        public GameObject processing = null;
        
        [SerializeField]
        public Button startButton = null;
        [SerializeField]
        public Button removeAccountButton = null;
        [SerializeField]
        public GameObject startToTitle = null;
        [SerializeField]
        public Button TapToStart = null;
        [SerializeField]
        public Button TakeOver = null;
        [SerializeField]
        public GameObject titleToGame = null;
        [SerializeField]
        public GameObject gameMask = null;
        
        [SerializeField]
        public GameObject[] tabObject;
        
        [SerializeField]
        public TextMeshProUGUI questStateText = null;
        
        // Start is called before the first frame update
        void Start()
        {
            dialog1.gameObject.SetActive(true);
            dialog2.gameObject.SetActive(true);
            
            startToTitle.SetActive(false);
            TapToStart.interactable = false;
            TakeOver.interactable = false;
            titleToGame.SetActive(false);

            OnTapTab(0);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetStateText(string text)
        {
            stateText.SetText(text);
           
            Debug.Log("State:" + text);
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

        public void OpenDialog1(string title, string text, string buttonText = "OK")
        {
            dialog1.Initialize(title, text, buttonText);
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
        }
        
        /// <summary>
        /// アカウント連携ボタン
        /// </summary>
        /// <param name="enable"></param>
        public void SetTakeOverInteractable(bool enable)
        {
            TakeOver.interactable = enable ;
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
    }
}