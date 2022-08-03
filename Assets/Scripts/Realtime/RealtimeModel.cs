using System.Collections;
using Gs2.Core.Exception;
using Gs2.Unity.Core;
using Gs2.Unity.Gs2Realtime;
using Gs2.Unity.Gs2Realtime.Model;
using UnityEngine;
using UnityEngine.Events;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
#endif

namespace Gs2.Sample.Realtime
{
    public class RealtimeModel : MonoBehaviour
    {
        /// <summary>
        /// ギャザリングで作成されるゲームサーバ固有のID
        /// Unique game server ID created by the Gathering
        /// </summary>
        public string gatheringName;

        public EzRoom room;
        
        public RelayRealtimeSession realtimeSession;

        public void Clear()
        {
            gatheringName = string.Empty;
            room = null;
            realtimeSession = null;
        }
        
        /// <summary>
        /// GS2-Matchmaking のギャザリング情報から GS2-Realtime のルーム情報を取得
        /// Get GS2-Realtime room information from GS2-Matchmaking gathering information
        /// </summary>
        public IEnumerator GetRoom(
            UnityAction<EzRoom> callback,
            Gs2Domain gs2,
            string realtimeNamespaceName,
            GetRoomEvent onGetRoom,
            Gs2.Unity.Util.ErrorEvent onError
        )
        {
            var domain = gs2.Realtime.Namespace(
                namespaceName: realtimeNamespaceName
            ).Room(
                roomName: gatheringName
            );
            var future = domain.Model();
            yield return future;
            if (future.Error != null)
            {
                onError.Invoke(
                    future.Error
                );
                callback.Invoke(null);
                yield break;
            }

            room = future.Result;
            
            onGetRoom.Invoke(room);

            callback.Invoke(room);
        }
#if GS2_ENABLE_UNITASK
        public async UniTask<EzRoom> GetRoomAsync(
            Gs2Domain gs2,
            string realtimeNamespaceName,
            GetRoomEvent onGetRoom,
            Gs2.Unity.Util.ErrorEvent onError
        )
        {
            var domain = gs2.Realtime.Namespace(
                namespaceName: realtimeNamespaceName
            ).Room(
                roomName: gatheringName
            );
            try
            {
                room = await domain.ModelAsync();
                
                onGetRoom.Invoke(room);
                
                return room;
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e);
                throw;
            }
        }
#endif
    }
}