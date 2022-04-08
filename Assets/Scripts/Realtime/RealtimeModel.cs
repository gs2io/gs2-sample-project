using System.Collections;
using Gs2.Core;
using Gs2.Unity.Gs2Realtime;
using Gs2.Unity.Gs2Realtime.Model;
using Gs2.Unity.Gs2Realtime.Result;
using UnityEngine;
using UnityEngine.Events;

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
        /// <param name="callback"></param>
        /// <param name="client"></param>
        /// <param name="realtimeNamespaceName"></param>
        /// <param name="onGetRoom"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator GetRoom(
            UnityAction<AsyncResult<EzGetRoomResult>> callback,
            Gs2.Unity.Client client,
            string realtimeNamespaceName,
            GetRoomEvent onGetRoom,
            Gs2.Unity.Util.ErrorEvent onError
        )
        {
            AsyncResult<EzGetRoomResult> result = null;
            yield return client.Realtime.GetRoom(
                r => { result = r; },
                realtimeNamespaceName,
                gatheringName
            );
            
            if (result.Error != null)
            {
                onError.Invoke(
                    result.Error
                );
                callback.Invoke(result);
                yield break;
            }

            room = result.Result.Item;
            
            onGetRoom.Invoke(result.Result.Item);

            callback.Invoke(result);
        }
    }
}