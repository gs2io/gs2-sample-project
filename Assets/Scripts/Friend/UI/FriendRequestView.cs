using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gs2.Sample.Friend
{
    public class FriendRequestView : MonoBehaviour
    {
        [SerializeField] public TextMeshProUGUI userId;
        
        [SerializeField] public Button profile;
        
        [SerializeField] public Button accept;
        
        [SerializeField] public Button reject;
    }
}