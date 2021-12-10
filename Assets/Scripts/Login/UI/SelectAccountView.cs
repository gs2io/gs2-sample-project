using TMPro;
using UnityEngine;

namespace Gs2.Sample.Login
{
    public class SelectAccountView : MonoBehaviour
    {
        [SerializeField]
        public GameObject selectAccount;
        
        private void Start()
        {
            OnCloseEvent();
        }

        public void OnOpenEvent()
        {
            selectAccount.SetActive(true);
        }
        
        public void OnCloseEvent()
        {
            selectAccount.SetActive(false);
        }
    }
}