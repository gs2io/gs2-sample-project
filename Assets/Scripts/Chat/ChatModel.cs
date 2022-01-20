using System.Collections;
using System.Collections.Generic;
using Gs2.Core;
using Gs2.Unity;
using Gs2.Unity.Gs2Chat.Model;
using Gs2.Unity.Gs2Chat.Result;
using Gs2.Unity.Util;
using UnityEngine;

namespace Gs2.Sample.Chat
{
    public class ChatModel : MonoBehaviour
    {
        public EzRoom room;

        public List<EzSubscribe> rooms;
        
        /// <summary>
        /// ルーム情報を取得
        /// </summary>
        /// <param name="client"></param>
        /// <param name="chatNamespaceName"></param>
        /// <param name="roomName"></param>
        /// <param name="onGetRoom"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator GetRoom(
            Client client,
            string chatNamespaceName,
            string roomName,
            GetRoomEvent onGetRoom,
            ErrorEvent onError
        )
        {
            AsyncResult<EzGetRoomResult> result = null;
            yield return client.Chat.GetRoom(
                r => { result = r; },
                chatNamespaceName,
                roomName
            );

            if (result.Error != null)
            {
                onError.Invoke(result.Error);
                yield break;
            }

            room = result.Result.Item;
            
            onGetRoom.Invoke(room);
        }
        
        /// <summary>
        /// 購読しているルームの一覧を取得
        /// </summary>
        /// <param name="client"></param>
        /// <param name="session"></param>
        /// <param name="chatNamespaceName"></param>
        /// <param name="onListSubscribeRooms"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator ListSubscribeRooms(
            Client client,
            GameSession session,
            string chatNamespaceName,
            ListSubscribeRoomsEvent onListSubscribeRooms,
            ErrorEvent onError
        )
        {
            AsyncResult<EzListSubscribeRoomsResult> result = null;
            yield return client.Chat.ListSubscribeRooms(
                r => { result = r; },
                session,
                chatNamespaceName,
                null,
                null
            );

            if (result.Error != null)
            {
                onError.Invoke(result.Error);
                yield break;
            }

            rooms = result.Result.Items;
            
            onListSubscribeRooms.Invoke(rooms);
        }
        
        /// <summary>
        /// ルームを作成
        /// </summary>
        /// <param name="client"></param>
        /// <param name="session"></param>
        /// <param name="chatNamespaceName"></param>
        /// <param name="roomName"></param>
        /// <param name="onCreateRoom"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator CreateRoom(
            Client client,
            GameSession session,
            string chatNamespaceName,
            string roomName,
            CreateRoomEvent onCreateRoom,
            ErrorEvent onError
        )
        {
            AsyncResult<EzCreateRoomResult> result = null;
            yield return client.Chat.CreateRoom(
                r => { result = r; },
                session,
                chatNamespaceName,
                roomName,
                null,
                null,
                null
            );

            if (result.Error != null)
            {
                onError.Invoke(result.Error);
                yield break;
            }

            room = result.Result.Item;
            
            onCreateRoom.Invoke(room);
        }
        
        /// <summary>
        /// ルームを購読
        /// </summary>
        /// <returns></returns>
        public IEnumerator Subscribe(
            Client client,
            GameSession session,
            string chatNamespaceName,
            string roomName,
            SubscribeEvent onSubscribe,
            ErrorEvent onError
        )
        {
            AsyncResult<EzSubscribeResult> result = null;
            List<EzNotificationType> notificationTypes = new List<EzNotificationType>()
            {
                new EzNotificationType()
            };
            yield return client.Chat.Subscribe(
                r => { result = r; },
                session,
                chatNamespaceName,
                roomName,
                notificationTypes
            );

            if (result.Error != null)
            {
                onError.Invoke(result.Error);
                yield break;
            }

            var item = result.Result.Item;

            onSubscribe.Invoke(item);
        }
        
        /// <summary>
        /// ルームの購読を解除
        /// </summary>
        /// <returns></returns>
        public IEnumerator Unsubscribe(
            Client client,
            GameSession session,
            string chatNamespaceName,
            string roomName,
            UnsubscribeEvent onUnsubscribe,
            ErrorEvent onError
        )
        {
            AsyncResult<EzUnsubscribeResult> result = null;
            yield return client.Chat.Unsubscribe(
                r => { result = r; },
                session,
                chatNamespaceName,
                roomName
            );

            if (result.Error != null)
            {
                onError.Invoke(result.Error);
                yield break;
            }

            var item = result.Result.Item;

            onUnsubscribe.Invoke(item);
        }
        
        /// <summary>
        /// メッセージを取得
        /// </summary>
        /// <returns></returns>
        public IEnumerator ListMessages(
            Client client,
            GameSession session,
            string chatNamespaceName,
            string roomName,
            ListMessagesEvent onListMessages,
            ErrorEvent onError
        )
        {
            AsyncResult<EzListMessagesResult> result = null;
            yield return client.Chat.ListMessages(
                r => { result = r; },
                session,
                chatNamespaceName,
                roomName,
                null,
                null,
                null
            );

            if (result.Error != null)
            {
                onError.Invoke(result.Error);
                yield break;
            }

            var items = result.Result.Items;
            
            onListMessages.Invoke(items);
        }
        
        /// <summary>
        /// メッセージを投稿
        /// </summary>
        /// <returns></returns>
        public IEnumerator Post(
            Client client,
            GameSession session,
            string chatNamespaceName,
            string roomName,
            string message,
            PostEvent onPost,
            ErrorEvent onError
        )
        {
            AsyncResult<EzPostResult> result = null;
            yield return client.Chat.Post(
                r => { result = r; },
                session,
                chatNamespaceName,
                roomName,
                message,
                null,
                null
            );

            if (result.Error != null)
            {
                onError.Invoke(result.Error);
                yield break;
            }

            var item = result.Result.Item;
            
            onPost.Invoke(item);
        }
    }
}