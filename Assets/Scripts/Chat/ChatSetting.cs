using System.Collections.Generic;
using Gs2.Unity.Gs2Chat.Model;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Chat
{
    [System.Serializable]
    public class GetRoomEvent : UnityEvent<EzRoom>
    {
    }
    
    [System.Serializable]
    public class ListSubscribeRoomsEvent : UnityEvent<List<EzSubscribe>>
    {
    }
    
    [System.Serializable]
    public class CreateRoomEvent : UnityEvent<EzRoom>
    {
    }
    
    [System.Serializable]
    public class SubscribeEvent : UnityEvent<EzSubscribe>
    {
    }
    
    [System.Serializable]
    public class UnsubscribeEvent : UnityEvent
    {
    }

    [System.Serializable]
    public class ListMessagesEvent : UnityEvent<List<EzMessage>>
    {
    }
    
    [System.Serializable]
    public class PostEvent : UnityEvent<EzMessage>
    {
    }
    public class ChatSetting : MonoBehaviour
    {
        /// <summary>
        /// GS2-Chat のネームスペース名
        /// </summary>
        public string chatNamespaceName;

        /// <summary>
        /// ルーム名
        /// </summary>
        public string roomName;
        
        public GetRoomEvent onGetRoom = new GetRoomEvent();

        public ListSubscribeRoomsEvent onListSubscribeRooms = new ListSubscribeRoomsEvent();
        
        public CreateRoomEvent onCreateRoom = new CreateRoomEvent();
        
        public SubscribeEvent onSubscribe = new SubscribeEvent();
        
        public UnsubscribeEvent onUnsubscribe = new UnsubscribeEvent();
        
        public ListMessagesEvent onListMessages = new ListMessagesEvent();
        
        public PostEvent onPost = new PostEvent();
        
        public ErrorEvent onError = new ErrorEvent();
    }
}