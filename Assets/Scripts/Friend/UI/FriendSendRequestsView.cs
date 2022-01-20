using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gs2.Sample.Friend
{
    public class FriendSendRequestsView : MonoBehaviour
    {
        /// <summary>
        /// リストを表示する GameObject
        /// </summary>
        [SerializeField]
        public GameObject friendListContent;

        /// <summary>
        /// リストにフレンド名を表示するプレハブ
        /// </summary>
        [SerializeField]
        public GameObject friendNamePrefab;

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