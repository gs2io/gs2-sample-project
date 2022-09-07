using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using Gs2.Core;
using Gs2.Core.Model;
using Gs2.Gs2Realtime.Model;
using Gs2.Unity.Gs2Realtime;
using Gs2.Unity.Gs2Realtime.Result;
using Gs2.Util.LitJson;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using Gs2.Core.Exception;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
#endif

namespace Gs2.Sample.Realtime
{
    public enum RPSState
    {
        Select = 0,
        Decide = 1,
    }

    public enum RPSType
    {
        Stone = 0,
        Scissors = 1,
        Paper = 2,
    }
    
    public class RealtimePresenter : MonoBehaviour
    {
        /// <summary>
        /// GS2-Realtime の設定値
        /// </summary>
        [SerializeField] private RealtimeSetting _realtimeSetting;

        [SerializeField] private RealtimeModel _realtimeModel;
        [SerializeField] private RealtimeView _realtimeView;

        [SerializeField] public PlayerDescriptor myCharacter;

        private readonly Dictionary<uint, OtherPlayerDescriptor> players = new Dictionary<uint, OtherPlayerDescriptor>();

        private bool[] _handType = new bool[3];
        private RPSState _rpsState = RPSState.Select;
        
        public enum State
        {
            Initialize,
            
            /// <summary>
            /// ルーム情報を取得
            /// </summary>
            GetRoom,
            /// <summary>
            /// ルーム情報の取得に失敗
            /// </summary>
            GetRoomFailed,
            
            /// <summary>
            /// ルームに接続
            /// </summary>
            ConnectRoom,
            /// <summary>
            /// ルームへの接続に失敗
            /// </summary>
            ConnectRoomFailed,
            
            /// <summary>
            /// 他プレイヤーと情報を同期
            /// </summary>
            SyncPlayerProfiles,
            /// <summary>
            /// 他プレイヤーと情報の同期に失敗
            /// </summary>
            SyncPlayerProfilesFailed,
            
            /// <summary>
            /// ゲームプレイ中
            /// </summary>
            Main,
            /// <summary>
            /// 切断
            /// </summary>
            Disconnected,

            Error,
            
            /// <summary>
            /// 退室
            /// </summary>
            Leave,
        }
        
        private State _realtimeState = State.Initialize;

        /// <summary>
        /// 通知の種別
        /// </summary>
        private string _issuer;
        
        private void Start()
        {
            Assert.IsNotNull(_realtimeSetting);
            Assert.IsNotNull(_realtimeModel);
            Assert.IsNotNull(_realtimeView);
            
            _realtimeSetting.onJoinPlayer.AddListener(JoinPlayerHandler);
            
            _realtimeSetting.onLeavePlayer.AddListener(
                player =>
                {
                    if (players.ContainsKey(player.ConnectionId))
                    {
                        Destroy(players[player.ConnectionId].gameObject);
                        players.Remove(player.ConnectionId);
                    }

                }
            );
            
            _realtimeSetting.onUpdateProfile.AddListener(
                player => 
                {
                    if (players.ContainsKey(player.ConnectionId))
                    {
                        var data = player.Profile.ToByteArray();
                        var p = players[player.ConnectionId];
                        if (p != null)
                            p.ProfileDeserialize(data);
                    }
                    else
                    {
                        JoinPlayerHandler(player);
                    }
                }
            );
            _realtimeSetting.onRelayMessage.AddListener(
                message => 
                {
                    if (players.ContainsKey(message.ConnectionId))
                    {
                        var data = message.Data.ToByteArray();
                        var p = players[message.ConnectionId];
                        if (p != null)
                            p.StateDeserialize(data);
                    }
                }
            );
        }
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Initialize()
        {
            UIManager.Instance.AddLog("Initialize");
            
            GameManager.Instance.Profile.Gs2Session.OnNotificationMessage += PushNotificationHandler;
        }
        
        public void Finish()
        {
            GameManager.Instance.Profile.Gs2Session.OnNotificationMessage -= PushNotificationHandler;
        }
        
