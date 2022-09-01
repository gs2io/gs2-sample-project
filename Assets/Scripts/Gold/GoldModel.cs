using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gs2.Core.Exception;
using Gs2.Unity.Core;
using Gs2.Unity.Gs2Inventory.Model;
using Gs2.Unity.Util;
using UnityEngine;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
#endif

namespace Gs2.Sample.Gold
{
    public class GoldModel : MonoBehaviour
    {
        /// <summary>
        /// ゴールドのインベントリモデル
        /// Gold inventory model
        /// </summary>
        public EzInventoryModel InventoryModel;

        /// <summary>
        /// ゴールドのアイテムモデル
        /// Gold Item Model
        /// </summary>
        public List<EzItemModel> ItemModels = new List<EzItemModel>();
        
        /// <summary>
        /// ゴールドのインベントリ
        /// Gold Inventory
        /// </summary>
        public EzInventory Inventory;

        /// <summary>
        /// ゴールドのアイテムセット
        /// Gold Item Sets
        /// </summary>
        public List<EzItemSet> ItemSets = new List<EzItemSet>();

        /// <summary>
        /// インベントリモデルを取得
        /// Get Inventory Model
        /// </summary>
        /// <param name="gs2"></param>
        /// <param name="inventoryNamespaceName"></param>
        /// <param name="inventoryModelName"></param>
        /// <param name="onGetInventoryModel"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator GetInventoryModel(
            Gs2Domain gs2,
            string inventoryNamespaceName,
            string inventoryModelName,
            GetInventoryModelEvent onGetInventoryModel,
            ErrorEvent onError
        )
        {
            {
                var domain = gs2.Inventory.Namespace(
                    namespaceName: inventoryNamespaceName
                ).InventoryModel(
                    inventoryName: inventoryModelName
                );
                var future = domain.Model();
                yield return future;
                if (future.Error != null)
                {
                    onError.Invoke(future.Error, null);
                    yield break;
                }

                InventoryModel = future.Result;
            }
            {
                ItemModels.Clear();
                var it = gs2.Inventory.Namespace(
                    namespaceName: inventoryNamespaceName
                ).InventoryModel(
                    inventoryName: inventoryModelName
                ).ItemModels();
                while (it.HasNext())
                {
                    yield return it.Next();
                    if (it.Error != null)
                    {
                        onError.Invoke(it.Error, null);
                        break;
                    }

                    if (it.Current != null)
                    {
                        ItemModels.Add(it.Current);
                    }
                }
            }

            onGetInventoryModel.Invoke(inventoryModelName, InventoryModel, ItemModels);
        }
#if GS2_ENABLE_UNITASK
        /// <summary>
        /// インベントリモデルを取得
        /// Get Inventory Model
        /// </summary>
        /// <param name="client"></param>
        /// <param name="inventoryNamespaceName"></param>
        /// <param name="inventoryModelName"></param>
        /// <param name="onGetInventoryModel"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public async UniTask GetInventoryModelAsync(
            Gs2Domain gs2,
            string inventoryNamespaceName,
            string inventoryModelName,
            GetInventoryModelEvent onGetInventoryModel,
            ErrorEvent onError
        )
        {
            {
                var domain = gs2.Inventory.Namespace(
                    namespaceName: inventoryNamespaceName
                ).InventoryModel(
                    inventoryName: inventoryModelName
                );
                try
                {
                    InventoryModel = await domain.ModelAsync();
                }
                catch (Gs2Exception e)
                {
                    onError.Invoke(e, null);
                    return;
                }
            }
            {
                ItemModels.Clear();
                var domain = gs2.Inventory.Namespace(
                    namespaceName: inventoryNamespaceName
                ).InventoryModel(
                    inventoryName: inventoryModelName
                );
                await domain.ItemModelsAsync()
                    .ForEachAsync(item => { ItemModels.Add(item); });

            }

            onGetInventoryModel.Invoke(inventoryModelName, InventoryModel, ItemModels);
        }
