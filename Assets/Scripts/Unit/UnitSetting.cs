using System;
using Gs2.Sample.Inventory;
using Gs2.Unity.Util;
using UnityEngine;

namespace Gs2.Sample.Unit
{
    [Serializable]
    public class UnitSetting : MonoBehaviour
    {
        [SerializeField]
        public string inventoryNamespaceName;

        [SerializeField]
        public string inventoryModelName;

        [SerializeField]
        public GetInventoryModelEvent onGetInventoryModel = new GetInventoryModelEvent();

        [SerializeField]
        public GetInventoryEvent onGetInventory = new GetInventoryEvent();

        [SerializeField]
        public ConsumeEvent onConsume = new ConsumeEvent();

        [SerializeField]
        public ErrorEvent onError = new ErrorEvent();
    }
}