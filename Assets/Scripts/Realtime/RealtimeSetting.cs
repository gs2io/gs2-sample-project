using Gs2.Gs2Realtime.Message;
using Gs2.Unity.Gs2Realtime.Model;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Events;
using Gs2.Util.WebSocketSharp;

namespace Gs2.Sample.Realtime
{
    [System.Serializable]
    public class RelayMessageEvent : UnityEvent<RelayBinaryMessage>
    {
    }
    
    [System.Serializable]
    public class GetRoomEvent : UnityEvent<EzRoom>
    {
    }

    [System.Serializable]
    public class JoinPlayerEvent : UnityEvent<Player>
    {
    }

    [System.Serializable]
    public class LeavePlayerEvent : UnityEvent<Player>
    {
    }
        
    [System.Serializable]
    public class UpdateProfileEvent : UnityEvent<Player>
    {
    }

    [System.Serializable]
    public class RelayErrorEvent : UnityEvent<Error>
    {
    }

    [System.Serializable]
    public class GeneralErrorEvent : UnityEvent<ErrorEventArgs>
    {
    }
        
    [System.Serializable]
    public class CloseEvent : UnityEvent<CloseEventArgs>
    {
    }
    
    public class RealtimeSetting : MonoBehaviour
    {
        /// <summary>
        /// GS2-Realtime のネームスペース名
        /// </summary>
        public string realtimeNamespaceName;

        public RelayMessageEvent onRelayMessage = new RelayMessageEvent();
        
        public GetRoomEvent onGetRoom = new GetRoomEvent();

        public JoinPlayerEvent onJoinPlayer = new JoinPlayerEvent();

        public LeavePlayerEvent onLeavePlayer = new LeavePlayerEvent();

        public UpdateProfileEvent onUpdateProfile = new UpdateProfileEvent();
        
        public CloseEvent onClose = new CloseEvent();
        
        public RelayErrorEvent onRelayError = new RelayErrorEvent();
        
        public GeneralErrorEvent onGeneralError = new GeneralErrorEvent();
        
        public ErrorEvent onError = new ErrorEvent();
    }
}