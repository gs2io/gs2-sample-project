using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gs2.Core.Exception;
using Gs2.Sample.Inventory;
using Gs2.Unity.Core;
using Gs2.Unity.Gs2Inventory.Model;
using Gs2.Unity.Util;
using UnityEngine;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
#endif

namespace Gs2.Sample.Unit
{
    public class UnitModel : MonoBehaviour
    {
        public EzInventoryModel Model;
        
        public List<EzItemModel> ItemModels;
        
        public EzInventory Inventory;
        
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
                var future = domain.ModelFuture();
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
        /// インベントリモデルを取得
        /// Get Inventory Model
        /// </summary>
        /// <param name="gs2"></param>
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
                await domain.ItemModelsAsync()
                    .ForEachAsync(item => { ItemModels.Add(item); });

            }

            onGetInventoryModel.Invoke(inventoryModelName, Model, ItemModels);
        }
#endif
        
        /// <summary>
        /// インベントリ名を指定してインベントリの情報を取得
        /// Get inventory information by specifying the inventory name
        /// </summary>
        /// <param name="gs2"></param>
        /// <param name="session"></param>
        /// <param name="inventoryNamespaceName"></param>
        /// <param name="inventoryName"></param>
        /// <param name="onGetInventory"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
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
                var future = domain.ModelFuture();
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
            ).ConsumeFuture(
                consumeCount: consumeValue
            );
            yield return future;
            if (future.Error != null)
            {
                onError.Invoke(future.Error, null);
                yield break;
            }
            
            var result = future.Result;
            var future2 = result.ModelFuture();
            yield return future2;
            if (future2.Error != null)
            {
                onError.Invoke(future2.Error, null);
                yield break;
            }
            var itemSets = future2.Result;
            
            var future3 = domain.ModelFuture();
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
                return;
            }

        }
#endif
    }
}