        /// <summary>
        /// 任意のタイミングで届く通知
        /// ※メインスレッド外
        /// Notification delivered at any given time
        /// *Outside the main thread
        /// </summary>
        public void PushNotificationHandler(NotificationMessage message)
        {
            Debug.Log("PushNotificationHandler :" + message.issuer);
            
            if (!message.issuer.StartsWith("Gs2Realtime:")) return;

            _issuer = message.issuer;
            if (message.issuer.EndsWith(":Create"))
            {
                var notification = CreateNotification.FromJson(JsonMapper.ToObject(message.payload));
                Debug.Log("RoomName :" + notification.RoomName);
            }
        }

        private void Update()
        {
            _realtimeView.SetPlayerCount(players.Count + 1);

            int result = -1;
            _rpsState = RPSState.Decide;
            if (myCharacter.state == RPSState.Select)
            {
                _rpsState = RPSState.Select;
            }
            foreach (var player in players)
            {
                var otherPlayer = player.Value;
                if (otherPlayer.state == RPSState.Select)
                {
                    _rpsState = RPSState.Select;
                }
            }

            foreach (var player in players)
            {
                var p = player.Value;
                p.SetMembersState(_rpsState);
            }
            
            if (_rpsState != RPSState.Decide)
            {
                _realtimeView.SetResult("");
                return;
            }

            if (players.Count > 1)
            {
                _handType[0]=false; _handType[1]=false; _handType[2]=false;
                
                foreach (var player in players)
                {
                    var otherPlayer = player.Value;
                    _handType[(int)otherPlayer.handType] = true;
                }
                _handType[(int)myCharacter.handType] = true;

                int cnt = 0;
                for (int i = 0; i < 3; i++)
                {
                    if (_handType[i]) cnt++;
                }
                if (cnt == 3 || cnt == 1)
                {
                    _realtimeView.SetResult("DRAW");
                    return;
                }

                foreach (var player in players)
                {
                    if (myCharacter.handType == player.Value.handType)
                        continue;
                    
                    var otherPlayer = player.Value;
                    result = (myCharacter.handType - otherPlayer.handType + 3) % 3;
                    break;
                }
            }
            else
            {
                foreach (var player in players)
                {
                    var otherPlayer = player.Value;
                    result = (myCharacter.handType - otherPlayer.handType + 3) % 3;
                }
            }

            switch (result)
            {
                case 0:
                    _realtimeView.SetResult("DRAW");
                    break;
                case 1:
                    _realtimeView.SetResult("LOSE");
                    break;
                case 2:
                    _realtimeView.SetResult("WIN");
                    break;
            }
        }

        private void SetState(State _state)
        {
            if (_realtimeState != _state)
            {
                switch (_state)
                {
                    default:
                        _realtimeView.OnDisableEvent();
                        break;
                    
                    case State.GetRoom:
                        UIManager.Instance.OpenProcessing();
#if GS2_ENABLE_UNITASK
                        GetRoomAsync().Forget();
#else
                        StartCoroutine(
                            GetRoom()
                        );
#endif
                        break;
                        
                    case State.ConnectRoom:
                        UIManager.Instance.OpenProcessing();
#if GS2_ENABLE_UNITASK
                        ConnectRoomAsync().Forget();
#else
                        StartCoroutine(
                            ConnectRoom()
                        );
#endif
                        break;
                    
                    case State.SyncPlayerProfiles:
                        UIManager.Instance.OpenProcessing();
                        
                        SetState(State.Main);
                        
                        break;
                    
                    case State.GetRoomFailed:
                    case State.ConnectRoomFailed:
                    case State.SyncPlayerProfilesFailed:
                        UIManager.Instance.CloseProcessing();
                        _realtimeView.OnDisableEvent();
                        break;
                    
                    case State.Main:
                        UIManager.Instance.CloseProcessing();
                        _realtimeView.OnEnableEvent();
#if GS2_ENABLE_UNITASK
                        myCharacter.UpdateProfileAsync().Forget();
#else
                        StartCoroutine(
                            myCharacter.UpdateProfile()
                        );
#endif
                        break;
                    
                    case State.Error:
                    case State.Disconnected:
                    case State.Leave:
                        _realtimeView.OnDisableEvent();
                        break;
                }
            }
            _realtimeState = _state;
        }

        /// <summary>
        /// ルーム取得を開始
        /// Start acquiring rooms
        /// </summary>
        /// <returns></returns>
        public void StartGetRoom()
        {
            SetState(State.GetRoom);
        }
        
