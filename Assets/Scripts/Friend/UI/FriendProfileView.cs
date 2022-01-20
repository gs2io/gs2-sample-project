using TMPro;
using UnityEngine;

namespace Gs2.Sample.Friend
{
    public class FriendProfileView : MonoBehaviour
    {
        [SerializeField]
        public TextMeshProUGUI userId;
        
        [SerializeField]
        public TextMeshProUGUI publicProfile;

        [SerializeField]
        public TextMeshProUGUI friendProfile;

        public void SetUserId(string _userId)
        {
            userId.SetText(_userId);
        }
        
        public void SetPublicProfile(string profile)
        {
            publicProfile.SetText(profile);
        }
        
        public void SetFriendProfile(string profile)
        {
            friendProfile.SetText(profile);
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