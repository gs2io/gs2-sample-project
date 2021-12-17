using System.Collections;
using Gs2.Unity.Gs2Inventory.Model;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Gs2.Sample.Inventory
{
    public class UseItemEvent : UnityEvent<EzItemSet>
    {
    }

    public class InventoryView : MonoBehaviour
    {
        [SerializeField] 
        public InventoryItem inventoryItemPrefab;

        [SerializeField] 
        public InventoryItemEmpty inventoryItemEmptyPrefab;

        [SerializeField] 
        public TextMeshProUGUI capacity;

        [SerializeField]
        public Transform contentTransform;

        [SerializeField]
        public ScrollRect scrollRect;
        
        public UseItemEvent onUseItem = new UseItemEvent();

        public void OnEnable()
        {
            StartCoroutine(ScrollTop());
        }
        private IEnumerator ScrollTop(){
            yield return null;
            scrollRect.verticalNormalizedPosition = 1.0f;
        }
        
        public void OnOpenEvent()
        {
            gameObject.SetActive(true);
        }
        
        public void OnCloseEvent()
        {
            gameObject.SetActive(false);
        }
        
        public void OnClickItem(EzItemSet itemSet)
        {
            onUseItem.Invoke(itemSet);
        }
    }
}