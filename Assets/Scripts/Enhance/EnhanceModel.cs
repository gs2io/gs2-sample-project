using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gs2.Core.Exception;
using Gs2.Unity.Core;
using Gs2.Unity.Gs2Enhance.Model;
using Gs2.Unity.Gs2Experience.Model;
using Gs2.Unity.Gs2Inventory.Model;
using Gs2.Unity.Util;
using Gs2.Util.LitJson;
using UnityEngine;
using UnityEngine.Events;
using EzConfig = Gs2.Unity.Gs2Exchange.Model.EzConfig;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
#endif

namespace Gs2.Sample.Enhance
{
    /// <summary>
    /// アイテムモデルのメタデータ
    /// Metadata of the item model
    /// </summary>
    public class EnhanceItemMetadata
    {
        /// <summary>
        /// 素材1個あたりに加算される経験値
        /// Experience added per material
        /// </summary>
        public int experience;
    }

    public class EnhanceModel : MonoBehaviour
    {
        /// <summary>
        /// 強化対象（キャラクター）のアイテムセット
        /// Item sets of the enhancement targets (characters)
        /// </summary>
        public List<EzItemSet> Characters = new List<EzItemSet>();

        /// <summary>
        /// 強化素材のアイテムセット
        /// Item sets of the enhancement materials
        /// </summary>
        public List<EzItemSet> Materials = new List<EzItemSet>();

        /// <summary>
        /// 強化対象のステータス（ランク・経験値）
        /// Statuses (rank and experience) of the enhancement targets
        /// </summary>
        public List<EzStatus> Statuses = new List<EzStatus>();

        /// <summary>
        /// 強化素材のアイテムモデル
        /// Item models of the enhancement materials
        /// </summary>
        public List<EzItemModel> MaterialItemModels = new List<EzItemModel>();

        /// <summary>
        /// 強化対象の経験値モデル（ランクのしきい値の取得に使用）
        /// Experience model of the enhancement target (used to read the rank thresholds)
        /// </summary>
        public EzExperienceModel ExperienceModel;

        /// <summary>
        /// ランク1からランク2に上がるために必要な経験値
        /// Experience required to go from rank 1 to rank 2
        ///
        /// 一度も強化していない強化対象にはステータスが存在せず、
        /// EzStatus.NextRankUpExperienceValue が参照できないため経験値モデルから取得します。
        /// A target that has never been enhanced has no status, so EzStatus.NextRankUpExperienceValue
        /// is not available and the value is read from the experience model instead.
        /// </summary>
        public long FirstRankUpExperience =>
            ExperienceModel?.RankThreshold?.Values == null || ExperienceModel.RankThreshold.Values.Count == 0
                ? 0
                : ExperienceModel.RankThreshold.Values[0];

        /// <summary>
        /// 強化対象のアイテムセットに対応するステータスを返す
        /// Returns the status that corresponds to the item set of the enhancement target
        ///
        /// ※GS2-Enhance は強化対象のアイテムセットID に強化レートの acquireExperienceSuffix を
        /// 連結したものを GS2-Experience のプロパティID として経験値を加算します。
        /// 本サンプルの強化レート `level` は acquireExperienceSuffix に `:level` を設定しているため、
        /// プロパティID は `<アイテムセットID>:level` になります。
        /// *GS2-Enhance adds experience using the item set ID of the target concatenated with the
        /// acquireExperienceSuffix of the rate as the property ID of GS2-Experience.
        /// The `level` rate of this sample sets `:level` as its acquireExperienceSuffix, so the
        /// property ID is `<itemSetId>:level`.
        /// </summary>
        public EzStatus FindStatus(EzItemSet itemSet)
        {
            if (itemSet == null || string.IsNullOrEmpty(itemSet.ItemSetId))
            {
                return null;
            }

            return Statuses.FirstOrDefault(
                status => status.PropertyId == itemSet.ItemSetId
                          || status.PropertyId.StartsWith(itemSet.ItemSetId + ":")
            );
        }

