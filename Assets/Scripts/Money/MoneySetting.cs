using System;
using System.Collections.Generic;
using Gs2.Sample.MoneyStore;
using Gs2.Unity.Gs2Money.Model;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Money
{
    [Serializable]
    public class GetWalletEvent : UnityEvent<EzWallet>
    {
    }
    
    [System.Serializable]
    public class GetProductsEvent : UnityEvent<List<Product>>
    {
    }

    [Serializable]
    public class DepositEvent : UnityEvent<EzWallet, float, int>
    {
    }

    [Serializable]
    public class WithdrawEvent : UnityEvent<EzWallet, int>
    {
    }
    
    [Serializable]
    public class MoneySetting : MonoBehaviour
    {
        [SerializeField]
        public string moneyNamespaceName;

        [SerializeField] 
        public string showcaseNamespaceName;

        [SerializeField] 
        public string showcaseName;
        
        /// <summary>
        /// ウォレットを取得したとき
        /// </summary>
        [SerializeField]
        public GetWalletEvent onGetWallet = new GetWalletEvent();
        
        /// <summary>
        /// 販売中の課金通貨一覧を取得したとき
        /// </summary>
        [SerializeField]
        public GetProductsEvent onGetProducts = new GetProductsEvent();
        
        /// <summary>
        /// 課金通貨を購入したとき
        /// </summary>
        [SerializeField]
        public BuyEvent onBuy = new BuyEvent();

        /// <summary>
        /// エラー発生時に発行されるイベント
        /// </summary>
        [SerializeField]
        public ErrorEvent onError = new ErrorEvent();
    }
}