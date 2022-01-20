using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gs2.Core;
using Gs2.Core.Model;
using Gs2.Core.Net;
using Gs2.Gs2Experience;
using Gs2.Gs2Experience.Request;
using Gs2.Unity;
using Gs2.Unity.Gs2Experience.Model;
using Gs2.Unity.Gs2Experience.Result;
using Gs2.Unity.Gs2Inventory.Model;
using Gs2.Unity.Util;
using UnityEngine;

namespace Gs2.Sample.Experience
{
    public class ExperienceModel : MonoBehaviour
    {
        /// <summary>
        /// プレイヤー経験値モデル
        /// </summary>
        public EzExperienceModel playerExperienceModel;
        
        /// <summary>
        /// 現在のステータス
        /// </summary>
        public Dictionary<string, EzStatus> playerStatuses;
        
        /// <summary>
        /// アイテム経験値モデル
        /// </summary>
        public EzExperienceModel itemExperienceModel;
        
        /// <summary>
        /// 現在のステータス
        /// </summary>
        public Dictionary<string, EzStatus> itemStatuses;

        /// <summary>
        /// 操作中のアイテムセット
        /// </summary>
        public EzItemSet selectedItem;
        
        /// <summary>
        /// プレイヤー経験値モデルの取得
        /// </summary>
        /// <param name="client"></param>
        /// <param name="experienceNamespaceName"></param>
        /// <param name="experienceModelName"></param>
        /// <param name="onGetExperienceModel"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator GetPlayerExperienceModel(
            Client client,
            string experienceNamespaceName,
            string experienceModelName,
            GetExperienceModelEvent onGetExperienceModel,
            ErrorEvent onError
        )
        {
            AsyncResult<EzGetExperienceModelResult> result = null;
            yield return client.Experience.GetExperienceModel(
                r =>
                {
                    result = r;
                },
                experienceNamespaceName,
                experienceModelName
            );
            
            if (result.Error != null)
            {
                onError.Invoke(
                    result.Error
                );
                yield break;
            }

            playerExperienceModel = result.Result.Item;

            onGetExperienceModel.Invoke(experienceModelName, playerExperienceModel);
        }
        
        /// <summary>
        /// アイテム経験値モデルの取得
        /// </summary>
        /// <param name="client"></param>
        /// <param name="experienceNamespaceName"></param>
        /// <param name="experienceModelName"></param>
        /// <param name="onGetExperienceModel"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator GetItemExperienceModel(
            Client client,
            string experienceNamespaceName,
            string experienceModelName,
            GetExperienceModelEvent onGetExperienceModel,
            ErrorEvent onError
        )
        {
            AsyncResult<EzGetExperienceModelResult> result = null;
            yield return client.Experience.GetExperienceModel(
                r =>
                {
                    result = r;
                },
                experienceNamespaceName,
                experienceModelName
            );
            
            if (result.Error != null)
            {
                onError.Invoke(
                    result.Error
                );
                yield break;
            }

            itemExperienceModel = result.Result.Item;

            onGetExperienceModel.Invoke(experienceModelName, itemExperienceModel);
        }
        
        /// <summary>
        /// プレイヤーのステータス情報の一覧を取得
        /// </summary>
        /// <param name="client"></param>
        /// <param name="session"></param>
        /// <param name="experienceNamespaceName"></param>
        /// <param name="onGetStatuses"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator GetPlayerStatuses(
            Client client,
            GameSession session,
            string experienceNamespaceName,
            GetStatusesEvent onGetStatuses,
            ErrorEvent onError
        )
        {
            var _statuses = new List<EzStatus>();
            string pageToken = null;
            while (true)
            {
                AsyncResult<EzListStatusesResult> result = null;
                yield return client.Experience.ListStatuses(
                    r =>
                    {
                        result = r;
                    },
                    session,
                    experienceNamespaceName,
                    playerExperienceModel.Name,
                    pageToken
                );
            
                if (result.Error != null)
                {
                    onError.Invoke(
                        result.Error
                    );
                    yield break;
                }

                _statuses.AddRange(result.Result.Items);

                if (result.Result.NextPageToken == null)
                {
                    break;
                }

                pageToken = result.Result.NextPageToken;
            }

            playerStatuses = _statuses.ToDictionary(status => status.PropertyId);
            
            onGetStatuses.Invoke(playerExperienceModel, _statuses);
        }
        
        /// <summary>
        /// アイテムのステータス情報の一覧を取得
        /// </summary>
        /// <param name="client"></param>
        /// <param name="session"></param>
        /// <param name="experienceNamespaceName"></param>
        /// <param name="onGetStatuses"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator GetItemStatuses(
            Client client,
            GameSession session,
            string experienceNamespaceName,
            GetStatusesEvent onGetStatuses,
            ErrorEvent onError
        )
        {
            var _statuses = new List<EzStatus>();
            string pageToken = null;
            while (true)
            {
                AsyncResult<EzListStatusesResult> result = null;
                yield return client.Experience.ListStatuses(
                    r =>
                    {
                        result = r;
                    },
                    session,
                    experienceNamespaceName,
                    itemExperienceModel.Name,
                    pageToken
                );
            
                if (result.Error != null)
                {
                    onError.Invoke(
                        result.Error
                    );
                    yield break;
                }

                _statuses.AddRange(result.Result.Items);

                if (result.Result.NextPageToken == null)
                {
                    break;
                }

                pageToken = result.Result.NextPageToken;
            }

            itemStatuses = _statuses.ToDictionary(status => status.PropertyId);
            
            onGetStatuses.Invoke(itemExperienceModel, _statuses);
        }

        /// <summary>
        /// 経験値の増加
        /// </summary>
        /// <param name="session"></param>
        /// <param name="identifierIncreaseExperienceClientId"></param>
        /// <param name="identifierIncreaseExperienceClientSecret"></param>
        /// <param name="experienceNamespaceName"></param>
        /// <param name="experienceModel"></param>
        /// <param name="propertyId"></param>
        /// <param name="value"></param>
        /// <param name="onIncreaseExperience"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator IncreaseExperience(
            GameSession session,
            string identifierIncreaseExperienceClientId,
            string identifierIncreaseExperienceClientSecret,
            string experienceNamespaceName,
            EzExperienceModel experienceModel,
            string propertyId,
            int value,
            IncreaseExperienceEvent onIncreaseExperience,
            ErrorEvent onError
        )
        {
            // ※この処理はサンプルの動作確認のためものです。
            // 実際にクライアントが直接経験値の増加を行う実装は非推奨となります。
            
            var restSession = new Gs2RestSession(
                new BasicGs2Credential(
                    identifierIncreaseExperienceClientId,
                    identifierIncreaseExperienceClientSecret
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

            var restClient = new Gs2ExperienceRestClient(
                restSession
            );

            yield return restClient.AddExperienceByUserId(
                new AddExperienceByUserIdRequest()
                    .WithNamespaceName(experienceNamespaceName)
                    .WithUserId(session.AccessToken.UserId)
                    .WithExperienceName(experienceModel.Name)
                    .WithPropertyId(propertyId)
                    .WithExperienceValue(value),
                r =>
                {
                    if (r.Error != null)
                    {
                        onError.Invoke(r.Error);
                        error = true;
                    }
                    else
                    {
                        onIncreaseExperience.Invoke(
                            experienceModel,
                            EzStatus.FromModel(r.Result.Item),
                            value
                        );
                    }
                }
            );
            
            yield return restSession.Close(() => { });
        }
    }
}