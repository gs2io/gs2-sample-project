using System;
using Gs2.Unity.Util;
using Gs2.Sample.Money;
using Gs2.Unity.Gs2Showcase.Model;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.MoneyStore
{
    [Serializable]
    public class GetShowcaseEvent : UnityEvent<EzShowcase>
    {
    }
    
    [Serializable]
    public class BuyEvent : UnityEvent<Product>
    {
    }

    [Serializable]
    public class IssueBuyStampSheetEvent : UnityEvent<string>
    {
    }
    
    [Serializable]
    public class MoneyStoreSetting : MonoBehaviour
    {
        [SerializeField]
        public string moneyNamespaceName;
        [SerializeField]
        public string showcaseNamespaceName;
        [SerializeField]
        public string showcaseName;
        [SerializeField]
        public string showcaseKeyId;

        [SerializeField]
        public GetShowcaseEvent onGetShowcase = new GetShowcaseEvent();
        
        /// <summary>
        /// 販売中のガチャ一覧を取得したとき
        /// </summary>
        [SerializeField]
        public GetProductsEvent onGetProducts = new GetProductsEvent();
        
        [SerializeField]
        public BuyEvent onBuy = new BuyEvent();
        
        [SerializeField]
        public IssueBuyStampSheetEvent onIssueBuyStampSheet = new IssueBuyStampSheetEvent();
        
        [SerializeField]
        public ErrorEvent onError = new ErrorEvent();
    }
}