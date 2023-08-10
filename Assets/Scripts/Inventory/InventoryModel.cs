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

namespace Gs2.Sample.Inventory
{
    public class InventoryModel : MonoBehaviour
    {
        /// <summary>
        /// インベントリモデル
        /// inventory model
        /// </summary>
        public EzInventoryModel Model;
        
        /// <summary>
        /// アイテムモデル
        /// Item Model
        /// </summary>
        public List<EzItemModel> ItemModels;
        
        /// <summary>
        /// インベントリ
        /// inventory
        /// </summary>
        public EzInventory Inventory;
        
        /// <summary>
        /// アイテムセット
        /// item set
        /// </summary>
        public List<EzItemSet> ItemSets = new List<EzItemSet>();

        /// <summary>
        /// インベントリモデル/アイテムモデルを取得
        /// Get Inventory Model
        /// </summary>
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

                Model = future.Result;
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

            onGetInventoryModel.Invoke(inventoryModelName, Model, ItemModels);
        }
#if GS2_ENABLE_UNITASK
        /// <summary>
        /// インベントリモデル/アイテムモデルを取得
        /// Get Inventory Model
        /// </summary>
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
                    Model = await domain.ModelAsync();
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
                ItemModels = await domain.ItemModelsAsync().ToListAsync();
            }

            onGetInventoryModel.Invoke(inventoryModelName, Model, ItemModels);
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
        /// アイテムの入手
        /// Obtaining Items
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
            // 実際にクライアントが直接アイテムの増加を行う実装は非推奨となります。
            // *This process is only for sample confirmation.
            // The actual implementation in which the client directly increases the number of items is deprecated.

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
        /// アイテムの消費
        /// Item Consumption
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
            var domain = gs2.Inventory.Namespace(
                namespaceName: inventoryNamespaceName
            ).Me(
                gameSession: gameSession
            ).Inventory(
                inventoryName: inventoryName
            );
            var future = domain.ItemSet(
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
            
            var future3 = domain.Model();
            yield return future3;
            if (future3.Error != null)
            {
                onError.Invoke(future3.Error, null);
                yield break;
            }
 
            Inventory = future3.Result;

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
            EzItemSet[] itemSets;
            var domain = gs2.Inventory.Namespace(
                namespaceName: inventoryNamespaceName
            ).Me(
                gameSession: gameSession
            ).Inventory(
                inventoryName: inventoryName
            );
            var domain2 = domain.ItemSet(
                itemName: itemName,
                itemSetName: null
            );
            try
            {
                var result = await domain2.ConsumeAsync(
                    consumeCount: consumeValue
                );
                
                itemSets  = await result.ModelAsync();
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e, null);
                return;
            }
            try
            {
                Inventory = await domain.ModelAsync();
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e, null);
                return;
            }

            onConsume.Invoke(Inventory, itemSets.ToList(), consumeValue);
        }
#endif
    }
}