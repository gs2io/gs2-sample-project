using System;
using System.Collections;
using Gs2.Core;
using Gs2.Core.Exception;
using Gs2.Core.Model;
using Gs2.Gs2Matchmaking.Model;
using Gs2.Sample.Core.Runtime;
using Gs2.Unity.Gs2Matchmaking.Model;
using Gs2.Unity.Gs2Matchmaking.Result;
using Gs2.Util.LitJson;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace Gs2.Sample.Matchmaking
{
    public class MatchmakingPresenter : MonoBehaviour
    {
        /// <summary>
        /// GS2-Matchmaking の設定値
        /// </summary>
        [SerializeField]
        private MatchmakingSetting _matchmakingSetting;
        
        [SerializeField]
        private MatchmakingModel _matchmakingModel;
        
        [SerializeField]
        private CreateGatheringView _createGatheringView;
        [SerializeField]
        private MatchmakingView _matchmakingView;
        [SerializeField]
        private JoinGatheringView _joinGatheringView;
        
        public enum State
        {
            MainMenu,
            
            /// <summary>
            /// ギャザリング作成 メニュー
            /// </summary>
            CreateGatheringMenu,
            /// <summary>
            /// ギャザリング作成 開始
            /// </summary>
            CreateGathering,
            /// <summary>
            /// ギャザリング作成 失敗
            /// </summary>
            CreateGatheringFailed,
            
            /// <summary>
            /// ギャザリングに参加 開始
            /// </summary>
            JoinGathering,
            /// <summary>
            /// ギャザリングに参加 成功
            /// </summary>
            JoinGatheringSucceed,
            /// <summary>
            /// ギャザリングが見つからない
            /// </summary>
            GatheringNotFound,

            /// <summary>
            /// マッチメイキング中
            /// </summary>
            Matchmaking,
            /// <summary>
            /// マッチメイキングを中止　ギャザリングから離脱
            /// </summary>
            CancelMatchmaking,
            
            /// <summary>
            /// マッチメイキングが成立
            /// </summary>
            Complete,
            
            Error,
        }
        
        private State matchmakingState = State.MainMenu;
        
        /// <summary>
        /// マッチメイキングが完了したか
        /// </summary>
        private bool _complete;
        
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
        
        // Start is called before the first frame update
        void Start()
        {
            Assert.IsNotNull(_matchmakingSetting);
            Assert.IsNotNull(_matchmakingModel);
            Assert.IsNotNull(_createGatheringView);
            Assert.IsNotNull(_matchmakingView);
            Assert.IsNotNull(_joinGatheringView);
            
            _createGatheringView.OnCloseEvent();
            _matchmakingView.OnCloseEvent();
            _joinGatheringView.OnCloseEvent();
            
            _recievedNotification = false;
            _complete = false;
            
            _matchmakingSetting.onUpdateJoinedPlayerIds.AddListener(
                (gathering, joinedPlayerIds) =>
                {
                    Debug.Log("onUpdateJoinedPlayerIds");
                    
                    _matchmakingView.displayPlayerNamePrefab.SetActive(false);

                    if (_matchmakingView.joinedPlayersContent != null)
                    {
                        foreach (Transform child in _matchmakingView.joinedPlayersContent.transform)
                        {
                            if (child != null && child.gameObject != _matchmakingView.displayPlayerNamePrefab)
                            {
                                Destroy(child.gameObject);
                            }
                        }

                        foreach (var joinedPlayerId in joinedPlayerIds)
                        {
                            var gamePlayerName = Instantiate<GameObject>(_matchmakingView.displayPlayerNamePrefab,
                                _matchmakingView.joinedPlayersContent.transform);
                            var nameLabel = gamePlayerName.GetComponent<TextMeshProUGUI>();
                            nameLabel.text = joinedPlayerId;
                            nameLabel.enabled = true;
                            gamePlayerName.SetActive(true);
                        }
                    }
                }
            );
        }

        public void Initialize()
        {
            GameManager.Instance.Cllient.Profile.Gs2Session.OnNotificationMessage += PushNotificationHandler;
        }
        
        public void OnClose()
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
            
            if (!message.issuer.StartsWith("Gs2Matchmaking:")) return;

            _issuer = message.issuer;

            if (message.issuer.EndsWith(":Join"))
            {
                var notification = JsonMapper.ToObject<JoinNotification>(message.payload);
                if (!_matchmakingModel.JoinedPlayerIds.Contains(notification.joinUserId))
                {
                    _matchmakingModel.JoinedPlayerIds.Add(notification.joinUserId);
                    _userId = notification.joinUserId;
                    _recievedNotification = true;
                }
            }
            else if (message.issuer.EndsWith(":Leave"))
            {
                var notification = JsonMapper.ToObject<LeaveNotification>(message.payload);
                _matchmakingModel.JoinedPlayerIds.Remove(notification.leaveUserId);
                _userId = notification.leaveUserId;
                _recievedNotification = true;
            }
            else if (message.issuer.EndsWith(":Complete"))
            {
                _recievedNotification = true;
                _complete = true;
            }
        }
        
        private void Update()
        {
            if (_recievedNotification)
            {
                if (_issuer.EndsWith(":Join"))
                {
                    if (_matchmakingModel.Gathering != null)
                    {
                        _matchmakingSetting.onJoinPlayer.Invoke(_matchmakingModel.Gathering, _userId);
                        _matchmakingSetting.onUpdateJoinedPlayerIds.Invoke(_matchmakingModel.Gathering, _matchmakingModel.JoinedPlayerIds);
                    }
                }
                else if (_issuer.EndsWith(":Leave"))
                {
                    if (_matchmakingModel.Gathering != null)
                    {
                        _matchmakingSetting.onLeavePlayer.Invoke(_matchmakingModel.Gathering, _userId);
                        _matchmakingSetting.onUpdateJoinedPlayerIds.Invoke(_matchmakingModel.Gathering, _matchmakingModel.JoinedPlayerIds);
                    }
                }
                else if (_issuer.EndsWith(":Complete"))
                {
                    if (_matchmakingModel.Gathering != null)
                    {
                        // Joinと同時にマッチメイキングが成立する場合
                        // DoMatchmaking の応答より先にマッチメイキング完了通知が届くことがある
                        SetState(State.Complete);
                        _matchmakingSetting.onMatchmakingComplete.Invoke(_matchmakingModel.Gathering, _matchmakingModel.JoinedPlayerIds);

                        OnClose();
                    }
                }
                
                _issuer = String.Empty;
                _userId = String.Empty;
                _recievedNotification = false;
            }
        }
        
        private void SetState(State _state)
        {
            if (matchmakingState != _state)
            {
                switch (_state)
                {
                    default:
                        _createGatheringView.OnCloseEvent();
                        _matchmakingView.OnCloseEvent();
                        _joinGatheringView.OnCloseEvent();
                        break;

                    // ギャザリング作成ダイアログを開く
                    case State.CreateGatheringMenu:
                        _createGatheringView.OnOpenEvent();
                        break;
                    
                    // 新規ギャザリングを作成中
                    case State.CreateGathering:
                        _createGatheringView.OnCloseEvent();
                        break;
                    
                    // ギャザリングへのマッチングを待機中
                    case State.Matchmaking:
                        _createGatheringView.OnCloseEvent();
                        _joinGatheringView.OnCloseEvent();
                        _matchmakingView.OnOpenEvent();
                        break;
                    
                    // ギャザリングへの参加を開始
                    case State.JoinGathering:
                        _joinGatheringView.OnOpenEvent();
                        break;
                    
                    // ギャザリングへの参加に成功
                    case State.JoinGatheringSucceed:
                        _joinGatheringView.OnCloseEvent();
                        break;
                }
            }
            
            matchmakingState = _state;
        }
        
        /// <summary>
        /// ギャザリング作成ボタンをクリック
        /// </summary>
        public void OnClickToCreateGathering()
        {
            Initialize();
            
            SetState(State.CreateGatheringMenu);
        }

        /// <summary>
        /// 募集人数を確定してマッチメイキングを開始するボタンをクリック
        /// </summary>
        public void OnClickToSubmitCapacity()
        {
            _matchmakingModel.Capacity = int.Parse(_createGatheringView.capacityInputField.text);
            SetState(State.CreateGathering);
            StartCoroutine(
                SimpleMatchmakingCreateGatheringTask()
            );
        }

        public IEnumerator SimpleMatchmakingCreateGatheringTask()
        {
            AsyncResult<EzCreateGatheringResult> result = null;
            yield return _matchmakingModel.CreateGathering(
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                r => result = r,
                _matchmakingSetting.matchmakingNamespaceName,
                _matchmakingSetting.onUpdateJoinedPlayerIds,
                _matchmakingSetting.onError
            );
            
            if (result.Error != null)
            {
                SetState(State.CreateGatheringFailed);
                yield break;
            }
            
            SetState(State.Matchmaking);
        }
        
        /// <summary>
        /// 既存のギャザリングへの参加ボタンをクリック
        /// </summary>
        public void OnClickToJoinGathering()
        {
            Initialize();
            
            SetState(State.JoinGathering);
            
            StartCoroutine(
                SimpleMatchmakingJoinGathering()
            );
        }
        
        /// <summary>
        /// 既存のギャザリングに参加する
        /// </summary>
        /// <returns></returns>
        public IEnumerator SimpleMatchmakingJoinGathering(
        )
        {
            UIManager.Instance.AddLog("SimpleMatchmakingJoinGathering");
            
            while (matchmakingState == State.JoinGathering)
            {
                AsyncResult<EzDoMatchmakingResult> result = null;
                string contextToken = null;
                yield return _matchmakingModel.JoinGathering(
                    r => { result = r; },
                    GameManager.Instance.Cllient.Client,
                    GameManager.Instance.Session.Session,
                    _matchmakingSetting.matchmakingNamespaceName,
                    contextToken,
                    _matchmakingSetting.onError
                );
                
                if (result.Error != null)
                {
                    SetState(State.Error);
                    yield break;
                }

                if (result.Result.Item != null)
                {
                    _matchmakingModel.Gathering = result.Result.Item;
                    if (_matchmakingModel.Gathering.CapacityOfRoles.Count > 0)
                    {
                        foreach (var CapacityOfRole in _matchmakingModel.Gathering.CapacityOfRoles)
                        {
                            if (CapacityOfRole != null)
                            {
                                foreach (var player in CapacityOfRole.Participants)
                                {
                                    if (!_matchmakingModel.JoinedPlayerIds.Contains(player.UserId))
                                    {
                                        _matchmakingModel.JoinedPlayerIds.Add(player.UserId);
                                        _matchmakingSetting.onJoinPlayer.Invoke(_matchmakingModel.Gathering, _userId);
                                    }
                                }
                            }
                        }
                    }
                    _matchmakingSetting.onJoinPlayer.Invoke(_matchmakingModel.Gathering, _userId);
                    _matchmakingSetting.onUpdateJoinedPlayerIds.Invoke(_matchmakingModel.Gathering, _matchmakingModel.JoinedPlayerIds);
                    
                    if (result.Error is NotFoundException)
                    {
                        SetState(State.GatheringNotFound);
                    }
                    else
                    {
                        SetState(result.Error == null
                            ? State.Matchmaking
                            : State.Error);
                    }

                    if (_complete)
                    {
                        SetState(State.Complete);
                        
                        _matchmakingSetting.onMatchmakingComplete.Invoke(_matchmakingModel.Gathering, _matchmakingModel.JoinedPlayerIds);
                        
                        OnClose();
                        
                        yield break;
                    }
                }

                contextToken = result.Result.MatchmakingContextToken;
                
                yield return new WaitForSeconds(1);
            }
        }
        
        /// <summary>
        /// ギャザリング作成 キャンセルボタンをクリック
        /// </summary>
        public void OnClickToCancelCreateGathering()
        {
            SetState(State.MainMenu);
        }
        
        /// <summary>
        /// 既存のギャザリングへの参加 キャンセルボタンをクリック
        /// </summary>
        public void OnClickToCancelJoinGathering()
        {
            SetState(State.MainMenu);
        }
        
        /// <summary>
        /// マッチメイキングの中断　キャンセルボタンをクリック
        /// </summary>
        public void OnClickToCancelMatchmaking()
        {
            StartCoroutine(
                CancelMatchmaking()
            );
        }

        /// <summary>
        /// ギャザリングから離脱する
        /// </summary>
        /// <returns></returns>
        public IEnumerator CancelMatchmaking()
        {
            UIManager.Instance.AddLog("CancelMatchmaking");

            AsyncResult<EzCancelMatchmakingResult> result = null;
            yield return _matchmakingModel.CancelMatchmaking(
                r => { result = r; },
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                _matchmakingSetting.matchmakingNamespaceName,
                _matchmakingSetting.onMatchmakingCancel,
                _matchmakingSetting.onError
            );
        
            if (result.Error != null)
            {
                SetState(State.Error);
                yield break;
            }
            
            SetState(State.MainMenu);
        }
        
        /// <summary>
        /// エラー内容の確認ボタンをクリック
        /// </summary>
        public void OnClickToConfirmError()
        {
            SetState(State.MainMenu);
        }
    }
}