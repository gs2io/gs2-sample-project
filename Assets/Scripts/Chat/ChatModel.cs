using System.Collections;
using System.Collections.Generic;
using Gs2.Core.Exception;
using Gs2.Unity.Core;
using Gs2.Unity.Gs2Chat.Model;
using Gs2.Unity.Util;
using UnityEngine;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
#endif

namespace Gs2.Sample.Chat
{
    public class ChatModel : MonoBehaviour
    {
        public EzRoom Room;

        public List<EzSubscribe> Rooms;
        
        /// <summary>
        /// ルーム情報を取得
        /// Get room information
        /// </summary>
        public IEnumerator GetRoom(
            Gs2Domain gs2,
            string chatNamespaceName,
            string roomName,
            GetRoomEvent onGetRoom,
            ErrorEvent onError
        )
        {
            var domain = gs2.Chat.Namespace(
                namespaceName: chatNamespaceName
            ).User(
                userId: null
            ).Room(
                roomName: roomName,
                password: null
            );
            var future = domain.Model();
            yield return future;
            if (future.Error != null)
            {
                onError.Invoke(future.Error);
                yield break;
            }
            
            Room = future.Result;
            
            onGetRoom.Invoke(Room);
        }
#if GS2_ENABLE_UNITASK
        public async UniTask GetRoomAsync(
            Gs2Domain gs2,
            string chatNamespaceName,
            string roomName,
            GetRoomEvent onGetRoom,
            ErrorEvent onError
        )
        {
            var domain = gs2.Chat.Namespace(
                namespaceName: chatNamespaceName
            ).User(
                userId: null
            ).Room(
                roomName: roomName,
                password: null
            );
            try
            {
                Room = await domain.ModelAsync();
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e);
            }
            Room = await domain.ModelAsync();
            
            onGetRoom.Invoke(Room);
        }
#endif
        
        /// <summary>
        /// 購読しているルームの一覧を取得
        /// Get a list of rooms to which you are subscribed
        /// </summary>
        public IEnumerator ListSubscribeRooms(
            Gs2Domain gs2,
            GameSession gameSession,
            string chatNamespaceName,
            ListSubscribeRoomsEvent onListSubscribeRooms,
            ErrorEvent onError
        )
        {
            Rooms.Clear();
            var domain = gs2.Chat.Namespace(
                namespaceName: chatNamespaceName
            ).Me(
                gameSession: gameSession
            );
            var it = domain.Subscribes();
            while (it.HasNext())
            {
                yield return it.Next();
                if (it.Error != null)
                {
                    onError.Invoke(it.Error);
                    break;
                }

                if (it.Current != null)
                {
                    Rooms.Add(it.Current);
                }
            }
            
            onListSubscribeRooms.Invoke(Rooms);
        }
        
#if GS2_ENABLE_UNITASK
        public async UniTask ListSubscribeRoomsAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string chatNamespaceName,
            ListSubscribeRoomsEvent onListSubscribeRooms,
            ErrorEvent onError
        )
        {
            Rooms.Clear();
            var domain = gs2.Chat.Namespace(
                namespaceName: chatNamespaceName
            ).Me(
                gameSession: gameSession
            );
            try
            {
                Rooms = await domain.SubscribesAsync().ToListAsync();
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e);
            }
            
            onListSubscribeRooms.Invoke(Rooms);
        }
#endif

        /// <summary>
        /// ルームを作成
        /// Create Room
        /// </summary>
        public IEnumerator CreateRoom(
            Gs2Domain gs2,
            GameSession gameSession,
            string chatNamespaceName,
            string roomName,
            CreateRoomEvent onCreateRoom,
            ErrorEvent onError
        )
        {
            var domain = gs2.Chat.Namespace(
                namespaceName: chatNamespaceName
            ).Me(
                gameSession: gameSession
            );
            var future = domain.CreateRoom(
                name: roomName,
                metadata: null,
                password: null,
                whiteListUserIds: null
            );
            yield return future;
            if (future.Error != null)
            {
                onError.Invoke(future.Error);
                yield break;
            }

            var result = future.Result;
            var future2 = result.Model();
            yield return future2;
            if (future2.Error != null)
            {
                onError.Invoke(future2.Error);
                yield break;
            }
            
            Room = future2.Result;
            
            onCreateRoom.Invoke(Room);
        }
#if GS2_ENABLE_UNITASK
        public async UniTask CreateRoomAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string chatNamespaceName,
            string roomName,
            CreateRoomEvent onCreateRoom,
            ErrorEvent onError
        )
        {
            var domain = gs2.Chat.Namespace(
                namespaceName: chatNamespaceName
            ).Me(
                gameSession: gameSession
            );
            var result  = await domain.CreateRoomAsync(
                name: roomName,
                metadata: null,
                password: null,
                whiteListUserIds: null
            );
            try
            {
                Room = await result.ModelAsync();
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e);
            }
            Room = await result.ModelAsync();
            
            onCreateRoom.Invoke(Room);
        }
#endif
        
        /// <summary>
        /// ルームを購読
        /// Subscribe to Room
        /// </summary>
        public IEnumerator Subscribe(
            Gs2Domain gs2,
            GameSession gameSession,
            string chatNamespaceName,
            string roomName,
            SubscribeEvent onSubscribe,
            ErrorEvent onError
        )
        {
            var domain = gs2.Chat.Namespace(
                namespaceName: chatNamespaceName
            ).Me(
                gameSession: gameSession
            ).Subscribe(
                roomName: roomName
            );
            var future = domain.Subscribe(
                notificationTypes: new [] {
                    new EzNotificationType()
                }
            );
            yield return future;
            if (future.Error != null)
            {
                onError.Invoke(future.Error);
                yield break;
            }
            
            var result = future.Result;
            var future2 = result.Model();
            yield return future2;
            if (future2.Error != null)
            {
                onError.Invoke(future2.Error);
                yield break;
            }
            
            var item = future2.Result;
            onSubscribe.Invoke(item);
        }
