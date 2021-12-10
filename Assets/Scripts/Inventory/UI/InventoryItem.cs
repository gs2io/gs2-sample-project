using Gs2.Unity.Gs2Inventory.Model;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Gs2.Sample.Inventory
{
    public class ClickItemEvent : UnityEvent<EzItemSet>
    {
    }
    
    public class InventoryItem : MonoBehaviour
    {
        public Image image;
        public TextMeshProUGUI icon;
        public TextMeshProUGUI stackCount;

        public ClickItemEvent onClickItem = new ClickItemEvent();

        private EzItemSet _itemSet;

        public void Initialize(
            EzItemSet itemSet
        )
        {
            _itemSet = itemSet;
        }
        
        public void Start()
        {
            if (_itemSet.ItemName == "fire_element")
            {
                icon.SetText("炎");
                image.color = Color.red;
            }
            else if (_itemSet.ItemName == "water_element")
            {
                icon.SetText("水");
                image.color = Color.blue;
            }
            else
            {
                icon.SetText("？");
            }

            stackCount.text = $"{_itemSet.Count}";
        }

        public void OnClick()
        {
            onClickItem.Invoke(
                _itemSet
            );
        }
    }
}