using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gs2.Core.Exception;
using Gs2.Unity.Core;
using Gs2.Unity.Gs2Exchange.Model;
using Gs2.Unity.Gs2Experience.Model;
using Gs2.Unity.Gs2Inventory.Model;
using Gs2.Unity.Util;
using UnityEngine;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
#endif

namespace Gs2.Sample.Experience
{
    public class ExperienceModel : MonoBehaviour
    {
        /// <summary>
        /// プレイヤー経験値モデル
        /// Player Experience Model
        /// </summary>
        public EzExperienceModel playerExperienceModel;
        
        /// <summary>
        /// プレイヤーの現在のステータス
        /// Current Status
        /// </summary>
        public Dictionary<string, EzStatus> playerStatuses;
        
        /// <summary>
        /// プレイヤーの現在のステータス
        /// Current Status
        /// </summary>
        public EzStatus playerStatus;
        
        /// <summary>
        /// アイテム経験値モデル
        ///  Item Experience Model
        /// </summary>
        public EzExperienceModel itemExperienceModel;
        
        /// <summary>
        /// アイテムの現在のステータス
        /// Current Status
        /// </summary>
        public Dictionary<string, EzStatus> itemStatuses;

        /// <summary>
        /// 選択中のアイテムセット
        /// Selected Item Sets
        /// </summary>
        public EzItemSet selectedItem;
        
        /// <summary>
        /// プレイヤー経験値モデルの取得
        /// Acquisition of player experience model
        /// </summary>
        /// <param name="gs2"></param>
        /// <param name="experienceNamespaceName"></param>
        /// <param name="experienceModelName"></param>
        /// <param name="onGetExperienceModel"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator GetPlayerExperienceModel(
            Gs2Domain gs2,
            string experienceNamespaceName,
            string experienceModelName,
            GetExperienceModelEvent onGetExperienceModel,
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
            playerExperienceModel = future.Result;

            onGetExperienceModel.Invoke(experienceModelName, playerExperienceModel);
        }
        
#if GS2_ENABLE_UNITASK
        public async UniTask GetPlayerExperienceModelAsync(
            Gs2Domain gs2,
            string experienceNamespaceName,
            string experienceModelName,
            GetExperienceModelEvent onGetExperienceModel,
            ErrorEvent onError
        )
        {
            var domain = gs2.Experience.Namespace(
                namespaceName: experienceNamespaceName
            ).ExperienceModel(
                experienceName: experienceModelName
            );
            
            try
            {
                playerExperienceModel = await domain.ModelAsync();
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e, null);
                return;
            }

            onGetExperienceModel.Invoke(experienceModelName, playerExperienceModel);
        }
#endif
        
        /// <summary>
        /// アイテム経験値モデルの取得
        /// Acquisition of item experience model
        /// </summary>
        /// <param name="gs2"></param>
        /// <param name="experienceNamespaceName"></param>
        /// <param name="experienceModelName"></param>
        /// <param name="onGetExperienceModel"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator GetItemExperienceModel(
            Gs2Domain gs2,
            string experienceNamespaceName,
            string experienceModelName,
            GetExperienceModelEvent onGetExperienceModel,
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
            itemExperienceModel = future.Result;

            onGetExperienceModel.Invoke(experienceModelName, itemExperienceModel);
        }
        
#if GS2_ENABLE_UNITASK
        public async UniTask GetItemExperienceModelAsync(
            Gs2Domain gs2,
            string experienceNamespaceName,
            string experienceModelName,
            GetExperienceModelEvent onGetExperienceModel,
            ErrorEvent onError
        )
        {
            var domain = gs2.Experience.Namespace(
                namespaceName: experienceNamespaceName
            ).ExperienceModel(
                experienceName: experienceModelName
            );
            try
            {
                itemExperienceModel = await domain.ModelAsync();
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e, null);
                return;
            }

            onGetExperienceModel.Invoke(experienceModelName, itemExperienceModel);
        }
#endif
        
        /// <summary>
        /// プレイヤーのステータス情報の一覧を取得
        /// Get a list of player status information
        /// </summary>
        /// <param name="gs2"></param>
        /// <param name="gameSession"></param>
        /// <param name="experienceNamespaceName"></param>
        /// <param name="onGetStatuses"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator GetPlayerStatuses(
            Gs2Domain gs2,
            GameSession gameSession,
            string experienceNamespaceName,
            GetStatusesEvent onGetStatuses,
            ErrorEvent onError
        )
        {
            var _statuses = new List<EzStatus>();
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
                    break;
                }

                if (it.Current != null)
                {
                    _statuses.Add(it.Current);
                }
            }

            playerStatuses = _statuses.ToDictionary(status => status.PropertyId);
            
            onGetStatuses.Invoke(playerExperienceModel, _statuses);
        }
