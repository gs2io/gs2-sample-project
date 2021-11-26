using UnityEngine;
using UnityEngine.UI;

namespace Gs2.Sample.Matchmaking
{
    public class JoinGatheringView : MonoBehaviour
    {
        /// <summary>
        /// キャンセル
        /// </summary>
        [SerializeField]
        public Button cancelButton;
        
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