        /// <summary>
        /// 強化対象の経験値モデルを取得
        /// Get the experience model of the enhancement target
        /// </summary>
        public IEnumerator GetExperienceModel(
            Gs2Domain gs2,
            string experienceNamespaceName,
            string experienceModelName,
            ErrorEvent onError
        )
        {
            var future = gs2.Experience.Namespace(
                namespaceName: experienceNamespaceName
            ).ExperienceModel(
                experienceName: experienceModelName
            ).ModelFuture();
            yield return future;
            if (future.Error != null)
            {
                onError.Invoke(future.Error, null);
                yield break;
            }

            ExperienceModel = future.Result;
        }
#if GS2_ENABLE_UNITASK
        public async UniTask GetExperienceModelAsync(
            Gs2Domain gs2,
            string experienceNamespaceName,
            string experienceModelName,
            ErrorEvent onError
        )
        {
            try
            {
                ExperienceModel = await gs2.Experience.Namespace(
                    namespaceName: experienceNamespaceName
                ).ExperienceModel(
                    experienceName: experienceModelName
                ).ModelAsync();
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e, null);
            }
        }
#endif

        /// <summary>
        /// 強化素材1個あたりに加算される経験値を返す
        /// Returns the experience added per enhancement material
        /// </summary>
        public int GetMaterialExperience(EzItemSet material)
        {
            if (material == null)
            {
                return 0;
            }

            var itemModel = MaterialItemModels.FirstOrDefault(model => model.Name == material.ItemName);
            if (itemModel == null || string.IsNullOrEmpty(itemModel.Metadata))
            {
                return 0;
            }

            var metadata = JsonMapper.ToObject<EnhanceItemMetadata>(itemModel.Metadata);
            return metadata?.experience ?? 0;
        }

        /// <summary>
        /// 強化素材のアイテムモデルを取得
        /// Get the item models of the enhancement materials
        /// </summary>
        public IEnumerator GetMaterialItemModels(
            Gs2Domain gs2,
            string inventoryNamespaceName,
            string materialInventoryModelName,
            ErrorEvent onError
        )
        {
            MaterialItemModels.Clear();

            var it = gs2.Inventory.Namespace(
                namespaceName: inventoryNamespaceName
            ).InventoryModel(
                inventoryName: materialInventoryModelName
            ).ItemModels();
            while (it.HasNext())
            {
                yield return it.Next();
                if (it.Error != null)
                {
                    onError.Invoke(it.Error, null);
                    yield break;
                }

                if (it.Current != null)
                {
                    MaterialItemModels.Add(it.Current);
                }
            }
        }
#if GS2_ENABLE_UNITASK
        public async UniTask GetMaterialItemModelsAsync(
            Gs2Domain gs2,
            string inventoryNamespaceName,
            string materialInventoryModelName,
            ErrorEvent onError
        )
        {
            MaterialItemModels.Clear();

            try
            {
                MaterialItemModels = await gs2.Inventory.Namespace(
                    namespaceName: inventoryNamespaceName
                ).InventoryModel(
                    inventoryName: materialInventoryModelName
                ).ItemModelsAsync().ToListAsync();
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e, null);
            }
        }
#endif

        /// <summary>
        /// 強化対象（キャラクター）のアイテムセットを取得
        /// Get the item sets of the enhancement targets (characters)
        /// </summary>
        public IEnumerator GetCharacters(
            Gs2Domain gs2,
            GameSession gameSession,
            string inventoryNamespaceName,
            string characterInventoryModelName,
            GetItemSetsEvent onGetItemSets,
            ErrorEvent onError
        )
        {
            Characters.Clear();

            yield return FetchItemSets(
                gs2,
                gameSession,
                inventoryNamespaceName,
                characterInventoryModelName,
                Characters,
                onError
            );

            Sort(Characters);

            onGetItemSets.Invoke(characterInventoryModelName, Characters);
        }

        /// <summary>
        /// 強化素材のアイテムセットを取得
        /// Get the item sets of the enhancement materials
        /// </summary>
        public IEnumerator GetMaterials(
            Gs2Domain gs2,
            GameSession gameSession,
            string inventoryNamespaceName,
            string materialInventoryModelName,
            GetItemSetsEvent onGetItemSets,
            ErrorEvent onError
        )
        {
            Materials.Clear();

            yield return FetchItemSets(
                gs2,
                gameSession,
                inventoryNamespaceName,
                materialInventoryModelName,
                Materials,
                onError
            );

            Sort(Materials);

            onGetItemSets.Invoke(materialInventoryModelName, Materials);
        }

        private IEnumerator FetchItemSets(
            Gs2Domain gs2,
            GameSession gameSession,
            string inventoryNamespaceName,
            string inventoryModelName,
            List<EzItemSet> destination,
            ErrorEvent onError
        )
        {
            var it = gs2.Inventory.Namespace(
                namespaceName: inventoryNamespaceName
            ).Me(
                gameSession: gameSession
            ).Inventory(
                inventoryName: inventoryModelName
            ).ItemSets();
            while (it.HasNext())
            {
                yield return it.Next();
                if (it.Error != null)
                {
                    onError.Invoke(it.Error, null);
                    yield break;
                }

                if (it.Current != null)
                {
                    destination.Add(it.Current);
                }
            }
        }
