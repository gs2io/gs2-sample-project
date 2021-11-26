using System;
using System.Xml.Schema;
using Gs2.Unity.Gs2Stamina.Model;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Gs2.Sample.Stamina
{
    public class StaminaStoreView : MonoBehaviour
    {
        public TextMeshProUGUI destText;

        public TextMeshProUGUI currentStaminaText;
        
        public TextMeshProUGUI recoveredStaminaText;
        
        public TextMeshProUGUI priceText;

        public Button buyButton;

        public void Initialize(EzStamina stamina, int recover, long price, UnityAction onClick)
        {
            destText.SetText(recover.ToString());
            
            currentStaminaText.SetText(stamina.Value.ToString());
            var recoveredStamina = Math.Min(stamina.Value + recover, stamina.MaxValue);
            recoveredStaminaText.SetText(recoveredStamina.ToString());
            
            priceText.SetText(price.ToString());
            
            buyButton.onClick.AddListener(onClick);
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
