using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gs2.Sample.Friend
{
    public class PlayerInfoView : MonoBehaviour
    {
        [SerializeField]
        public TextMeshProUGUI userId;

        [SerializeField]
        public Button profile;

        [SerializeField]
        public Button follow;
        
        [SerializeField]
        public Button friendRequest;

        [SerializeField]
        public Button blackList;
        
        public void SetUserId(string _userId)
        {
            userId.SetText(_userId);
        }

        public void OnOpenEvent()
        {
            gameObject.SetActive(true);
        }
        
        public void OnCloseEvent()
        {
            gameObject.SetActive(false);
        }
    }
}