using Gs2.Unity.Gs2Inventory.Model;
using Gs2.Util.LitJson;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Unit
{
    public class ClickItemEvent : UnityEvent<EzItemSet>
    {
    }
    
    public class Metadata
    {
        public string displayName;

        public int rarity;
    }
    
    public class UnitItem : MonoBehaviour
    {
        public TextMeshProUGUI icon;
        public TextMeshProUGUI rarity;

        public ClickItemEvent onClickItem = new ClickItemEvent();

        private EzItemSet _itemSet;

        public void Initialize(
            EzItemModel itemModel,
            EzItemSet itemSet
        )
        {
            _itemSet = itemSet;

            var metadata = JsonMapper.ToObject<Metadata>(itemModel.Metadata);
            icon.text = metadata.displayName;
            rarity.text = "";
            for (int i = 0; i < metadata.rarity + 1; i++)
            {
                rarity.text += "★";
            }

            while (rarity.text.Length < 5)
            {
                rarity.text = "☆" + rarity.text;
            }
        }
        
        public void OnClick()
        {
            onClickItem.Invoke(
                _itemSet
            );
        }
    }
}