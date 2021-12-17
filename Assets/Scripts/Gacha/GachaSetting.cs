using System;
using System.Collections.Generic;
using Gs2.Gs2Inventory.Request;
using Gs2.Sample.Money;
using Gs2.Unity.Gs2Showcase.Model;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Gacha
{
    [Serializable]
    public class GetShowcaseEvent : UnityEvent<EzShowcase>
    {
    }
    
    [Serializable]
    public class BuyEvent : UnityEvent<SalesItem>
    {
    }

    [Serializable]
    public class IssueBuyStampSheetEvent : UnityEvent<string>
    {
    }
    
    [Serializable]
    public class AcquireInventoryItemEvent : UnityEvent<List<AcquireItemSetByUserIdRequest>>
    {
        
    }
    
    [Serializable]
    public class GachaSetting : MonoBehaviour
    {
        [SerializeField]
        public string lotteryNamespaceName;
        [SerializeField]
        public string jobQueueNamespaceName;
        [SerializeField]
        public string showcaseNamespaceName;
        [SerializeField]
        public string showcaseName;
        [SerializeField]
        public string showcaseKeyId;
        [SerializeField]
        public string lotteryKeyId;

        [SerializeField]
        public GetShowcaseEvent onGetShowcase = new GetShowcaseEvent();
        [SerializeField]
        public IssueBuyStampSheetEvent onIssueBuyStampSheet = new IssueBuyStampSheetEvent();
        [SerializeField]
        public AcquireInventoryItemEvent onAcquireInventoryItem = new AcquireInventoryItemEvent();
        [SerializeField]
        public ErrorEvent onError = new ErrorEvent();
    }
}