        void JoinPlayerHandler(Gs2.Gs2Realtime.Message.Player player)
        {
            if (myCharacter == null || myCharacter.Session == null) return;
            if (player.ConnectionId == myCharacter.Session.MyConnectionId) return;

            _realtimeView.OtherPlayerPrefab.SetActive(false);

            var otherPlayer = Instantiate<GameObject>(_realtimeView.OtherPlayerPrefab, _realtimeView.OtherPlayerPrefab.transform.parent);
            otherPlayer.SetActive(true);
            players[player.ConnectionId] = otherPlayer.GetComponent<OtherPlayerDescriptor>();
        }

        void ClearPlayers()
        {
            foreach (Transform child in _realtimeView.joinedPlayersContent.transform)
            {
                if (child != null && child.gameObject != _realtimeView.OtherPlayerPrefab)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// GS2-Realtime のルーム情報を取得
        /// Get GS2-Realtime room information
        /// </summary>
        private IEnumerator GetRoom()
        {
            UIManager.Instance.AddLog("RealtimePresenter::GetRoom");
            
            while (true)
            {
                yield return new WaitForSeconds(0.5f);

                Gs2.Unity.Gs2Realtime.Model.EzRoom result = null;
                yield return _realtimeModel.GetRoom(
                    r => { result = r; },
                    GameManager.Instance.Domain,
                    _realtimeSetting.realtimeNamespaceName,
                    _realtimeSetting.onGetRoom,
                    _realtimeSetting.onError
                );
            
                if (result == null)
                {
                    SetState(State.GetRoomFailed);
                    yield break;
                }

                if (!string.IsNullOrEmpty(result.IpAddress))
                {
                    break;
                }
            }
            
            SetState(State.ConnectRoom);
        }
#if GS2_ENABLE_UNITASK
        /// <summary>
        /// GS2-Realtime のルーム情報を取得
        /// Get GS2-Realtime room information
        /// </summary>
        /// <returns></returns>
        private async UniTask GetRoomAsync()
        {
            UIManager.Instance.AddLog("RealtimePresenter::GetRoomAsync");
            
            while (true)
            {
                await UniTask.Delay(500);

                Gs2.Unity.Gs2Realtime.Model.EzRoom result = null;
                try
                {
                    result = await _realtimeModel.GetRoomAsync(
                        GameManager.Instance.Domain,
                        _realtimeSetting.realtimeNamespaceName,
                        _realtimeSetting.onGetRoom,
                        _realtimeSetting.onError
                    );
                }
                catch (Exception)
                {
                    SetState(State.GetRoomFailed);
                    return;
                }

                if (!string.IsNullOrEmpty(result.IpAddress))
                {
                    break;
                }
            }
            
            SetState(State.ConnectRoom);
        }
#endif
        
        /// <summary>
        /// GS2-Realtime のルームに接続
        /// Connect to GS2-Realtime room
        /// </summary>
        private IEnumerator ConnectRoom()
        {
            yield return ConnectRoom(
                (r) =>
                {
                    SetState(r.Error == null
                        ? State.SyncPlayerProfiles
                        : State.ConnectRoomFailed
                    );
                    _realtimeModel.realtimeSession = r.Result;
                    
                    myCharacter.Session = _realtimeModel.realtimeSession;
                },
                _realtimeModel.room.IpAddress,
                _realtimeModel.room.Port,
                _realtimeModel.room.EncryptionKey
            );
        }
#if GS2_ENABLE_UNITASK
        private async UniTask ConnectRoomAsync()
        {
            var session = await ConnectRoomAsync(
                _realtimeModel.room.IpAddress,
                _realtimeModel.room.Port,
                _realtimeModel.room.EncryptionKey
            );
            if (session == null)
            {
                SetState(State.ConnectRoomFailed);
                myCharacter.Session = _realtimeModel.realtimeSession = null;
            }
            else
            {
                SetState(State.SyncPlayerProfiles);
                myCharacter.Session = _realtimeModel.realtimeSession = session;
            }
        }
#endif
        
        /// <summary>
        /// GS2-Realtime のルームに接続
        /// Connect to GS2-Realtime room
        /// </summary>
        public IEnumerator ConnectRoom(
            UnityAction<AsyncResult<RelayRealtimeSession>> callback,
            string ipAddress,
            int port,
            string encryptionKey
        )
        {
            UIManager.Instance.AddLog("RealtimePresenter::ConnectRoom");
            
            var realtimeSession = new RelayRealtimeSession(
                GameManager.Instance.Session.AccessToken.Token,
                ipAddress,
                port,
                encryptionKey,
                ByteString.CopyFrom()
            );
            
            realtimeSession.OnRelayMessage += message =>
            {
                _realtimeSetting.onRelayMessage.Invoke(message);
            }; 
            realtimeSession.OnJoinPlayer += player =>
            {
                _realtimeSetting.onJoinPlayer.Invoke(player);
            };
            realtimeSession.OnLeavePlayer += player =>
            {
                _realtimeSetting.onLeavePlayer.Invoke(player);
            };
            realtimeSession.OnGeneralError += args => 
            {
                _realtimeSetting.onGeneralError.Invoke(args);
            };
            realtimeSession.OnError += error =>
            {
                _realtimeSetting.onRelayError.Invoke(error);
            };
            realtimeSession.OnUpdateProfile += player =>
            {
                _realtimeSetting.onUpdateProfile.Invoke(player);
            };
            realtimeSession.OnClose += args =>
            {
                _realtimeSetting.onClose.Invoke(args);
            };

            AsyncResult<bool> result = null;
            yield return realtimeSession.Connect(
                this,
                r =>
                {
                    result = r;
                }
            );

            if (realtimeSession.Connected)
            {
                callback.Invoke(
                    new AsyncResult<RelayRealtimeSession>(realtimeSession, null)
                );
            }
            else
            {
                if (result.Error != null)
                {
                    _realtimeSetting.onError.Invoke(
                        result.Error,
                        null
                    );
                    callback.Invoke(
                        new AsyncResult<RelayRealtimeSession>(null, result.Error)
                    );
                }
            }
        }
#if GS2_ENABLE_UNITASK
        private async UniTask<RelayRealtimeSession> ConnectRoomAsync(
            string ipAddress,
            int port,
            string encryptionKey
        )
        {
            UIManager.Instance.AddLog("RealtimePresenter::ConnectRoomAsync");
            
            var realtimeSession = new RelayRealtimeSession(
                GameManager.Instance.Session.AccessToken.Token,
                ipAddress,
                port,
                encryptionKey,
                ByteString.CopyFrom()
            );
            
            realtimeSession.OnRelayMessage += message =>
            {
                _realtimeSetting.onRelayMessage.Invoke(message);
            }; 
            realtimeSession.OnJoinPlayer += player =>
            {
                _realtimeSetting.onJoinPlayer.Invoke(player);
            };
            realtimeSession.OnLeavePlayer += player =>
            {
                _realtimeSetting.onLeavePlayer.Invoke(player);
            };
            realtimeSession.OnGeneralError += args => 
            {
                _realtimeSetting.onGeneralError.Invoke(args);
            };
            realtimeSession.OnError += error =>
            {
                _realtimeSetting.onRelayError.Invoke(error);
            };
            realtimeSession.OnUpdateProfile += player =>
            {
                _realtimeSetting.onUpdateProfile.Invoke(player);
            };
            realtimeSession.OnClose += args =>
            {
                _realtimeSetting.onClose.Invoke(args);
            };

            try
            {
                await realtimeSession.ConnectAsync(
                    this
                );
            }
            catch (Gs2Exception e)
            {
                _realtimeSetting.onError.Invoke(
                    e,
                    null
                );
                return null;
            }
            
            return realtimeSession;
        }
#endif
        
        /// <summary>
        /// 退室
        /// exit from a room
        /// </summary>
        public void OnLeaveRoom()
        {
            SetState(State.Leave);

            ClearPlayers();
            players.Clear();
            
#if GS2_ENABLE_UNITASK
            _realtimeModel.realtimeSession.CloseAsync().Forget();
#else
            StartCoroutine(
                _realtimeModel.realtimeSession.Close()
            );
#endif
            
            _realtimeModel.Clear();
            
            SetState(State.Initialize);
        }
        
        
        public void OnEnterRoom()
        {
            if (_realtimeModel.room != null)
            {
                SetState(State.ConnectRoom);
            }
            else
            {
                SetState(State.GetRoom);
            }
        }
    }
}