#if GS2_ENABLE_UNITASK
        public async UniTask SubscribeAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string chatNamespaceName,
            string roomName,
            SubscribeEvent onSubscribe,
            ErrorEvent onError
        )
        {
            var domain = gs2.Chat.Namespace(
                namespaceName: chatNamespaceName
            ).Me(
                gameSession: gameSession
            ).Subscribe(
                roomName: roomName
            );
            try
            {
                var result  = await domain.SubscribeAsync(
                    notificationTypes: new [] {
                        new EzNotificationType()
                    }
                );
                var item = await result.ModelAsync();
                onSubscribe.Invoke(item);
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e);
            }
        }
#endif
        /// <summary>
        /// ルームの購読を解除
        /// Unsubscribe from Room
        /// </summary>
        public IEnumerator Unsubscribe(
            Gs2Domain gs2,
            GameSession gameSession,
            string chatNamespaceName,
            string roomName,
            UnsubscribeEvent onUnsubscribe,
            ErrorEvent onError
        )
        {
            var domain = gs2.Chat.Namespace(
                namespaceName: chatNamespaceName
            ).Me(
                gameSession: gameSession
            ).Subscribe(
                roomName: roomName
            );
            var future = domain.Unsubscribe(
            );
            yield return future;
            if (future.Error != null)
            {
                onError.Invoke(future.Error);
                yield break;
            }
            
            var result = future.Result;
            var future2 = result.Model();
            yield return future2;
            if (future2.Error != null)
            {
                onError.Invoke(future2.Error);
                yield break;
            }
            
            var item = future2.Result; 
            onUnsubscribe.Invoke(item);
        }
#if GS2_ENABLE_UNITASK
        public async UniTask UnsubscribeAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string chatNamespaceName,
            string roomName,
            UnsubscribeEvent onUnsubscribe,
            ErrorEvent onError
        )
        {
            var domain = gs2.Chat.Namespace(
                namespaceName: chatNamespaceName
            ).Me(
                gameSession: gameSession
            ).Subscribe(
                roomName: roomName
            );
            try
            {
                var result = await domain.UnsubscribeAsync();
                var item = await result.ModelAsync();

                onUnsubscribe.Invoke(item);
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e);
            }
        }
#endif
        
        /// <summary>
        /// メッセージを取得
        /// Get Message
        /// </summary>
        /// <returns></returns>
        public IEnumerator ListMessages(
            Gs2Domain gs2,
            GameSession gameSession,
            string chatNamespaceName,
            string roomName,
            ListMessagesEvent onListMessages,
            ErrorEvent onError
        )
        {
            var domain = gs2.Chat.Namespace(
                namespaceName: chatNamespaceName
            ).Me(
                gameSession: gameSession
            ).Room(
                roomName: roomName,
                password: null
            );
            var it = domain.Messages();
            List<EzMessage> massages = new List<EzMessage>();
            while (it.HasNext())
            {
                yield return it.Next();
                if (it.Error != null)
                {
                    onError.Invoke(it.Error);
                    break;
                }

                if (it.Current != null)
                {
                    massages.Add(it.Current);
                }
                else
                {
                    break;
                }
            }
            
            onListMessages.Invoke(massages);
        }
#if GS2_ENABLE_UNITASK
        public async UniTask ListMessagesAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string chatNamespaceName,
            string roomName,
            ListMessagesEvent onListMessages,
            ErrorEvent onError
        )
        {
            var domain = gs2.Chat.Namespace(
                namespaceName: chatNamespaceName
            ).Me(
                gameSession: gameSession
            ).Room(
                roomName: roomName,
                password: null
            );
            try
            {
                List<EzMessage> massages = await domain.MessagesAsync().ToListAsync();
                
                onListMessages.Invoke(massages);
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e);
            }
        }
#endif
        
        /// <summary>
        /// メッセージを投稿
        /// Post a message
        /// </summary>
        /// <returns></returns>
        public IEnumerator Post(
            Gs2Domain gs2,
            GameSession gameSession,
            string chatNamespaceName,
            string roomName,
            string message,
            PostEvent onPost,
            ErrorEvent onError
        )
        {
            var domain = gs2.Chat.Namespace(
                namespaceName: chatNamespaceName
            ).Me(
                gameSession: gameSession
            ).Room(
                roomName: roomName,
                password: null
            );
            var future = domain.Post(
                metadata: message,
                category: null
            );
            yield return future;
            if (future.Error != null)
            {
                onError.Invoke(future.Error);
                yield break;
            }
            
            var result = future.Result;
            var future2 = result.Model();
            yield return future2;
            if (future2.Error != null)
            {
                onError.Invoke(future2.Error);
                yield break;
            }
            
            var item = future2.Result; 
            onPost.Invoke(item);
        }
#if GS2_ENABLE_UNITASK
        public async UniTask PostAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string chatNamespaceName,
            string roomName,
            string message,
            PostEvent onPost,
            ErrorEvent onError
        )
        {
            var domain = gs2.Chat.Namespace(
                namespaceName: chatNamespaceName
            ).Me(
                gameSession: gameSession
            ).Room(
                roomName: roomName,
                password: null
            );
            try
            {
                var result = await domain.PostAsync(
                    metadata: message,
                    category: null
                );
                var item = await result.ModelAsync();
                onPost.Invoke(item);
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e);
            }
        }
#endif
    }
}