#endif
        
        /// <summary>
        /// インベントリの情報を取得
        /// Retrieve inventory information
        /// </summary>
        public IEnumerator GetInventory(
            Gs2Domain gs2,
            GameSession gameSession,
            string inventoryNamespaceName,
            string inventoryName,
            GetInventoryEvent onGetInventory,
            ErrorEvent onError
        )
        {
            {
                var domain = gs2.Inventory.Namespace(
                    namespaceName: inventoryNamespaceName
                ).Me(
                    gameSession: gameSession
                ).Inventory(
                    inventoryName: inventoryName
                );
                var future = domain.Model();
                yield return future;
                if (future.Error != null)
                {
                    onError.Invoke(future.Error, null);
                    yield break;
                }
 
                Inventory = future.Result;
            }
            {
                ItemSets.Clear();
                var it = gs2.Inventory.Namespace(
                    namespaceName: inventoryNamespaceName
                ).Me(
                    gameSession: gameSession
                ).Inventory(
                    inventoryName: inventoryName
                ).ItemSets();
                while (it.HasNext())
                {
                    yield return it.Next();
                    if (it.Error != null)
                    {
                        onError.Invoke(it.Error, null);
                        break;
                    }

                    if (it.Current != null)
                    {
                        ItemSets.Add(it.Current);
                    }
                }
            }

            onGetInventory.Invoke(Inventory, ItemSets);
        }
#if GS2_ENABLE_UNITASK
        /// <summary>
        /// インベントリの情報を取得
        /// Retrieve inventory information
        /// </summary>
        public async UniTask GetInventoryAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string inventoryNamespaceName,
            string inventoryName,
            GetInventoryEvent onGetInventory,
            ErrorEvent onError
        )
        {
            {
                var domain = gs2.Inventory.Namespace(
                    namespaceName: inventoryNamespaceName
                ).Me(
                    gameSession: gameSession
                ).Inventory(
                    inventoryName: inventoryName
                );
                try
                {
                    Inventory = await domain.ModelAsync();
                }
                catch (Gs2Exception e)
                {
                    onError.Invoke(e, null);
                }
            }
            {
                var domain = gs2.Inventory.Namespace(
                    namespaceName: inventoryNamespaceName
                ).Me(
                    gameSession: gameSession
                ).Inventory(
                    inventoryName: inventoryName
                );
                ItemSets = await domain.ItemSetsAsync().ToListAsync();
            }
            
            onGetInventory.Invoke(Inventory, ItemSets);
        }
#endif
        
        /// <summary>
        /// ゴールドの入手
        /// Obtaining Gold
        /// </summary>
        public IEnumerator Acquire(
            Gs2Domain gs2,
            GameSession gameSession,
            string exchangeNamespaceName,
            string exchangeRateName,
            int value,
            string inventoryNamespaceName,
            string inventoryName,
            AcquireEvent onAcquire,
            ErrorEvent onError
        )
        {
            // ※この処理はサンプルの動作確認のためものです。
            // 実際にクライアントが直接ゴールドの増加を行う実装は非推奨となります。
            // *This process is only for sample confirmation.
            // The actual implementation in which the client directly increases the gold is deprecated.

            {
                var domain = gs2.Exchange.Namespace(
                    namespaceName: exchangeNamespaceName
                ).Me(
                    gameSession: gameSession
                ).Exchange();
                var future = domain.Exchange(
                    rateName: exchangeRateName,
                    count: value,
                    config: null
                );
                yield return future;
                if (future.Error != null)
                {
                    onError.Invoke(
                        future.Error,
                        null
                    );
                    yield break;
                }
            }
            {
                var domain = gs2.Inventory.Namespace(
                    namespaceName: inventoryNamespaceName
                ).Me(
                    gameSession: gameSession
                ).Inventory(
                    inventoryName: inventoryName
                );
                var future = domain.Model();
                yield return future;
                if (future.Error != null)
                {
                    onError.Invoke(future.Error, null);
                    yield break;
                }
 
                Inventory = future.Result;
            }
            {
                ItemSets.Clear();
                var it = gs2.Inventory.Namespace(
                    namespaceName: inventoryNamespaceName
                ).Me(
                    gameSession: gameSession
                ).Inventory(
                    inventoryName: inventoryName
                ).ItemSets();
                while (it.HasNext())
                {
                    yield return it.Next();
                    if (it.Error != null)
                    {
                        onError.Invoke(it.Error, null);
                        break;
                    }

                    if (it.Current != null)
                    {
                        ItemSets.Add(it.Current);
                    }
                }
            }

            onAcquire.Invoke(Inventory, ItemSets.ToList(), value);
        }
