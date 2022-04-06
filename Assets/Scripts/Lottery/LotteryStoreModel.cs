using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gs2.Core;
using Gs2.Core.Exception;
using Gs2.Gs2Limit.Request;
using Gs2.Gs2Money.Request;
using Gs2.Sample.Core;
using Gs2.Sample.Money;
using Gs2.Unity;
using Gs2.Unity.Gs2Limit.Result;
using Gs2.Unity.Gs2Showcase.Model;
using Gs2.Unity.Gs2Showcase.Result;
using Gs2.Unity.Util;
using Gs2.Util.LitJson;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using Product = Gs2.Sample.Money.Product;

namespace Gs2.Sample.Lottery
{
    public class LotteryStoreModel : MonoBehaviour
    {
        /// <summary>
        /// 販売中のガチャ
        /// </summary>
        public EzShowcase Showcase { get; private set; }

        public List<EzDisplayItem> DisplayItems => Showcase.DisplayItems;

        /// <summary>
        /// 選択したガチャ
        /// </summary>
        public SalesItem selectedItem;
        
        /// <summary>
        /// 商品棚を取得
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="client"></param>
        /// <param name="session"></param>
        /// <param name="showcaseNamespaceName"></param>
        /// <param name="showcaseName"></param>
        /// <param name="onGetShowcase"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator GetShowcase(
            UnityAction<AsyncResult<EzGetShowcaseResult>> callback,
            Client client,
            GameSession session,
            string showcaseNamespaceName,
            string showcaseName,
            GetShowcaseEvent onGetShowcase,
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
                callback.Invoke(new AsyncResult<EzGetShowcaseResult>(null, result.Error));
                yield break;
            }

            Showcase = result.Result.Item;

            onGetShowcase.Invoke(Showcase);
            
            callback.Invoke(new AsyncResult<EzGetShowcaseResult>(null, result.Error));
        }
        
        /// <summary>
        /// 入手アクション取得
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
        /// 消費アクション取得
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
        /// ガチャを購入する
        /// </summary>
        public IEnumerator Buy(
            Client client,
            GameSession session,
            string showcaseNamespaceName,
            string showcaseName,
            string displayItemId,
            IssueBuyStampSheetEvent onIssueBuyStampSheet,
            ErrorEvent onError,
            List<EzConfig> config,
            string contentsId = null
        )
        {
            var tempConfig = new List<EzConfig>(config);
#if GS2_ENABLE_PURCHASING
            IStoreController controller = null;
            UnityEngine.Purchasing.Product product = null;
            string receipt = null;
            if (contentsId != null)
            {
                AsyncResult<Gs2.Unity.Util.PurchaseParameters> result = null;
                yield return new IAPUtil().Buy(
                    r => { result = r; },
                    contentsId
                );

                if (result.Error != null)
                {
                    onError.Invoke(
                        result.Error
                    );
                    yield break;
                }

                receipt = result.Result.receipt;
                controller = result.Result.controller;
                product = result.Result.product;
            }
            
            // ガチャ購入 レシート情報
            if (receipt != null)
            {
                tempConfig.Add(
                    new EzConfig
                    {
                        Key = "receipt", 
                        Value = receipt
                    }
                );
                
                UIManager.Instance.AddLog("receipt:" + receipt);
            }
#else
            Debug.LogError("課金通貨・ガチャ機能の実行には Unity Purchasing を有効にしてください。");
            throw new InvalidProgramException("課金通貨・ガチャ機能の実行には Unity Purchasing を有効にしてください。");
#endif

            string stampSheet = null;
            {
                // Showcase 商品の購入をリクエスト
                AsyncResult<EzBuyResult> result = null;
                yield return client.Showcase.Buy(
                    r => { result = r; },
                    session,
                    showcaseNamespaceName,
                    showcaseName,
                    displayItemId,
                    tempConfig
                );

                if (result.Error != null)
                {
                    onError.Invoke(
                        result.Error
                    );
                    yield break;
                }
                
                // スタンプシートを取得
                stampSheet = result.Result.StampSheet;
            }

            // スタンプシートを実行
            onIssueBuyStampSheet.Invoke(stampSheet);

#if GS2_ENABLE_PURCHASING
            if (controller != null)
                controller.ConfirmPendingPurchase(product);
#endif
        }
    }
}