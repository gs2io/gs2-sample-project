using TMPro;
using UnityEngine;

namespace Gs2.Sample.Friend
{
    public class FriendProfileEditView : MonoBehaviour
    {
        [SerializeField]
        public TMP_InputField publicProfile;

        [SerializeField]
        public TMP_InputField followerProfile;

        [SerializeField]
        public TMP_InputField friendProfile;

        public void SetPublicProfile(string profile)
        {
            publicProfile.text = profile;
        }
        
        public void SetFollowProfile(string profile)
        {
            followerProfile.text = profile;
        }
        
        public void SetFriendProfile(string profile)
        {
            friendProfile.text = profile;
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