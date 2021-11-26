using System;
using System.Collections.Generic;
using Gs2.Unity.Util;
using Gs2.Unity.Gs2Exchange.Model;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.StaminaStore
{
    [Serializable]
    public class GetExchangeRateEvent : UnityEvent<string, List<EzRateModel>>
    {
    }

    [Serializable]
    public class ExchangeEvent : UnityEvent
    {
    }

    [Serializable]
    public class IssueExchangeStampSheetEvent : UnityEvent<string>
    {
    }
    
    [Serializable]
    public class StaminaStoreSetting : MonoBehaviour
    {
        [SerializeField]
        public string exchangeNamespaceName;
        
        [SerializeField]
        public string exchangeKeyId;
        
        [SerializeField]
        public GetExchangeRateEvent onGetExchangeRate = new GetExchangeRateEvent();
        
        [SerializeField]
        public ExchangeEvent onBuy = new ExchangeEvent();

        [SerializeField]
        public IssueExchangeStampSheetEvent onIssueBuyStampSheet = new IssueExchangeStampSheetEvent();

        [SerializeField]
        public ErrorEvent onError = new ErrorEvent();
    }
}