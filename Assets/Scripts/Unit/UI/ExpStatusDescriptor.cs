using System;
using Gs2.Unity.Gs2Inventory.Model;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Gs2.Sample.Experience
{
    [Serializable]
    public class ClickIncreaseExperienceEvent : UnityEvent<string, int>
    {
        
    }

    [Serializable]
    public class ClickDecreaseItemEvent : UnityEvent<EzItemSet>
    {
    }
    
    public class ExpStatusDescriptor : MonoBehaviour
    {
        [SerializeField] public TextMeshProUGUI rankText;

        [SerializeField] public TextMeshProUGUI currentExpText;

        [SerializeField] public TextMeshProUGUI nextExpText;

        [SerializeField] public TextMeshProUGUI addValueText;

        [SerializeField] public TextMeshProUGUI propertyIdText;

        public ClickIncreaseExperienceEvent onClickIncreaseExperience = new ClickIncreaseExperienceEvent();

        public ClickDecreaseItemEvent onClickDecreaseItem = new ClickDecreaseItemEvent();
        
        public int increaseValue = 100;

        private EzItemSet _item;
        private string _propertyId;
        
        public void Initialize(long rankValue, long experienceValue, long nextRankExperience, EzItemSet item)
        {
            _item = item;
            _propertyId = item.ItemSetId;
            
            rankText.SetText(rankValue.ToString());
            
            currentExpText.SetText(experienceValue.ToString());
            nextExpText.SetText(nextRankExperience.ToString());
            
            addValueText.SetText(increaseValue.ToString());
            
            propertyIdText.SetText(_propertyId);
            
            OnOpenEvent();
        }
        
        public void OnOpenEvent()
        {
            gameObject.SetActive(true);
        }
        
        public void OnCloseEvent()
        {
            gameObject.SetActive(false);
        }
        
        public void OnClickIncreaseExperienceButton()
        {
            onClickIncreaseExperience.Invoke(
                _propertyId,
                increaseValue
            );
        }
        
        public void OnClickDecreaseItemButton()
        {
            onClickDecreaseItem.Invoke(
                _item
            );

            OnCloseEvent();
        }
    }
}