#if GS2_ENABLE_UNITASK
        public async UniTask GetCharactersAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string inventoryNamespaceName,
            string characterInventoryModelName,
            GetItemSetsEvent onGetItemSets,
            ErrorEvent onError
        )
        {
            try
            {
                Characters = await gs2.Inventory.Namespace(
                    namespaceName: inventoryNamespaceName
                ).Me(
                    gameSession: gameSession
                ).Inventory(
                    inventoryName: characterInventoryModelName
                ).ItemSetsAsync().ToListAsync();
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e, null);
                return;
            }

            Sort(Characters);

            onGetItemSets.Invoke(characterInventoryModelName, Characters);
        }

        public async UniTask GetMaterialsAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string inventoryNamespaceName,
            string materialInventoryModelName,
            GetItemSetsEvent onGetItemSets,
            ErrorEvent onError
        )
        {
            try
            {
                Materials = await gs2.Inventory.Namespace(
                    namespaceName: inventoryNamespaceName
                ).Me(
                    gameSession: gameSession
                ).Inventory(
                    inventoryName: materialInventoryModelName
                ).ItemSetsAsync().ToListAsync();
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e, null);
                return;
            }

            Sort(Materials);

            onGetItemSets.Invoke(materialInventoryModelName, Materials);
        }
#endif

        private static void Sort(List<EzItemSet> itemSets)
        {
            itemSets.Sort((o1, o2) => o1.SortValue != o2.SortValue ? o1.SortValue - o2.SortValue : (int)(o2.Count - o1.Count));
        }

        /// <summary>
        /// 強化対象のステータス（ランク・経験値）を取得
        /// Get the statuses (rank and experience) of the enhancement targets
        /// </summary>
        public IEnumerator GetStatuses(
            Gs2Domain gs2,
            GameSession gameSession,
            string experienceNamespaceName,
            string experienceModelName,
            GetEnhanceStatusesEvent onGetStatuses,
            ErrorEvent onError
        )
        {
            var statuses = new List<EzStatus>();

            var it = gs2.Experience.Namespace(
                namespaceName: experienceNamespaceName
            ).Me(
                gameSession: gameSession
            ).Statuses();
            while (it.HasNext())
            {
                yield return it.Next();
                if (it.Error != null)
                {
                    onError.Invoke(it.Error, null);
                    yield break;
                }

                if (it.Current != null && it.Current.ExperienceName == experienceModelName)
                {
                    statuses.Add(it.Current);
                }
            }

            Statuses = statuses;

            onGetStatuses.Invoke(Statuses);
        }
#if GS2_ENABLE_UNITASK
        public async UniTask GetStatusesAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string experienceNamespaceName,
            string experienceModelName,
            GetEnhanceStatusesEvent onGetStatuses,
            ErrorEvent onError
        )
        {
            try
            {
                var statuses = await gs2.Experience.Namespace(
                    namespaceName: experienceNamespaceName
                ).Me(
                    gameSession: gameSession
                ).StatusesAsync().ToListAsync();

                Statuses = statuses
                    .Where(status => status.ExperienceName == experienceModelName)
                    .ToList();
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e, null);
                return;
            }

            onGetStatuses.Invoke(Statuses);
        }
#endif

        /// <summary>
        /// 強化対象に素材を使用してランクを上げる
        /// Enhance the target with materials to raise its rank
        /// </summary>
        public IEnumerator Enhance(
            UnityAction<Gs2Exception> callback,
            Gs2Domain gs2,
            GameSession gameSession,
            string enhanceNamespaceName,
            string enhanceRateName,
            EzItemSet target,
            EzItemSet material,
            int count,
            EnhanceEvent onEnhance,
            ErrorEvent onError
        )
        {
            var domain = gs2.Enhance.Namespace(
                namespaceName: enhanceNamespaceName
            ).Me(
                gameSession: gameSession
            ).Enhance();
            var future = domain.EnhanceFuture(
                rateName: enhanceRateName,
                targetItemSetId: target.ItemSetId,
                materials: new[]
                {
                    new EzMaterial
                    {
                        MaterialItemSetId = material.ItemSetId,
                        Count = count,
                    },
                }
            );
            yield return future;
            if (future.Error != null)
            {
                onError.Invoke(future.Error, null);
                callback.Invoke(future.Error);
                yield break;
            }

            // トランザクションの自動実行の完了を待機（連鎖するトランザクションも含めて全て待つ）
            // Wait for automatic transaction execution to complete (including all chained transactions)
            var waitFuture = future.Result.WaitFuture(true);
            yield return waitFuture;
            if (waitFuture.Error != null)
            {
                onError.Invoke(waitFuture.Error, null);
                callback.Invoke(waitFuture.Error);
                yield break;
            }

            onEnhance.Invoke(target, count);

            callback.Invoke(null);
        }
