using System.Collections;
using System.Collections.Generic;
using Gs2.Core.Exception;
using Gs2.Unity.Core;
using Gs2.Unity.Gs2Matchmaking.Model;
using Gs2.Unity.Util;
using UnityEngine;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
#endif

namespace Gs2.Sample.Matchmaking
{
    public class MatchmakingModel : MonoBehaviour
    {
        /// <summary>
        /// 自分を含む募集人数
        /// Number of applicants including player
        /// </summary>
        public int Capacity;
        
        /// <summary>
        /// 参加しているギャザリング
        /// Participating Gatherings
        /// </summary>
        public EzGathering Gathering;
        
        /// <summary>
        /// ギャザリングに参加しているプレイヤーIDリスト
        /// </summary>
        public List<string> JoinedPlayerIds = new List<string>();
        
        public List<EzGathering> ResultGatherings = new List<EzGathering>();
        
        public Gs2Exception Error;
        
        /// <summary>
        /// だれでも参加可能、参加人数を指定でギャザリングを新規作成
        /// </summary>
        /// <returns></returns>
        public IEnumerator CreateGathering(
            Gs2Domain gs2,
            GameSession gameSession,
            string matchmakingNamespaceName,
            UpdateJoinedPlayerIdsEvent onUpdateJoinedPlayerIds,
            ErrorEvent onError
            )
        {
            Error = null;
            var domain = gs2.Matchmaking.Namespace(
                namespaceName: matchmakingNamespaceName
            ).Me(
                gameSession: gameSession
            );
            var future = domain.CreateGathering(
                player: new EzPlayer
                {
                    RoleName = "default"
                },
                attributeRanges: null,
                capacityOfRoles: new [] {
                    new EzCapacityOfRole
                    {
                        RoleName = "default",
                        Capacity = Capacity
                    }
                },
                allowUserIds: null,
                expiresAt: null,
                expiresAtTimeSpan: null
            );
            yield return future;
            if (future.Error != null)
            {
                Error = future.Error;
                onError.Invoke(
                    future.Error,
                    null
                );
                yield break;
            }

            var future2 = future.Result.Model();
            yield return future2;
            if (future2.Error != null)
            {
                Error = future.Error;
                onError.Invoke(
                    future2.Error,
                    null
                );
                yield break;
            }
            
            JoinedPlayerIds.Clear();
            Gathering = future2.Result;
            JoinedPlayerIds.Add(gameSession.AccessToken.UserId);

            onUpdateJoinedPlayerIds.Invoke(Gathering, JoinedPlayerIds);
        }
#if GS2_ENABLE_UNITASK
        public async UniTask CreateGatheringAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string matchmakingNamespaceName,
            UpdateJoinedPlayerIdsEvent onUpdateJoinedPlayerIds,
            ErrorEvent onError
        )
        {
            Error = null;
            var domain = gs2.Matchmaking.Namespace(
                namespaceName: matchmakingNamespaceName
            ).Me(
                gameSession: gameSession
            );
            try
            {
                var result = await domain.CreateGatheringAsync(
                    player: new EzPlayer
                    {
                        RoleName = "default"
                    },
                    attributeRanges: null,
                    capacityOfRoles: new[]
                    {
                        new EzCapacityOfRole
                        {
                            RoleName = "default",
                            Capacity = Capacity
                        }
                    },
                    allowUserIds: null,
                    expiresAt: null,
                    expiresAtTimeSpan: null
                );
                Gathering = await result.ModelAsync();

                JoinedPlayerIds.Clear();
                JoinedPlayerIds.Add(gameSession.AccessToken.UserId);

                onUpdateJoinedPlayerIds.Invoke(Gathering, JoinedPlayerIds);
            }
            catch (Gs2Exception e)
            {
                Error = e;
                onError.Invoke(e, null);
            }
        }
#endif
        
        /// <summary>
        /// ギャザリングを検索する
        /// </summary>
        public IEnumerator JoinGathering(
            Gs2Domain gs2,
            GameSession gameSession,
            string matchmakingNamespaceName,
            ErrorEvent onError
        )
        {
            ResultGatherings.Clear();
            Error = null;
            var domain = gs2.Matchmaking.Namespace(
                namespaceName: matchmakingNamespaceName
            ).Me(
                gameSession: gameSession
            );
            var it = domain.DoMatchmaking(
                new EzPlayer
                {
                    RoleName = "default"
                }
            );
            while (it.HasNext())
            {
                yield return it.Next();
                if (it.Error != null)
                {
                    Error = it.Error;
                    onError.Invoke(it.Error, null);
                    break;
                }

                if (it.Current != null)
                {
                    ResultGatherings.Add(it.Current);
                }
            }
                
            JoinedPlayerIds.Clear();
        }
#if GS2_ENABLE_UNITASK
        public async UniTask JoinGatheringAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string matchmakingNamespaceName,
            ErrorEvent onError
        )
        {
            ResultGatherings.Clear();
            Error = null;
            Gathering = null;
            var domain = gs2.Matchmaking.Namespace(
                namespaceName: matchmakingNamespaceName
            ).Me(
                gameSession: gameSession
            );
            try
            {
                ResultGatherings = await domain.DoMatchmakingAsync(
                    new EzPlayer
                    {
                        RoleName = "default"
                    }
                ).ToListAsync();
                JoinedPlayerIds.Clear();
            }
            catch (Gs2Exception e)
            {
                Error = e;
                onError.Invoke(e, null);
            }
        }
#endif
        
        /// <summary>
        /// ギャザリングから離脱する
        /// </summary>
        /// <returns></returns>
        public IEnumerator CancelMatchmaking(
            Gs2Domain gs2,
            GameSession gameSession,
            string matchmakingNamespaceName,
            MatchmakingCancelEvent onMatchmakingCancel,
            ErrorEvent onError
            )
        {
            Error = null;
            var domain = gs2.Matchmaking.Namespace(
                namespaceName: matchmakingNamespaceName
            ).Me(
                gameSession: gameSession
            ).Gathering(
                gatheringName: Gathering.Name
            );
            var future = domain.CancelMatchmaking();
            yield return future;
            if (future.Error != null)
            {
                onError.Invoke(future.Error, null);
                yield break;
            }
 
            var domain2 = future.Result;
            var future2 = domain2.Model();
            yield return future2;
            if (future.Error != null)
            {
                Error = future.Error;
                onError.Invoke(future.Error, null);
                yield break;
            }
            
            onMatchmakingCancel.Invoke(future2.Result);
            
            Gathering = null;
            JoinedPlayerIds.Clear();
        }
#if GS2_ENABLE_UNITASK
        /// <summary>
        /// ギャザリングから離脱する
        /// </summary>
        public async UniTask CancelMatchmakingAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string matchmakingNamespaceName,
            MatchmakingCancelEvent onMatchmakingCancel,
            ErrorEvent onError
        )
        {
            Error = null;
            var domain = gs2.Matchmaking.Namespace(
                namespaceName: matchmakingNamespaceName
            ).Me(
                gameSession: gameSession
            ).Gathering(
                gatheringName: Gathering.Name
            );
            try
            {
                var result = await domain.CancelMatchmakingAsync();
                Gathering = await result.ModelAsync();
                
                onMatchmakingCancel.Invoke(Gathering);
                Gathering = null;
                JoinedPlayerIds.Clear();
            }
            catch (Gs2Exception e)
            {
                Error = e;
                onError.Invoke(e, null);
            }
        }
#endif
    }
}