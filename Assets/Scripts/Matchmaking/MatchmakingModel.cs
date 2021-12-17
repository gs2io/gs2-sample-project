using System.Collections;
using System.Collections.Generic;
using Gs2.Core;
using Gs2.Unity;
using Gs2.Unity.Gs2Matchmaking.Model;
using Gs2.Unity.Gs2Matchmaking.Result;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Matchmaking
{
    public class MatchmakingModel : MonoBehaviour
    {
        /// <summary>
        /// 自分を含む募集人数
        /// </summary>
        public int Capacity;
        
        /// <summary>
        /// 参加しているギャザリング
        /// </summary>
        public EzGathering Gathering;
        
        /// <summary>
        /// ギャザリングに参加しているプレイヤーIDリスト
        /// </summary>
        public List<string> JoinedPlayerIds = new List<string>();
        
        /// <summary>
        /// だれでも参加可能、参加人数を指定でギャザリングを新規作成
        /// </summary>
        /// <returns></returns>
        public IEnumerator CreateGathering(
            Client client,
            GameSession session,
            UnityAction<AsyncResult<EzCreateGatheringResult>> callback,
            string matchmakingNamespaceName,
            UpdateJoinedPlayerIdsEvent onUpdateJoinedPlayerIds,
            ErrorEvent onError
            )
        {
            AsyncResult<EzCreateGatheringResult> result = null;
            yield return client.Matchmaking.CreateGathering(
                r => { result = r; },
                session,
                matchmakingNamespaceName,
                new EzPlayer
                {
                    RoleName = "default"
                },
                new List<EzAttributeRange>(),
                new List<EzCapacityOfRole>
                {
                    new EzCapacityOfRole
                    {
                        RoleName = "default",
                        Capacity = Capacity
                    },
                },
                new List<string>(),
                null
            );
            
            if (result.Error != null)
            {
                onError.Invoke(
                    result.Error
                );
                callback.Invoke(result);
                yield break;
            }

            JoinedPlayerIds.Clear();
            Gathering = result.Result.Item;
            JoinedPlayerIds.Add(session.AccessToken.UserId);

            onUpdateJoinedPlayerIds.Invoke(Gathering, JoinedPlayerIds);
            
            callback.Invoke(result);
        }

        /// <summary>
        /// 既存のギャザリングに参加する
        /// </summary>
        /// <returns></returns>
        public IEnumerator JoinGathering(
            UnityAction<AsyncResult<EzDoMatchmakingResult>> callback,
            Client client,
            GameSession session,
            string matchmakingNamespaceName,
            string contextToken,
            ErrorEvent onError
        )
        {
            AsyncResult<EzDoMatchmakingResult> result = null;

            yield return client.Matchmaking.DoMatchmaking(
                r => { result = r; },
                session,
                matchmakingNamespaceName,
                new EzPlayer
                {
                    RoleName = "default"
                },
                contextToken
            );
                
            if (result.Error != null)
            {
                onError.Invoke(
                    result.Error
                );
                callback.Invoke(result);
                yield break;
            }
            
            callback.Invoke(result);
        }
        
        /// <summary>
        /// ギャザリングから離脱する
        /// </summary>
        /// <returns></returns>
        public IEnumerator CancelMatchmaking(
            UnityAction<AsyncResult<EzCancelMatchmakingResult>> callback,
            Client client,
            GameSession session,
            string matchmakingNamespaceName,
            MatchmakingCancelEvent onMatchmakingCancel,
            ErrorEvent onError
            )
        {
            AsyncResult<EzCancelMatchmakingResult> result = null;
            yield return client.Matchmaking.CancelMatchmaking(
                r => { result = r; },
                session,
                matchmakingNamespaceName,
                Gathering.Name
            );
        
            if (result.Error != null)
            {
                onError.Invoke(
                    result.Error
                );
                callback.Invoke(result);
                yield break;
            }

            onMatchmakingCancel.Invoke(
                Gathering
            );

            Gathering = null;
            
            callback.Invoke(result);
        }
    }
}