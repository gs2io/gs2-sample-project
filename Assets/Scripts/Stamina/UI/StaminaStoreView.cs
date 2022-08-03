using System;
using Gs2.Unity.Gs2Stamina.Model;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Gs2.Sample.Stamina
{
    public class StaminaStoreView : MonoBehaviour
    {
        [SerializeField]
        public TextMeshProUGUI destText;
        [SerializeField]
        public TextMeshProUGUI currentStaminaText;
        [SerializeField]
        public TextMeshProUGUI recoveredStaminaText;
        [SerializeField]
        public TextMeshProUGUI priceText;
        
        [SerializeField]
        public Button buyButton;

        public void Initialize(EzStamina stamina, int recover, long price, long balance)
        {
            destText.SetText(recover.ToString());
            
            currentStaminaText.SetText(stamina.Value.ToString());
            var recoveredStamina = Math.Min(stamina.Value + recover, stamina.MaxValue);
            recoveredStaminaText.SetText(recoveredStamina.ToString());
            
            priceText.SetText(price.ToString());

            if (balance < price)
            {
                buyButton.interactable = false;
            }
            else
            {
                buyButton.interactable = true;
            }
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
