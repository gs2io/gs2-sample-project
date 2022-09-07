using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gs2.Core;
using Gs2.Core.Exception;
using Gs2.Gs2Limit.Request;
using Gs2.Gs2Money.Request;
using Gs2.Sample.Core;
using Gs2.Sample.MoneyStore;
using Gs2.Unity.Core;
using Gs2.Unity.Gs2Money.Model;
using Gs2.Unity.Gs2Showcase.Model;
using Gs2.Unity.Util;
using Gs2.Util.LitJson;
using UnityEngine;
using UnityEngine.Events;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
#endif

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
        /// Billing Currency on Sale
        /// </summary>
        public List<Product> Products = new List<Product>();
        
        /// <summary>
        /// 購入メニューで選択した課金通貨
        /// Billing currency selected in the purchase menu
        /// </summary>
        public Product selectedProduct;
        
        /// <summary>
        /// 現在の課金通貨の取得
        /// Obtain current billing currency
        /// </summary>
        public IEnumerator GetWallet(
            Gs2Domain gs2,
            GameSession gameSession,
            string moneyNamespaceName,
            GetWalletEvent onGetWallet,
            ErrorEvent onError
        )
        {
            var domain = gs2.Money.Namespace(
                namespaceName: moneyNamespaceName
            ).Me(
                gameSession: gameSession
            ).Wallet(
                slot: Slot
            );
            var future = domain.Model();
            yield return future;
            if (future.Error != null)
            {
                onError.Invoke(future.Error, null);
                yield break;
            }
            
            Wallet = future.Result;

            onGetWallet.Invoke(Wallet);
        }
#if GS2_ENABLE_UNITASK
        public async UniTask GetWalletAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string moneyNamespaceName,
            GetWalletEvent onGetWallet,
            ErrorEvent onError
        )
        {
            var domain = gs2.Money.Namespace(
                namespaceName: moneyNamespaceName
            ).Me(
                gameSession: gameSession
            ).Wallet(
                slot: Slot
            );
            try
            {
                Wallet = await domain.ModelAsync();

                onGetWallet.Invoke(Wallet);
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e, null);
            }
        }
#endif
        
        /// <summary>
        /// 販売中の課金通貨商品一覧を取得
        /// Get a list of billable currency products on sale
        /// </summary>
        public IEnumerator GetProducts(
            UnityAction<Gs2Exception> callback,
            Gs2Domain gs2,
            GameSession gameSession,
            string showcaseNamespaceName,
            string showcaseName,
            GetProductsEvent onGetProducts,
            ErrorEvent onError
        )
        {
            var domain = gs2.Showcase.Namespace(
                namespaceName: showcaseNamespaceName
            ).Me(
                gameSession: gameSession
            ).Showcase(
                showcaseName: showcaseName
            );
            var future = domain.Model();
            yield return future;
            if (future.Error != null)
            {
                onError.Invoke(
                    future.Error,
                    null
                );
                yield break;
            }
            
            Products.Clear();
            foreach (var displayItem in future.Result.DisplayItems)
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
                    var domain2 = gs2.Limit.Namespace(
                        namespaceName: countUpRequest.NamespaceName
                    ).Me(
                        gameSession: gameSession
                    ).Counter(
                        limitName: countUpRequest.LimitName,
                        counterName: countUpRequest.CounterName
                    );
                    var future2 = domain2.Model();
                    yield return future2;
                    if (future2.Error == null)
                    {
                        boughtCount = future2.Result.Count;
                    }
                    else if (future2.Error is NotFoundException)
                    {
                        boughtCount = 0;
                    }
                }
                Products.Add(new Product
                {
                    Id = displayItem.DisplayItemId,
                    ContentsId = recordReceiptRequest.ContentsId,
                    Price = price,
                    CurrencyCount = count,
                    BoughtCount = boughtCount,
                    BoughtLimit = countUpRequest == null ? null : countUpRequest.MaxValue,
                });
            }
            
            onGetProducts.Invoke(Products);
            
            callback.Invoke(null);
        }