#if GS2_ENABLE_UNITASK
        public async UniTask GetPlayerStatusesAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string experienceNamespaceName,
            GetStatusesEvent onGetStatuses,
            ErrorEvent onError
        )
        {
            try
            {
                var _statuses = await gs2.Experience.Namespace(
                    namespaceName: experienceNamespaceName
                ).Me(
                    gameSession: gameSession
                ).StatusesAsync().ToListAsync();

                playerStatuses = _statuses.ToDictionary(status => status.PropertyId);

                onGetStatuses.Invoke(playerExperienceModel, _statuses);
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e, null);
            }
        }
#endif

        /// <summary>
        /// アイテムのステータス情報の一覧を取得
        /// Get a list of item status information
        /// </summary>
        /// <param name="gs2"></param>
        /// <param name="gameSession"></param>
        /// <param name="experienceNamespaceName"></param>
        /// <param name="onGetStatuses"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator GetItemStatuses(
            Gs2Domain gs2,
            GameSession gameSession,
            string experienceNamespaceName,
            GetStatusesEvent onGetStatuses,
            ErrorEvent onError
        )
        {
            var _statuses = new List<EzStatus>();
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
                    break;
                }

                if (it.Current != null)
                {
                    _statuses.Add(it.Current);
                }
            }

            itemStatuses = _statuses.ToDictionary(status => status.PropertyId);
            
            onGetStatuses.Invoke(itemExperienceModel, _statuses);
        }
#if GS2_ENABLE_UNITASK
        public async UniTask GetItemStatusesAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string experienceNamespaceName,
            GetStatusesEvent onGetStatuses,
            ErrorEvent onError
        )
        {
            try
            {
                var _statuses = await gs2.Experience.Namespace(
                    namespaceName: experienceNamespaceName
                ).Me(
                    gameSession: gameSession
                ).StatusesAsync().ToListAsync();
                
                itemStatuses = _statuses.ToDictionary(status => status.PropertyId);
                            
                onGetStatuses.Invoke(itemExperienceModel, _statuses);
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e, null);
            }
        }
#endif

        /// <summary>
        /// 経験値の増加
        /// Increased experience
        /// </summary>
        public IEnumerator IncreaseExperience(
            Gs2Domain gs2,
            GameSession gameSession,
            string exchangeNamespaceName,
            string exchangeRateName,
            string propertyId,
            int value,
            ErrorEvent onError
        )
        {
            // ※この処理はサンプルの動作確認のためものです。
            // 実際にクライアントが直接経験値の増加を行う実装は非推奨となります。
            // *This process is only for sample confirmation.
            // The actual implementation of direct experience increase by the client is deprecated.

            var domain = gs2.Exchange.Namespace(
                namespaceName: exchangeNamespaceName
            ).Me(
                gameSession: gameSession
            ).Exchange();
            var future = domain.ExchangeFuture(
                rateName: exchangeRateName,
                count: value,
                config: new[]
                {
                    new EzConfig
                    {
                        Key = "propertyId",
                        Value = propertyId
                    }
                }
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

            var item = future.Result;

        }
#if GS2_ENABLE_UNITASK
        public async UniTask IncreaseExperienceAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string exchangeNamespaceName,
            string exchangeRateName,
            string propertyId,
            int value,
            ErrorEvent onError
        )
        {
            // ※この処理はサンプルの動作確認のためものです。
            // 実際にクライアントが直接経験値の増加を行う実装は非推奨となります。
            // *This process is only for sample confirmation.
            // The actual implementation of direct experience increase by the client is deprecated.

            var domain = gs2.Exchange.Namespace(
                namespaceName: exchangeNamespaceName
            ).Me(
                gameSession: gameSession
            ).Exchange();
            try
            {
                var TransactionDomain = await domain.ExchangeAsync(
                    rateName: exchangeRateName,
                    count: value,
                    config: new[]
                    {
                        new EzConfig
                        {
                            Key = "propertyId",
                            Value = propertyId
                        }
                    }
                );

                var result = await TransactionDomain.WaitAsync();
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e, null);
            }
        }
#endif
    }
}