#if GS2_ENABLE_UNITASK
        public async UniTask<Gs2Exception> EnhanceAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string enhanceNamespaceName,
            string enhanceRateName,
            EzItemSet target,
            EzItemSet material,
            int count,
            EnhanceEvent onEnhance,
            ErrorEvent onError
        )
        {
            var domain = gs2.Enhance.Namespace(
                namespaceName: enhanceNamespaceName
            ).Me(
                gameSession: gameSession
            ).Enhance();
            try
            {
                var result = await domain.EnhanceAsync(
                    rateName: enhanceRateName,
                    targetItemSetId: target.ItemSetId,
                    materials: new[]
                    {
                        new EzMaterial
                        {
                            MaterialItemSetId = material.ItemSetId,
                            Count = count,
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
                // トランザクション結果の取得失敗（TimeoutException など）も失敗として扱う
                // Treat a failure to obtain the transaction result (TimeoutException, etc.) as a failure
                var error = new UnknownException(e.Message);
                onError.Invoke(error, null);
                return error;
            }

            onEnhance.Invoke(target, count);

            return null;
        }
#endif

        /// <summary>
        /// 動作確認用に強化対象／強化素材を入手する
        /// Obtain an enhancement target / materials for verification purposes
        ///
        /// ※実際のゲームでは、クライアントから直接アイテムを増やす実装は非推奨です。
        /// *In an actual game, an implementation in which the client directly increases items is deprecated.
        /// </summary>
        public IEnumerator AcquireItem(
            UnityAction<Gs2Exception> callback,
            Gs2Domain gs2,
            GameSession gameSession,
            string exchangeNamespaceName,
            string exchangeRateName,
            AcquireItemEvent onAcquireItem,
            ErrorEvent onError
        )
        {
            var domain = gs2.Exchange.Namespace(
                namespaceName: exchangeNamespaceName
            ).Me(
                gameSession: gameSession
            ).Exchange();
            var future = domain.ExchangeFuture(
                rateName: exchangeRateName,
                count: 1,
                config: new EzConfig[] { }
            );
            yield return future;
            if (future.Error != null)
            {
                onError.Invoke(future.Error, null);
                callback.Invoke(future.Error);
                yield break;
            }

            // トランザクションの自動実行の完了を待機（連鎖するトランザクションも含めて全て待つ）
            // Wait for automatic transaction execution to complete (including all chained transactions)
            var waitFuture = future.Result.WaitFuture(true);
            yield return waitFuture;
            if (waitFuture.Error != null)
            {
                onError.Invoke(waitFuture.Error, null);
                callback.Invoke(waitFuture.Error);
                yield break;
            }

            onAcquireItem.Invoke(exchangeRateName);

            callback.Invoke(null);
        }
#if GS2_ENABLE_UNITASK
        public async UniTask<Gs2Exception> AcquireItemAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string exchangeNamespaceName,
            string exchangeRateName,
            AcquireItemEvent onAcquireItem,
            ErrorEvent onError
        )
        {
            var domain = gs2.Exchange.Namespace(
                namespaceName: exchangeNamespaceName
            ).Me(
                gameSession: gameSession
            ).Exchange();
            try
            {
                var result = await domain.ExchangeAsync(
                    rateName: exchangeRateName,
                    count: 1,
                    config: new EzConfig[] { }
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
                // トランザクション結果の取得失敗（TimeoutException など）も失敗として扱う
                // Treat a failure to obtain the transaction result (TimeoutException, etc.) as a failure
                var error = new UnknownException(e.Message);
                onError.Invoke(error, null);
                return error;
            }

            onAcquireItem.Invoke(exchangeRateName);

            return null;
        }
#endif
    }
}
