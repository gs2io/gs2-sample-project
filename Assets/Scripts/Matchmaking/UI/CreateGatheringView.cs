using TMPro;
using UnityEngine;

namespace Gs2.Sample.Matchmaking
{
    public class CreateGatheringView : MonoBehaviour
    {
        /// <summary>
        /// 参加人数
        /// </summary>
        [SerializeField] public TMP_InputField capacityInputField;

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