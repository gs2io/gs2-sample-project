using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Gs2.Sample.Quest
{
    public class QuestClearView : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI questName;

        [SerializeField]
        private Button successButton = null;
        
        [SerializeField]
        private Button failedButton = null;

        public void OnOpenEvent()
        {
            gameObject.SetActive(true);
        }
        
        public void OnCloseEvent()
        {
            gameObject.SetActive(false);
        }
        
        public void SetProgress(string questName)
        {
            SetText( questName );
        }
        
        public void SetInteractable(bool enable)
        {
            successButton.interactable = enable;
            failedButton.interactable = enable;
        }
        public void SetText(string text)
        {
            questName.text = text;
        }
        
        public void AddSuccessListner(UnityAction callback)
        {
            successButton.onClick.RemoveAllListeners();
            successButton.onClick.AddListener(callback);
            successButton.onClick.AddListener(OnCloseEvent);
        }
            
        public void AddFailedListner(UnityAction callback)
        {
            failedButton.onClick.RemoveAllListeners();
            failedButton.onClick.AddListener(callback);
            failedButton.onClick.AddListener(OnCloseEvent);
        }
    }
}