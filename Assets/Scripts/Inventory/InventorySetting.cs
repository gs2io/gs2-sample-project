using System;
using System.Collections.Generic;
using Gs2.Unity.Gs2Inventory.Model;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Inventory
{
    [Serializable]
    public class GetInventoryModelEvent : UnityEvent<string, EzInventoryModel, List<EzItemModel>>
    {
    }

    [Serializable]
    public class GetInventoryEvent : UnityEvent<EzInventory, List<EzItemSet>>
    {
    }

    [Serializable]
    public class GetItemSetWithSignatureEvent : UnityEvent<string, string, string, string>
    {
    }

    [Serializable]
    public class AcquireEvent : UnityEvent<EzInventory, List<EzItemSet>, int>
    {
    }

    [Serializable]
    public class ConsumeEvent : UnityEvent<EzInventory, List<EzItemSet>, int>
    {
    }
    
    [Serializable]
    public class InventorySetting : MonoBehaviour
    {
        [SerializeField]
        public string inventoryNamespaceName;

        [SerializeField]
        public string inventoryModelName;

        [SerializeField]
        public string exchangeNamespaceName;

        [SerializeField]
        public string exchangeRateNameFire;

        [SerializeField]
        public string exchangeRateNameWater;

        [SerializeField]
        public GetInventoryModelEvent onGetInventoryModel = new GetInventoryModelEvent();

        [SerializeField]
        public GetInventoryEvent onGetInventory = new GetInventoryEvent();

        [SerializeField]
        public AcquireEvent onAcquire = new AcquireEvent();
        
        [SerializeField]
        public ConsumeEvent onConsume = new ConsumeEvent();

        [SerializeField]
        public ErrorEvent onError = new ErrorEvent();
    }
}