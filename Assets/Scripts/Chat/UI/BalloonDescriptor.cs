using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Gs2.Sample.Chat
{
    public class BalloonDescriptor : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI userId;
        
        [SerializeField]
        private TextMeshProUGUI chatText;

        [SerializeField]
        public Button button;

        private string _name;
        public string Name => _name;
        
        public void SetUserId(string text)
        {
            userId.SetText(text);
        }
        
        public void SetText(string text, bool other)
        {
            chatText.SetText(text);
        }
        
        public void SetName(string name)
        {
            _name = name;
        }
    }
}