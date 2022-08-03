using System;
using System.Collections.Generic;
using Gs2.Unity.Gs2Lottery.Model;
using Gs2.Unity.Gs2Showcase.Model;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Lottery
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
    public class AcquireInventoryItemEvent : UnityEvent<List<EzDrawnPrize>>
    {
        
    }
    
    [Serializable]
    public class LotterySetting : MonoBehaviour
    {
        [SerializeField]
        public string lotteryName;
        [SerializeField]
        public string showcaseNamespaceName;

        [SerializeField]
        public GetShowcaseEvent onGetShowcase = new GetShowcaseEvent();
        [SerializeField]
        public AcquireInventoryItemEvent onAcquireInventoryItem = new AcquireInventoryItemEvent();
        [SerializeField]
        public ErrorEvent onError = new ErrorEvent();
    }
}