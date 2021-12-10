using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using Gs2.Core;
using Gs2.Sample.Core.Runtime;
using Gs2.Unity.Gs2Realtime;
using Gs2.Unity.Gs2Realtime.Model;
using Gs2.Unity.Gs2Realtime.Result;
using Gs2.Unity.Gs2Realtime.Util;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

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
        [SerializeField] private RealtimeView _view;

        [SerializeField] public PlayerDescriptor myCharacter;

        private readonly Dictionary<uint, OtherPlayerDescriptor> players = new Dictionary<uint, OtherPlayerDescriptor>();

        private bool[] _handType = new bool[3];
        private RPSState _rpsState = RPSState.Select;
        
        public enum State
        {
            Initialize,
            GetRoom,
            GetRoomFailed,
            
            ConnectRoom,
            ConnectRoomFailed,
            
            SyncPlayerProfiles,
            SyncPlayerProfilesFailed,
            
            Main,
            Disconnected,
            Error,
            
            Leave,
        }
        
        private State _realtimeState = State.Initialize;

        private void Start()
        {
            Assert.IsNotNull(_realtimeSetting);
            Assert.IsNotNull(_realtimeModel);
            
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
                            p.Deserialize(data);
                    }
                    else
                    {
                        JoinPlayerHandler(player);
                    }
                }
            );
        }

        private void Update()
        {
            _view.SetPlayerCount(players.Count + 1);

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
                _view.SetResult("");
                return;
            }

            // 勝敗判定
            if (players.Count > 1)
            {
                // draw判定
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
                    _view.SetResult("DRAW");
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
                    _view.SetResult("DRAW");
                    break;
                case 1:
                    _view.SetResult("LOSE");
                    break;
                case 2:
                    _view.SetResult("WIN");
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
                        _view.OnDisableEvent();
                        break;
                    
                    case State.GetRoom:
                        UIManager.Instance.OpenProcessing();
                        
                        StartCoroutine(
                            GetRoom()
                        );
                        break;
                        
                    case State.ConnectRoom:
                        UIManager.Instance.OpenProcessing();
                        
                        StartCoroutine(
                            ConnectRoom()
                        );
                        
                        break;
                    
                    case State.SyncPlayerProfiles:
                        UIManager.Instance.OpenProcessing();
                        
                        StartCoroutine(
                            SyncPlayerProfiles()
                        );
                        
                        break;
                    case State.GetRoomFailed:
                    case State.ConnectRoomFailed:
                    case State.SyncPlayerProfilesFailed:
                        UIManager.Instance.CloseProcessing();
                        _view.OnDisableEvent();
                        break;
                    
                    case State.Main:
                        UIManager.Instance.CloseProcessing();
                        _view.OnEnableEvent();
                        
                        StartCoroutine(myCharacter.SendStatus());
                        
                        break;
                    
                    case State.Error:
                    case State.Disconnected:
                    case State.Leave:
                        _view.OnDisableEvent();
                        break;
                }
            }
            _realtimeState = _state;
        }
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <returns></returns>
        public void Initialize()
        {
            UIManager.Instance.AddLog("Initialize");
            
            SetState(State.GetRoom);
        }
        
        void JoinPlayerHandler(Gs2.Gs2Realtime.Message.Player player)
        {
            if (myCharacter == null || myCharacter.Session == null) return;
            if (player.ConnectionId == myCharacter.Session.MyConnectionId) return;

            _view.OtherPlayerPrefab.SetActive(false);

            var otherPlayer = Instantiate<GameObject>(_view.OtherPlayerPrefab, _view.OtherPlayerPrefab.transform.parent);
            otherPlayer.SetActive(true);
            players[player.ConnectionId] = otherPlayer.GetComponent<OtherPlayerDescriptor>();
        }
        
        void ClearPlayers()
        {
            foreach (Transform child in _view.joinedPlayersContent.transform)
            {
                if (child != null && child.gameObject != _view.OtherPlayerPrefab)
                {
                    Destroy(child.gameObject);
                }
            }
        }
        
        
        /// <summary>
        /// GS2-Realtime のルーム情報を取得
        /// </summary>
        /// <returns></returns>
        private IEnumerator GetRoom()
        {
            UIManager.Instance.AddLog("RealtimePresenter::GetRoom");
            
            if (!string.IsNullOrEmpty(_realtimeModel.room.IpAddress))
            {
                _realtimeModel.room = new EzRoom
                {
                    Name = _realtimeModel.room.Name,
                    IpAddress = _realtimeModel.room.IpAddress,
                    Port = _realtimeModel.room.Port,
                    EncryptionKey = _realtimeModel.room.EncryptionKey,
                };
                SetState(State.ConnectRoom);
                yield break;
            }
            
            while (true)
            {
                yield return new WaitForSeconds(0.5f);
                
                AsyncResult<EzGetRoomResult> result = null;
                yield return _realtimeModel.GetRoom(
                    r => { result = r; },
                    GameManager.Instance.Cllient.Client,
                    _realtimeSetting.realtimeNamespaceName,
                    _realtimeSetting.onGetRoom,
                    _realtimeSetting.onError
                );
            
                if (result.Error != null)
                {
                    SetState(State.GetRoomFailed);
                    yield break;
                }

                if (!string.IsNullOrEmpty(result.Result.Item.IpAddress))
                {
                    break;
                }
            }
            
            SetState(State.ConnectRoom);
        }
        
        /// <summary>
        /// GS2-Realtime のルームに接続
        /// </summary>
        /// <returns></returns>
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
                    myCharacter.Messenger = new Messenger(_realtimeModel.room.EncryptionKey);
                },
                _realtimeModel.room.IpAddress,
                _realtimeModel.room.Port,
                _realtimeModel.room.EncryptionKey
            );
        }
        
        /// <summary>
        /// GS2-Realtime のルームに接続
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <param name="encryptionKey"></param>
        /// <returns></returns>
        public IEnumerator ConnectRoom(
            UnityAction<AsyncResult<RelayRealtimeSession>> callback,
            string ipAddress,
            int port,
            string encryptionKey
        )
        {
            UIManager.Instance.AddLog("RealtimePresenter::ConnectRoom");
            
            var realtimeSession = new RelayRealtimeSession(
                GameManager.Instance.Session.Session.AccessToken.Token,
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
                        result.Error
                    );
                    callback.Invoke(
                        new AsyncResult<RelayRealtimeSession>(null, result.Error)
                    );
                }
            }
        }
                
        /// <summary>
        /// 他プレイヤーの座標情報を同期
        /// </summary>
        /// <returns></returns>
        private IEnumerator SyncPlayerProfiles()
        {
            UIManager.Instance.AddLog("RealtimePresenter::SyncPlayerProfiles");
            
            yield return SyncPlayerProfiles(
                r =>
                {
                    SetState(State.Main);
                }
            );
        }
        
        /// <summary>
        /// 他プレイヤーの座標情報を同期
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IEnumerator SyncPlayerProfiles(
            UnityAction<AsyncResult<object>> callback
        )
        {
            callback.Invoke(null);
            yield break;
        }

        public void OnLeaveRoom()
        {
            SetState(State.Leave);

            ClearPlayers();
            players.Clear();
            
            StartCoroutine(
                _realtimeModel.realtimeSession.Close()
            );
            
            _realtimeModel.Clear();
        }
        
        /// <summary>
        /// ルームを選択
        /// </summary>
        /// <param name="gathering"></param>
        /// <param name="joinPlayerIds"></param>
        public void OnSelectRoomName(string roomName)
        {
            _realtimeModel.gatheringId = roomName;
            
            Initialize();
        }
    }
}