#if GS2_ENABLE_UNITASK
        /// <summary>
        /// アイテムの入手
        /// Obtaining Items
        /// </summary>
        public async UniTask AcquireAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string exchangeNamespaceName,
            string exchangeRateName,
            int value,
            string inventoryNamespaceName,
            string inventoryName,
            AcquireEvent onAcquire,
            ErrorEvent onError
        )
        {
            // ※この処理はサンプルの動作確認のためものです。
            // 実際にクライアントが直接アイテムの増加を行う実装は非推奨となります。
            // *This process is only for sample confirmation.
            // The actual implementation in which the client directly increases the number of items is deprecated.

            {
                var domain = gs2.Exchange.Namespace(
                    namespaceName: exchangeNamespaceName
                ).Me(
                    gameSession: gameSession
                ).Exchange();
                try
                {
                    await domain.ExchangeAsync(
                        rateName: exchangeRateName,
                        count: value,
                        config: null
                    );
                }
                catch (Gs2Exception e)
                {
                    onError.Invoke(e, null);
                    return;
                }
            }
            {
                var domain = gs2.Inventory.Namespace(
                    namespaceName: inventoryNamespaceName
                ).Me(
                    gameSession: gameSession
                ).Inventory(
                    inventoryName: inventoryName
                );
                try
                {
                    Inventory = await domain.ModelAsync();
                }
                catch (Gs2Exception e)
                {
                    onError.Invoke(e, null);
                    return;
                }
            }
            {
                var domain = gs2.Inventory.Namespace(
                    namespaceName: inventoryNamespaceName
                ).Me(
                    gameSession: gameSession
                ).Inventory(
                    inventoryName: inventoryName
                );
                ItemSets = await domain.ItemSetsAsync().ToListAsync();
                try
                {
                    ItemSets = await domain.ItemSetsAsync().ToListAsync();
                }
                catch (Gs2Exception e)
                {
                    onError.Invoke(e, null);
                    return;
                }
            }
            
            onAcquire.Invoke(Inventory, ItemSets.ToList(), value);

        }
#endif
        
        /// <summary>
        /// ゴールドの消費
        /// Gold Consumption
        /// </summary>
        public IEnumerator Consume(
            Gs2Domain gs2,
            GameSession gameSession,
            string inventoryNamespaceName,
            string inventoryName,
            string itemName,
            int consumeValue,
            ConsumeEvent onConsume,
            ErrorEvent onError
        )
        {
            var future = gs2.Inventory.Namespace(
                namespaceName: inventoryNamespaceName
            ).Me(
                gameSession: gameSession
            ).Inventory(
                inventoryName: inventoryName
            ).ItemSet(
                itemName: itemName,
                itemSetName: null
            ).Consume(
                consumeCount: consumeValue
            );
            yield return future;
            if (future.Error != null)
            {
                onError.Invoke(future.Error, null);
                yield break;
            }

            var result = future.Result;
            var future2 = result.Model();
            yield return future2;
            if (future2.Error != null)
            {
                onError.Invoke(future2.Error, null);
                yield break;
            }

            var itemSets = future2.Result;

            onConsume.Invoke(Inventory, itemSets.ToList(), consumeValue);
        }
#if GS2_ENABLE_UNITASK
        /// <summary>
        /// アイテムの消費
        /// Item Consumption
        /// </summary>
        public async UniTask ConsumeAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string inventoryNamespaceName,
            string inventoryName,
            string itemName,
            int consumeValue,
            ConsumeEvent onConsume,
            ErrorEvent onError
        )
        {
            var domain = gs2.Inventory.Namespace(
                namespaceName: inventoryNamespaceName
            ).Me(
                gameSession: gameSession
            ).Inventory(
                inventoryName: inventoryName
            ).ItemSet(
                itemName: itemName,
                itemSetName: null
            );
            try
            {
                var result = await domain.ConsumeAsync(
                    consumeCount: consumeValue
                );
                
                var itemSets  = await result.ModelAsync();

                onConsume.Invoke(Inventory, itemSets.ToList(), consumeValue);
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e, null);
            }
        }
#endif
    }
}