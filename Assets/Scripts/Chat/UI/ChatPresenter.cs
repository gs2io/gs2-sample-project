using System.Collections;
using System.Collections.Generic;
using Gs2.Core.Exception;
using Gs2.Core.Model;
using Gs2.Gs2Chat.Model;
using Gs2.Sample.Friend;
using Gs2.Unity.Gs2Chat.Model;
using Gs2.Unity.Util;
using Gs2.Util.LitJson;
using UnityEngine;
using UnityEngine.Assertions;

namespace Gs2.Sample.Chat
{
    public class ChatPresenter : MonoBehaviour
    {
        /// <summary>
        /// GS2-Chat の設定値
        /// </summary>
        [SerializeField] private ChatSetting _chatSetting;

        [SerializeField] private ChatModel _chatModel;
        [SerializeField] private ChatView _chatView;

        [SerializeField] private FriendPresenter _friendPresenter;
        
        private readonly Dictionary<int, BalloonDescriptor> messages = new Dictionary<int, BalloonDescriptor>();

        /// <summary>
        /// 通知が届いたか
        /// </summary>
        private bool _recievedNotification;
        
        /// <summary>
        /// 通知の種別
        /// </summary>
        private string _issuer;
        
        /// <summary>
        /// 通知で対象になっているUserId
        /// </summary>
        /// <returns></returns>
        private string _userId;
        
        private void Start()
        {
            Assert.IsNotNull(_chatSetting);
            Assert.IsNotNull(_chatModel);
            Assert.IsNotNull(_chatView);
        }

        private void Update()
        {
            if (_recievedNotification)
            {
                StartCoroutine(ListMessages());
                
                _recievedNotification = false;
            }
        }
        
        private ErrorEvent _onError = new ErrorEvent();
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <returns></returns>
        public IEnumerator Initialize()
        {
            GameManager.Instance.Cllient.Profile.Gs2Session.OnNotificationMessage += PushNotificationHandler;

            bool roomNotFound = false;
            void OnError(Gs2Exception e)
            {
                _onError.RemoveListener(OnError);
                if (e.Errors[0].message == "chat.room.room.error.notFound")
                {
                    roomNotFound = true;
                }
            }
                
            _onError.AddListener(OnError);

            yield return _chatModel.GetRoom(
                GameManager.Instance.Cllient.Client,
                _chatSetting.chatNamespaceName,
                _chatSetting.roomName,
                _chatSetting.onGetRoom,
                _onError
            );

            if (_chatModel.room.Name != _chatSetting.roomName && roomNotFound)
            {
                yield return CreateRoom();
            }
            yield return ListMessages();
        }

        public void Finish()
        {
            GameManager.Instance.Cllient.Profile.Gs2Session.OnNotificationMessage -= PushNotificationHandler;
        }
        
        /// <summary>
        /// 任意のタイミングで届く通知
        /// ※メインスレッド外
        /// </summary>
        /// <param name="message"></param>
        public void PushNotificationHandler(NotificationMessage message)
        {
            Debug.Log("PushNotificationHandler :" + message.issuer);
            
            if (!message.issuer.StartsWith("Gs2Chat")) return;

            _issuer = message.issuer;

            if (message.issuer.EndsWith(":Post"))
            {
                var notification = JsonMapper.ToObject<PostNotification>(message.payload);
                _userId = notification.UserId;
                _recievedNotification = true;
            }
        }

        void ClearMessages()
        {
            foreach (var message in messages)
            {
                Destroy(message.Value.gameObject);
            }
            
            messages.Clear();
        }

        /// <summary>
        /// ルームを取得
        /// </summary>
        /// <returns></returns>
        private IEnumerator GetRoom()
        {
            UIManager.Instance.AddLog("ChatPresenter::GetRoom");

            yield return _chatModel.GetRoom(
                GameManager.Instance.Cllient.Client,
                _chatSetting.chatNamespaceName,
                _chatSetting.roomName,
                _chatSetting.onGetRoom,
                _chatSetting.onError
            );
        }
        
        /// <summary>
        /// 購読しているルームの一覧を取得
        /// </summary>
        /// <returns></returns>
        private IEnumerator ListSubscribeRooms()
        {
            UIManager.Instance.AddLog("ChatPresenter::ListSubscribeRooms");

            yield return _chatModel.ListSubscribeRooms(
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                _chatSetting.chatNamespaceName,
                _chatSetting.onListSubscribeRooms,
                _chatSetting.onError
            );
        }