#if GS2_ENABLE_UNITASK
        public async UniTask<List<Product>> GetProductsAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string showcaseNamespaceName,
            string showcaseName,
            GetProductsEvent onGetProducts,
            ErrorEvent onError
        )
        {
            var domain = gs2.Showcase.Namespace(
                namespaceName: showcaseNamespaceName
            ).Me(
                gameSession: gameSession
            ).Showcase(
                showcaseName: showcaseName
            );
            try
            {
                var showcase = await domain.ModelAsync();
                
                Products.Clear();
                foreach (var displayItem in showcase.DisplayItems)
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
                    if (countUpRequest != null)
                    {
                        var domain2 = gs2.Limit.Namespace(
                            namespaceName: countUpRequest.NamespaceName
                        ).Me(
                            gameSession: gameSession
                        ).Counter(
                            limitName: countUpRequest.LimitName,
                            counterName: countUpRequest.CounterName
                        );
                        try
                        {
                            var item = await domain2.ModelAsync();
                            boughtCount = item.Count;
                        }
                        catch (NotFoundException)
                        {
                            boughtCount = 0;
                        }
                        catch (Gs2Exception e)
                        {
                            onError.Invoke(e, null);
                        }
                    }

                    Products.Add(new Product
                    {
                        Id = displayItem.DisplayItemId,
                        ContentsId = recordReceiptRequest.ContentsId,
                        Price = price,
                        CurrencyCount = count,
                        BoughtCount = boughtCount,
                        BoughtLimit = countUpRequest == null ? null : countUpRequest.MaxValue,
                    });
                }

                onGetProducts.Invoke(Products);

                return Products;
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e, null);
            }

            return null;
        }
#endif

        /// <summary>
        /// 入手アクション取得
        /// Obtain aquire action
        /// </summary>
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
        /// 消費アクション取得
        /// Consumption Action Acquisition
        /// </summary>
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
        /// 課金通貨を購入する
        /// Purchase billable currency
        /// </summary>
        public IEnumerator Buy(
            UnityAction<Gs2Exception> callback,
            Gs2Domain gs2,
            GameSession gameSession,
            string showcaseNamespaceName,
            string showcaseName,
            BuyEvent onBuy,
            ErrorEvent onError
        )
        {
            string receipt = string.Empty;
            {
#if GS2_ENABLE_PURCHASING
                AsyncResult<PurchaseParameters> result = null;
                yield return new IAPUtil().Buy(
                    r => { result = r; },
                    selectedProduct.ContentsId
                );
                if (result.Error != null)
                {
                    onError.Invoke(
                        result.Error,
                        null
                    );
                    callback.Invoke(
                        result.Error
                    );
                    yield break;
                }

                // 課金通貨商品購入 レシート情報
                // Billed Currency Product Purchase Receipt Information
                receipt = result.Result.receipt;
#endif
            }
            {
                // Showcase 商品の購入をリクエスト
                // Request to purchase an item
                var domain = gs2.Showcase.Namespace(
                    namespaceName: showcaseNamespaceName
                ).Me(
                    gameSession: gameSession
                ).Showcase(
                    showcaseName: showcaseName
                );
                var future = domain.Buy(
                    displayItemId: selectedProduct.Id,
                    config: new []
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
                yield return future;
                if (future.Error != null)
                {
                    onError.Invoke(
                        future.Error,
                        null
                    );
                    callback.Invoke(
                        future.Error
                    );
                    yield break;
                }

                // 商品購入に成功
                // Successful product purchase

                onBuy.Invoke(selectedProduct);

                callback.Invoke(null);
            }
        }
#if GS2_ENABLE_UNITASK
        public async UniTask<Gs2Exception> BuyAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string showcaseNamespaceName,
            string showcaseName,
            BuyEvent onBuy,
            ErrorEvent onError
        )
        {
            string receipt;
            {
#if GS2_ENABLE_PURCHASING
                try
                {
                    PurchaseParameters result = await new IAPUtil().BuyAsync(
                        selectedProduct.ContentsId
                    );
                    
                    // 課金通貨商品購入 レシート情報
                    // Billed Currency Product Purchase Receipt Information
                    receipt = result.receipt;
                }
                catch (Gs2Exception e)
                {
                    onError.Invoke(e, null);
                    return e;
                }
#endif
            }
            {
                // Showcase 商品の購入をリクエスト
                // Request to purchase an item
                var domain = gs2.Showcase.Namespace(
                    namespaceName: showcaseNamespaceName
                ).Me(
                    gameSession: gameSession
                ).Showcase(
                    showcaseName: showcaseName
                );
                try
                {
                    var result = await domain.BuyAsync(
                        displayItemId: selectedProduct.Id,
                        config: new[]
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
                }
                catch (Gs2Exception e)
                {
                    onError.Invoke(e, null);
                    return e;
                }

                // 商品購入に成功
                // Successful product purchase
                onBuy.Invoke(selectedProduct);
                return null;
            }
        }
#endif

    }
}