using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gs2.Core;
using Gs2.Core.Exception;
using Gs2.Gs2Limit.Request;
using Gs2.Gs2Money2.Request;
using Gs2.Sample.Core;
using Gs2.Sample.Money2Store;
using Gs2.Unity.Core;
using Gs2.Unity.Gs2Money2.Model;
using Gs2.Unity.Gs2Showcase.Model;
using Gs2.Unity.Util;
using Gs2.Util.LitJson;
using UnityEngine;
using UnityEngine.Events;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
#endif

namespace Gs2.Sample.Money2
{
    public class Money2Model : MonoBehaviour
    {
        public EzWallet Wallet;

#if UNITY_IPHONE
        public const int Slot = 1;
#elif UNITY_ANDROID
        public const int Slot = 2;
#else
        public const int Slot = 0;
#endif

        // Money2 のレシート検証で使用するストア名 / Store name used for Money2 receipt verification
#if UNITY_IPHONE
        private const string StoreName = "AppleAppStore";
#elif UNITY_ANDROID
        private const string StoreName = "GooglePlay";
#else
        private const string StoreName = "fake";
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
            ErrorEvent onError,
            bool invalidateCache = false
        )
        {
            var domain = gs2.Money2.Namespace(
                namespaceName: moneyNamespaceName
            ).Me(
                gameSession: gameSession
            ).Wallet(
                slot: Slot
            );
            if (invalidateCache)
            {
                // 購入や報酬の受け取り直後はキャッシュを破棄し、サーバーの最新値を取得する
                // Discard the cache right after a purchase/reward so that the latest server value is fetched
                domain.Invalidate();
            }
            var future = domain.ModelFuture();
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
            ErrorEvent onError,
            bool invalidateCache = false
        )
        {
            var domain = gs2.Money2.Namespace(
                namespaceName: moneyNamespaceName
            ).Me(
                gameSession: gameSession
            ).Wallet(
                slot: Slot
            );
            if (invalidateCache)
            {
                // 購入や報酬の受け取り直後はキャッシュを破棄し、サーバーの最新値を取得する
                // Discard the cache right after a purchase/reward so that the latest server value is fetched
                domain.Invalidate();
            }
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
            var future = domain.ModelFuture();
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
                    "Gs2Money2:DepositByUserId"
                );
                var verifyReceiptRequest = GetConsumeAction<VerifyReceiptByUserIdRequest>(
                    displayItem.SalesItem,
                    "Gs2Money2:VerifyReceiptByUserId"
                );
                var countUpRequest = GetConsumeAction<CountUpByUserIdRequest>(
                    displayItem.SalesItem,
                    "Gs2Limit:CountUpByUserId"
                );
                // Money2 では価格・通貨量は depositTransactions に格納される
                // In Money2, the price and currency amount are stored in depositTransactions
                var depositTransaction = depositRequest.DepositTransactions != null && depositRequest.DepositTransactions.Length > 0
                    ? depositRequest.DepositTransactions[0]
                    : null;
                var price = (float?)depositTransaction?.Price;
                var count = depositTransaction?.Count;

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
                    var future2 = domain2.ModelFuture();
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
                    ContentsId = verifyReceiptRequest.ContentName,
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
                        "Gs2Money2:DepositByUserId"
                    );
                    var verifyReceiptRequest = GetConsumeAction<VerifyReceiptByUserIdRequest>(
                        displayItem.SalesItem,
                        "Gs2Money2:VerifyReceiptByUserId"
                    );
                    var countUpRequest = GetConsumeAction<CountUpByUserIdRequest>(
                        displayItem.SalesItem,
                        "Gs2Limit:CountUpByUserId"
                    );
                    // Money2 では価格・通貨量は depositTransactions に格納される
                    // In Money2, the price and currency amount are stored in depositTransactions
                    var depositTransaction = depositRequest.DepositTransactions != null && depositRequest.DepositTransactions.Length > 0
                        ? depositRequest.DepositTransactions[0]
                        : null;
                    var price = (float?)depositTransaction?.Price;
                    var count = depositTransaction?.Count;

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
                        ContentsId = verifyReceiptRequest.ContentName,
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
            // 既定はフェイクレシート（購入機能が無効な場合に使用。acceptFakeReceipt: Accept）
            // Default is a fake receipt (used when the purchasing feature is disabled. acceptFakeReceipt: Accept)
            string store = "fake";
            string payload = "fake";
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

                // 課金通貨商品購入 レシート情報を Money2 のレシート項目に設定
                // Set the purchased receipt information as Money2 receipt fields
                store = StoreName;
                payload = result.Result.receipt;
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
                ).DisplayItem(
                    displayItemId: selectedProduct.Id
                );
                var future = domain.BuyFuture(
                    quantity: 1,
                    config: new []
                    {
                        new EzConfig
                        {
                            Key = "slot",
                            Value = Slot.ToString(),
                        },
                        new EzConfig
                        {
                            Key = "store",
                            Value = store,
                        },
                        new EzConfig
                        {
                            Key = "transactionId",
                            Value = System.Guid.NewGuid().ToString(),
                        },
                        new EzConfig
                        {
                            Key = "payload",
                            Value = payload,
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

                // トランザクションの自動実行の完了を待機（連鎖するトランザクションも含めて全て待つ）
                // Wait for automatic transaction execution to complete (including all chained transactions)
                var waitFuture = future.Result.WaitFuture(true);
                yield return waitFuture;

                if (waitFuture.Error != null)
                {
                    onError.Invoke(
                        waitFuture.Error,
                        null
                    );
                    callback.Invoke(
                        waitFuture.Error
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
            // 既定はフェイクレシート（購入機能が無効な場合に使用。acceptFakeReceipt: Accept）
            // Default is a fake receipt (used when the purchasing feature is disabled. acceptFakeReceipt: Accept)
            string store = "fake";
            string payload = "fake";
            {
#if GS2_ENABLE_PURCHASING
                try
                {
                    PurchaseParameters result = await new IAPUtil().BuyAsync(
                        selectedProduct.ContentsId
                    );

                    // 課金通貨商品購入 レシート情報を Money2 のレシート項目に設定
                    // Set the purchased receipt information as Money2 receipt fields
                    store = StoreName;
                    payload = result.receipt;
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
                ).DisplayItem(
                    displayItemId: selectedProduct.Id
                );
                try
                {
                    var result = await domain.BuyAsync(
                        quantity: 1,
                        config: new[]
                        {
                            new EzConfig
                            {
                                Key = "slot",
                                Value = Slot.ToString(),
                            },
                            new EzConfig
                            {
                                Key = "store",
                                Value = store,
                            },
                            new EzConfig
                            {
                                Key = "transactionId",
                                Value = System.Guid.NewGuid().ToString(),
                            },
                            new EzConfig
                            {
                                Key = "payload",
                                Value = payload,
                            },
                        }
                    );
                    // トランザクションの自動実行の完了を待機（連鎖するトランザクションも含めて全て待つ）
                    // Wait for automatic transaction execution to complete (including all chained transactions)
                    await result.WaitAsync(true);
                }
                catch (Gs2Exception e)
                {
                    onError.Invoke(e, null);
                    return e;
                }
                catch (System.Exception e)
                {
                    // トランザクション結果の取得失敗（TimeoutException など）も購入失敗として扱う
                    // Treat a failure to obtain the transaction result (TimeoutException, etc.) as a purchase failure
                    var error = new UnknownException(e.Message);
                    onError.Invoke(error, null);
                    return error;
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