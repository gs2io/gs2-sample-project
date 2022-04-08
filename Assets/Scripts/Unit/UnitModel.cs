using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gs2.Core;
using Gs2.Core.Model;
using Gs2.Core.Net;
using Gs2.Gs2Inventory;
using Gs2.Gs2Inventory.Request;
using Gs2.Sample.Inventory;
using Gs2.Unity;
using Gs2.Unity.Gs2Inventory.Model;
using Gs2.Unity.Gs2Inventory.Result;
using Gs2.Unity.Util;
using UnityEngine;

namespace Gs2.Sample.Unit
{
    public class UnitModel : MonoBehaviour
    {
        public EzInventoryModel Model;
        
        public List<EzItemModel> ItemModels;
        
        public EzInventory Inventory;
        
        public List<EzItemSet> ItemSets = new List<EzItemSet>();
        
        public IEnumerator GetInventoryModel(
            Client client,
            string inventoryNamespaceName,
            string inventoryModelName,
            GetInventoryModelEvent onGetInventoryModel,
            ErrorEvent onError
        )
        {
            EzInventoryModel inventoryModel;
            {
                AsyncResult<EzGetInventoryModelResult> result = null;
                yield return client.Inventory.GetInventoryModel(
                    r => { result = r; },
                    inventoryNamespaceName,
                    inventoryModelName
                );

                if (result.Error != null)
                {
                    onError.Invoke(
                        result.Error
                    );
                    yield break;
                }

                inventoryModel = result.Result.Item;
            }
            List<EzItemModel> itemModels;
            {
                AsyncResult<EzListItemModelsResult> result = null;
                yield return client.Inventory.ListItemModels(
                    r => { result = r; },
                    inventoryNamespaceName,
                    inventoryModelName
                );

                if (result.Error != null)
                {
                    onError.Invoke(
                        result.Error
                    );
                    yield break;
                }

                itemModels = result.Result.Items;
            }

            onGetInventoryModel.Invoke(inventoryModelName, inventoryModel, itemModels);
        }
        
        /// <summary>
        /// インベントリ名を指定してインベントリの情報を取得
        /// Get inventory information by specifying the inventory name
        /// </summary>
        /// <param name="client"></param>
        /// <param name="session"></param>
        /// <param name="inventoryNamespaceName"></param>
        /// <param name="inventoryName"></param>
        /// <param name="onGetInventory"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator GetInventory(
            Client client,
            GameSession session,
            string inventoryNamespaceName,
            string inventoryName,
            GetInventoryEvent onGetInventory,
            ErrorEvent onError
        )
        {
            EzInventory inventory;
            {
                AsyncResult<EzGetInventoryResult> result = null;
                yield return client.Inventory.GetInventory(
                    r => { result = r; },
                    session,
                    inventoryNamespaceName,
                    inventoryName
                );

                if (result.Error != null)
                {
                    onError.Invoke(
                        result.Error
                    );
                    yield break;
                }

                inventory = result.Result.Item;
            }
            var itemSets = new List<EzItemSet>();
            string nextPageToken;
            do
            {
                AsyncResult<EzListItemsResult> result = null;
                yield return client.Inventory.ListItems(
                    r => { result = r; },
                    session,
                    inventoryNamespaceName,
                    inventoryName,
                    null,
                    30
                );

                if (result.Error != null)
                {
                    onError.Invoke(
                        result.Error
                    );
                    yield break;
                }

                itemSets.AddRange(result.Result.Items);
                nextPageToken = result.Result.NextPageToken;
            } while (nextPageToken != null);

            onGetInventory.Invoke(inventory, itemSets);
        }

        public IEnumerator Acquire(
            GameSession session,
            string identifierAcquireItemClientId,
            string identifierAcquireItemClientSecret,
            string inventoryNamespaceName,
            string inventoryModelName,
            string itemModelName,
            int value,
            AcquireEvent onAcquire,
            ErrorEvent onError
        )
        {
            // ※この処理はサンプルの動作確認のためものです。
            // 実際にクライアントが直接ゴールドの増加を行う実装は非推奨となります。
            // *This process is only for sample confirmation.
            // The actual implementation in which the client directly increases the gold is deprecated.
            
            var restSession = new Gs2RestSession(
                new BasicGs2Credential(
                    identifierAcquireItemClientId,
                    identifierAcquireItemClientSecret
                )
            );
            var error = false;
            yield return restSession.Open(
                r =>
                {
                    if (r.Error != null)
                    {
                        onError.Invoke(r.Error);
                        error = true;
                    }
                }
            );

            if (error)
            {
                yield return restSession.Close(() => { });
                yield break;
            }

            var restClient = new Gs2InventoryRestClient(
                restSession
            );

            yield return restClient.AcquireItemSetByUserId(
                new AcquireItemSetByUserIdRequest()
                    .WithNamespaceName(inventoryNamespaceName)
                    .WithUserId(session.AccessToken.UserId)
                    .WithInventoryName(inventoryModelName)
                    .WithItemName(itemModelName)
                    .WithAcquireCount(value),
                r =>
                {
                    if (r.Error != null)
                    {
                        onError.Invoke(r.Error);
                        error = true;
                    }
                    else
                    {
                        onAcquire.Invoke(
                            EzInventory.FromModel(r.Result.Inventory),
                            r.Result.Items.Select(item => EzItemSet.FromModel(item)).ToList(),
                            value
                        );
                    }
                }
            );

            yield return restSession.Close(() => { });
        }

        public IEnumerator Consume(
            Client client,
            GameSession session,
            string inventoryNamespaceName,
            string inventoryModelName,
            string itemModelName,
            int consumeValue,
            ConsumeEvent onConsume,
            ErrorEvent onError,
            string itemSetName = null
        )
        {
            AsyncResult<EzConsumeResult> result = null;
            yield return client.Inventory.Consume(
                r => { result = r; },
                session,
                inventoryNamespaceName,
                inventoryModelName,
                itemModelName,
                consumeValue,
                itemSetName
            );
            
            if (result.Error != null)
            {
                onError.Invoke(
                    result.Error
                );
                yield break;
            }

            var inventory = result.Result.Inventory;
            var itemSets = result.Result.Items;

            onConsume.Invoke(inventory, itemSets, consumeValue);
        }
    }
}