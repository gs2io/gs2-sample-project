using System;
using Gs2.Unity.Util;
using Gs2.Sample.Money2;
using Gs2.Unity.Gs2Showcase.Model;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Money2Store
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
    public class Money2StoreSetting : MonoBehaviour
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
        /// 販売中の商品一覧を取得したとき
        /// </summary>
        [SerializeField]
        public GetProductsEvent onGetProducts = new GetProductsEvent();
        
        [SerializeField]
        public BuyEvent onBuy = new BuyEvent();

        [SerializeField]
        public ErrorEvent onError = new ErrorEvent();
    }
}