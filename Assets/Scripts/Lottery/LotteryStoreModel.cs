using System.Collections;
using System.Collections.Generic;
using Gs2.Core.Exception;
using Gs2.Gs2Lottery.Model;
using Gs2.Gs2Lottery.Request;
using Gs2.Gs2Lottery.Result;
using Gs2.Unity.Core;
using Gs2.Unity.Gs2Lottery.Model;
using Gs2.Unity.Gs2Showcase.Model;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Events;
using EzConfig = Gs2.Unity.Gs2Showcase.Model.EzConfig;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
#endif

namespace Gs2.Sample.Lottery
{
    public class LotteryStoreModel : MonoBehaviour
    {
        /// <summary>
        /// 販売中の抽選商品
        /// Lottery items on sale
        /// </summary>
        public EzShowcase Showcase { get; private set; }

        public List<EzDisplayItem> DisplayItems => Showcase.DisplayItems;

        /// <summary>
        /// 選択した抽選商品
        /// Selected Lottery Items
        /// </summary>
        public SalesItem selectedItem;
        
        /// <summary>
        /// 商品棚を取得
        /// Retrieve product shelves
        /// </summary>
        public IEnumerator GetShowcase(
            UnityAction<Gs2Exception> callback,
            Gs2Domain gs2,
            GameSession gameSession,
            string showcaseNamespaceName,
            string showcaseName,
            GetShowcaseEvent onGetShowcase,
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
                callback.Invoke(future.Error);
                yield break;
            }

            Showcase = future.Result;
            
            onGetShowcase.Invoke(Showcase);
            
            callback.Invoke(null);
        }
#if GS2_ENABLE_UNITASK
        public async UniTask<Gs2Exception> GetShowcaseAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string showcaseNamespaceName,
            string showcaseName,
            GetShowcaseEvent onGetShowcase,
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
                Showcase = await domain.ModelAsync();
                
                onGetShowcase.Invoke(Showcase);
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e, null);
                return e;
            }
            return null;
        }
#endif
        
        /// <summary>
        /// 抽選商品を購入する
        /// Purchase Lottery Items
        /// </summary>
        public IEnumerator Buy(
            Gs2Domain gs2,
            GameSession gameSession,
            string showcaseNamespaceName,
            string showcaseName,
            string displayItemId,
            AcquireInventoryItemEvent onAcquireInventoryItem,
            ErrorEvent onError,
            List<EzConfig> config
        )
        {
            var tempConfig = new List<EzConfig>(config);
            
            // 抽選処理の結果を取得
            // Obtain the results of the lottery process
            void LotteryResult(
                string _namespace,
                DrawByUserIdRequest request,
                DrawByUserIdResult result
            )
            {
                // 抽選で獲得したアイテム
                // Items won in the lottery
                var DrawnPrizes = new List<EzDrawnPrize>();
                if (result == null) return;
                var prizes = result.Items;
                foreach (var prize in prizes)
                {
                    var item = EzDrawnPrize.FromModel(prize);
                    DrawnPrizes.Add(item);
                }

                onAcquireInventoryItem.Invoke(
                    DrawnPrizes
                );
            }
            
            // 抽選結果取得コールバックを登録
            // Register lottery result acquisition callback
            Gs2Lottery.Domain.Gs2Lottery.DrawByUserIdComplete.AddListener( LotteryResult );
            
            // 商品の購入をリクエスト
            // Request to purchase an item
            var domain = gs2.Showcase.Namespace(
                namespaceName: showcaseNamespaceName
            ).Me(
                gameSession: gameSession
            ).Showcase(
                showcaseName: showcaseName
            ).DisplayItem(
                displayItemId: displayItemId
            );
            var future = domain.BuyFuture(
                config: tempConfig.ToArray()
            );
            yield return future;
            if (future.Error != null)
            {
                onError.Invoke(
                    future.Error,
                    null
                );
            }
            
            Gs2Lottery.Domain.Gs2Lottery.DrawByUserIdComplete.RemoveListener( LotteryResult );
        }
#if GS2_ENABLE_UNITASK
        /// <summary>
        /// 抽選商品を購入する
        /// Purchase Lottery Items
        /// </summary>
        public async UniTask BuyAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string showcaseNamespaceName,
            string showcaseName,
            string displayItemId,
            AcquireInventoryItemEvent onAcquireInventoryItem,
            ErrorEvent onError,
            List<EzConfig> config
        )
        {
            var tempConfig = new List<EzConfig>(config);

            // 抽選処理の結果を取得
            // Obtain the results of the lottery process
            void LotteryResult(
                string _namespace,
                DrawByUserIdRequest request,
                DrawByUserIdResult result
            )
            {
                // 抽選で獲得したアイテム
                // Items won in the lottery
                var DrawnPrizes = new List<EzDrawnPrize>();
                if (result == null) return;
                var prizes = result.Items;
                foreach (var prize in prizes)
                {
                    var item = EzDrawnPrize.FromModel(prize);
                    DrawnPrizes.Add(item);
                }

                onAcquireInventoryItem.Invoke(
                    DrawnPrizes
                );
            }
            
            // 抽選結果取得コールバックを登録
            // Register lottery result acquisition callback
            Gs2Lottery.Domain.Gs2Lottery.DrawByUserIdComplete.AddListener( LotteryResult );
            
            // 商品の購入をリクエスト
            // Request to purchase an item
            var domain = gs2.Showcase.Namespace(
                namespaceName: showcaseNamespaceName
            ).Me(
                gameSession: gameSession
            ).Showcase(
                showcaseName: showcaseName
            ).DisplayItem(
                displayItemId: displayItemId
            );
            try
            {
                var result = await domain.BuyAsync(
                    quantity: null,
                    config: tempConfig.ToArray()
                );
                await result.WaitAsync();
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e, null);
                return;
            }
            
            Gs2Lottery.Domain.Gs2Lottery.DrawByUserIdComplete.RemoveListener( LotteryResult );
        }
#endif
    }
}