        /// <summary>
        /// ルームを作成
        /// </summary>
        /// <returns></returns>
        private IEnumerator CreateRoom()
        {
            UIManager.Instance.AddLog("ChatPresenter::CreateRoom");

            yield return _chatModel.CreateRoom(
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                _chatSetting.chatNamespaceName,
                _chatSetting.roomName,
                _chatSetting.onCreateRoom,
                _chatSetting.onError
            );
        }
        
        /// <summary>
        /// 購読を開始
        /// </summary>
        public void OnSubscribe()
        {
            StartCoroutine(Subscribe());
            
            StartCoroutine(ListMessages());
        }
        
        /// <summary>
        /// ルームを購読
        /// </summary>
        public IEnumerator Subscribe()
        {
            UIManager.Instance.AddLog("ChatPresenter::SubscribeRoom");
            
            yield return _chatModel.Subscribe(
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                _chatSetting.chatNamespaceName,
                _chatSetting.roomName,
                _chatSetting.onSubscribe,
                _chatSetting.onError
            );
        }
        
        /// <summary>
        /// メッセージを取得
        /// </summary>
        public IEnumerator ListMessages()
        {
            UIManager.Instance.AddLog("ChatPresenter::ListMessages");
            
            void AddMessageAction(
                List<EzMessage> messages
            )
            {
                _chatView.messagePrefab.SetActive(false);
                _chatView.myMessagePrefab.SetActive(false);

                if (_chatView.messagesContent != null)
                {
                    foreach (Transform child in _chatView.messagesContent.transform)
                    {
                        if (child != null && child.gameObject != _chatView.messagePrefab && child.gameObject != _chatView.myMessagePrefab)
                        {
                            Destroy(child.gameObject);
                        }
                    }

                    foreach (var message in messages)
                    {
                        GameObject balloonObject;
                        if (message.UserId == GameManager.Instance.Session.Session.AccessToken.UserId)
                        {
                            balloonObject = Instantiate<GameObject>(_chatView.myMessagePrefab,
                                _chatView.messagesContent.transform);
                        }
                        else
                        {
                            balloonObject = Instantiate<GameObject>(_chatView.messagePrefab,
                                _chatView.messagesContent.transform);
                        }

                        var balloonDescriptor = balloonObject.GetComponent<BalloonDescriptor>();
                        balloonDescriptor.SetUserId(message.UserId);
                        balloonDescriptor.SetText(message.Metadata, false);
                        if (balloonDescriptor.button != null)
                        {
                            balloonDescriptor.button.onClick.AddListener(
                                () => { _friendPresenter.OnOpenPlayerInfo(message.UserId); }
                            );
                        }
                        balloonDescriptor.gameObject.SetActive(true);
                    }
                }
                
                _chatView.ScrollDown();
                
                _chatSetting.onListMessages.RemoveListener(AddMessageAction);
            }

            _chatSetting.onListMessages.AddListener(AddMessageAction);
            
            yield return _chatModel.ListMessages(
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                _chatSetting.chatNamespaceName,
                _chatSetting.roomName,
                _chatSetting.onListMessages,
                _chatSetting.onError
            );
        }
        
        /// <summary>
        /// 購読を解除
        /// </summary>
        public void OnUnsubscribe()
        {
            StartCoroutine(Unsubscribe());

            ClearMessages();
        }
        
        /// <summary>
        /// 購読を解除
        /// </summary>
        public IEnumerator Unsubscribe()
        {
            UIManager.Instance.AddLog("ChatPresenter::Unsubscribe");
            
            yield return _chatModel.Unsubscribe(
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                _chatSetting.chatNamespaceName,
                _chatSetting.roomName,
                _chatSetting.onUnsubscribe,
                _chatSetting.onError
            );
        }
        
        /// <summary>
        /// メッセージを投稿
        /// </summary>
        public void OnPost()
        {
            StartCoroutine(Post());
        }
        
        /// <summary>
        /// メッセージを投稿
        /// </summary>
        public IEnumerator Post()
        {
            UIManager.Instance.AddLog("ChatPresenter::Post");
            
            yield return _chatModel.Post(
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                _chatSetting.chatNamespaceName,
                _chatSetting.roomName,
                _chatView.GetMessage(),
                _chatSetting.onPost,
                _chatSetting.onError
            );

            _chatView.ClearMessage();
        }
    }
}