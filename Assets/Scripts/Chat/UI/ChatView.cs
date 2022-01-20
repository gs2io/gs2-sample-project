using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gs2.Sample.Chat
{
    public class ChatView : MonoBehaviour
    {
        /// <summary>
        /// メッセージをアタッチするGameObject
        /// </summary>
        [SerializeField]
        public GameObject messagesContent;

        /// <summary>
        /// メッセージのプレハブ
        /// </summary>
        [SerializeField]
        public GameObject myMessagePrefab;
        [SerializeField]
        public GameObject messagePrefab;

        [SerializeField]
        private TMP_InputField InputField;
        
        [SerializeField]
        private ScrollRect scrollRect = null;
        
        /// <summary>
        /// 縮小時のタブ
        /// </summary>
        [SerializeField]
        public GameObject tab;
        /// <summary>
        /// 拡大ボタン
        /// </summary>
        [SerializeField]
        public Button tabButton;

        /// <summary>
        /// チャットUI
        /// </summary>
        [SerializeField]
        public GameObject chatUI;
        /// <summary>
        /// 縮小ボタン
        /// </summary>
        [SerializeField]
        public Button scaleDownButton;
        
        // Start is called before the first frame update
        void Start()
        {
        }

        public string GetMessage()
        {
            return InputField.text;
        }
        
        public string ClearMessage()
        {
            return InputField.text = String.Empty;
        }
        
        public void ScrollDown()
        {
            if (gameObject.activeInHierarchy)
            {
                // スクロールを一番下に
                StartCoroutine(ForceScrollDown());
            }
        }
        
        IEnumerator ForceScrollDown()
        {
            yield return new WaitForEndOfFrame();
            scrollRect.verticalNormalizedPosition = 0.0f;
        }
        
        public void OnClickScaleUp()
        {
            chatUI.SetActive(true);
        }
        public void OnClickScaleDown()
        {
            chatUI.SetActive(false);
        }
    }
}