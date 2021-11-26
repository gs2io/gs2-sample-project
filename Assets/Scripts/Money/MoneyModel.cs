using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gs2.Core;
using Gs2.Core.Exception;
using Gs2.Gs2Limit.Request;
using Gs2.Gs2Money.Request;
using Gs2.Sample.Core;
using Gs2.Sample.MoneyStore;
using Gs2.Unity;
using Gs2.Unity.Gs2Limit.Result;
using Gs2.Unity.Gs2Money.Model;
using Gs2.Unity.Gs2Money.Result;
using Gs2.Unity.Gs2Showcase.Model;
using Gs2.Unity.Gs2Showcase.Result;
using Gs2.Unity.Util;
using Gs2.Util.LitJson;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Money
{
    public class MoneyModel : MonoBehaviour
    {
        public EzWallet Wallet;

#if UNITY_IPHONE
        public const int Slot = 1;
#elif UNITY_ANDROID
        public const int Slot = 2;
#else
        public const int Slot = 0;
#endif
        
        /// <summary>
        /// 販売中の課金通貨
        /// </summary>
        public List<Product> products = new List<Product>();
        
        /// <summary>
        /// 購入メニューで選択した課金通貨
        /// </summary>
        public Product selectedProduct;
        
        /// <summary>
        /// 現在の有償財貨の取得
        /// </summary>
        /// <param name="client"></param>
        /// <param name="moneyNamespaceName"></param>
        /// <param name="slot"></param>
        /// <param name="onGetWallet"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator GetWallet(
            Client client,
            GameSession session,
            string moneyNamespaceName,
            GetWalletEvent onGetWallet,
            ErrorEvent onError
        )
        {
            AsyncResult<EzGetResult> result = null;
            yield return client.Money.Get(
                r =>
                {
                    result = r;
                },
                session,
                moneyNamespaceName,
                Slot
            );
            
            if (result.Error != null)
            {
                onError.Invoke(
                    result.Error
                );
                yield break;
            }

            Wallet = result.Result.Item;

            onGetWallet.Invoke(Wallet);
        }
        
        /// <summary>
        /// 販売中の課金通貨一覧を取得
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetListProducts(
            UnityAction<AsyncResult<List<Product>>> callback,
            Client client,
            GameSession session,
            string showcaseNamespaceName,
            string showcaseName,
            GetProductsEvent onGetProducts,
            ErrorEvent onError
        )
        {
            AsyncResult<EzGetShowcaseResult> result = null;
            yield return client.Showcase.GetShowcase(
                r =>
                {
                    result = r;
                },
                session,
                showcaseNamespaceName,
                showcaseName
            );
            
            if (result.Error != null)
            {
                onError.Invoke(
                    result.Error
                );
                callback.Invoke(new AsyncResult<List<Product>>(null, result.Error));
                yield break;
            }

            var products = new List<Product>();
            foreach (var displayItem in result.Result.Item.DisplayItems)
            {
                var depositRequest = GetAcquireAction<DepositByUserIdRequest>(
                    displayItem.SalesItem, 
                    "Gs2Money:DepositByUserId"
                );
                var recordReceiptRequest = GetConsumeAction<RecordReceiptRequest>(
                    displayItem.SalesItem, 
                    "Gs2Money:RecordReceipt"
                );
                var countUpRequest = GetConsumeAction<CountUpByUserIdRequest>(
                    displayItem.SalesItem, 
                    "Gs2Limit:CountUpByUserId"
                );
                var price = depositRequest.Price;
                var count = depositRequest.Count;

                int? boughtCount = null;
                if(countUpRequest != null) {
                    AsyncResult<EzGetCounterResult> result2 = null;
                    yield return client.Limit.GetCounter(
                        r => { result2 = r; },
                        session,
                        countUpRequest.NamespaceName,
                        countUpRequest.LimitName,
                        countUpRequest.CounterName
                    );
                    if (result2.Error == null)
                    {
                        boughtCount = result2.Result.Item.Count;
                    }
                    else if (result2.Error is NotFoundException)
                    {
                        boughtCount = 0;
                    }
                }
                products.Add(new Product
                {
                    Id = displayItem.DisplayItemId,
                    ContentsId = recordReceiptRequest.ContentsId,
                    Price = price,
                    CurrencyCount = count,
                    BoughtCount = boughtCount,
                    BoughtLimit = countUpRequest == null ? null : countUpRequest.MaxValue,
                });
            }
            
            onGetProducts.Invoke(products);
            
            callback.Invoke(new AsyncResult<List<Product>>(products, result.Error));
        }
             
        /// <summary>
        /// 
        /// </summary>
        /// <param name="salesItem"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private T GetAcquireAction<T>(
            EzSalesItem salesItem,
            string action
        )
        {
            var item = salesItem.AcquireActions.FirstOrDefault(acquireAction => acquireAction.Action == action);
            if (item == null)
            {
                return default;
            }
            return (T)typeof(T).GetMethod("FromJson")?.Invoke(null, new object[] { Gs2Util.RemovePlaceholder(JsonMapper.ToObject(item.Request)) });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="salesItem"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private T GetConsumeAction<T>(
            EzSalesItem salesItem,
            string action
        )
        {
            var item = salesItem.ConsumeActions.FirstOrDefault(consumeAction => consumeAction.Action == action);
            if (item == null)
            {
                return default;
            }
            return (T)typeof(T).GetMethod("FromJson")?.Invoke(null, new object[] { Gs2Util.RemovePlaceholder(JsonMapper.ToObject(item.Request)) });
        }
        
        /// <summary>
        /// 通貨を購入する
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="client"></param>
        /// <param name="session"></param>
        /// <param name="showcaseNamespaceName"></param>
        /// <param name="showcaseName"></param>
        /// <param name="distributorNamespaceName"></param>
        /// <param name="showcaseKeyId"></param>
        /// <param name="onBuy"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator Buy(
            UnityAction<AsyncResult<object>> callback,
            Client client,
            GameSession session,
            string showcaseNamespaceName,
            string showcaseName,
            string distributorNamespaceName,
            string showcaseKeyId,
            BuyEvent onBuy,
            ErrorEvent onError
        )
        {
            string receipt = null;
            {
#if UNITY_PURCHASING
                AsyncResult<PurchaseParameters> result = null;
                yield return new IAPUtil().Buy(
                    r => { result = r; },
                    selectedProduct.ContentsId
                );
                if (result.Error != null)
                {
                    onError.Invoke(
                        result.Error
                    );
                    callback.Invoke(new AsyncResult<object>(null, result.Error));
                    yield break;
                }

                receipt = result.Result.receipt;
#endif
            }
            string stampSheet = null;
            {
                AsyncResult<EzBuyResult> result = null;
                yield return client.Showcase.Buy(
                    r => { result = r; },
                    session,
                    showcaseNamespaceName,
                    showcaseName,
                    selectedProduct.Id,
                    new List<EzConfig>
                    {
                        new EzConfig
                        {
                            Key = "slot",
                            Value = Slot.ToString(),
                        },
                        new EzConfig
                        {
                            Key = "receipt",
                            Value = receipt,
                        },
                    }
                );

                if (result.Error != null)
                {
                    onError.Invoke(
                        result.Error
                    );
                    callback.Invoke(new AsyncResult<object>(null, result.Error));
                    yield break;
                }

                stampSheet = result.Result.StampSheet;
            }
            {
                var machine = new StampSheetStateMachine(
                    stampSheet,
                    client,
                    distributorNamespaceName,
                    showcaseKeyId
                );

                Gs2Exception exception = null;
                void OnError(Gs2Exception e)
                {
                    exception = e;
                }
                
                onError.AddListener(OnError);
                yield return machine.Execute(onError);
                onError.RemoveListener(OnError);
                
                if (exception != null)
                {
                    callback.Invoke(new AsyncResult<object>(null, exception));
                    yield break;
                }
            }
            
            onBuy.Invoke(selectedProduct);
            
            callback.Invoke(new AsyncResult<object>(null, null));
        }
    }
}