using System;
using System.Collections;
using Gs2.Core.Exception;
using Gs2.Core.Model;
using Gs2.Gs2Matchmaking.Model;
using Gs2.Util.LitJson;
using UnityEngine;
using UnityEngine.Assertions;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
#endif

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
        /// ギャザリングに新しい参加者が参加したときの通知が届いたか
        /// </summary>
        private bool _hasReceivedJoinNotification;
        
        /// <summary>
        /// 参加したユーザーのUserId
        /// </summary>
        private string _joinUserId;
        
        /// <summary>
        /// ギャザリングから参加者が離脱したときの通知が届いたか
        /// </summary>
        private bool _hasReceivedLeaveNotification;
        
        /// <summary>
        /// 離脱したユーザーのUserId
        /// </summary>
        private string _leaveUserId;
        
        /// <summary>
        /// ギャザリングが成立したときの通知が届いたか
        /// </summary>
        private bool _hasReceivedCompleteNotification;
        
        /// <summary>
        /// マッチメイキングが完了したギャザリングのgatheringName
        /// </summary>
        private string _gatheringName;
        
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
            
            _hasReceivedJoinNotification = false;
            _hasReceivedLeaveNotification = false;
            _hasReceivedCompleteNotification = false;
            
            _matchmakingSetting.onUpdateJoinedPlayerIds.AddListener(
                (gathering, joinedPlayerIds) =>
                {
                    foreach (var joinedPlayerId in joinedPlayerIds)
                    {
                        Debug.Log("onUpdateJoinedPlayerIds : " + joinedPlayerId);
                    }

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
                            var playerNameObject = Instantiate<GameObject>(_matchmakingView.displayPlayerNamePrefab,
                                _matchmakingView.joinedPlayersContent.transform);
                            var playerNameView = playerNameObject.GetComponent<PlayerNameView>();
                            playerNameView.userId.SetText(joinedPlayerId);
                            playerNameView.userId.enabled = true;

                            playerNameView.gameObject.SetActive(true);
                        }
                    }
                }
            );
        }

        public void Initialize()
        {
            GameManager.Instance.Domain.Matchmaking.OnJoinNotification += JoinNotificationHandler;
            GameManager.Instance.Domain.Matchmaking.OnLeaveNotification += LeaveNotificationHandler;
            GameManager.Instance.Domain.Matchmaking.OnCompleteNotification += CompleteNotificationHandler;
            
        }
        
        public void Finish()
        {
            GameManager.Instance.Domain.Matchmaking.OnJoinNotification -= JoinNotificationHandler;
            GameManager.Instance.Domain.Matchmaking.OnLeaveNotification -= LeaveNotificationHandler;
            GameManager.Instance.Domain.Matchmaking.OnCompleteNotification -= CompleteNotificationHandler;
        }
        
        /// <summary>
        /// 任意のタイミングで届く通知
        /// ※メインスレッド外
        /// Notifications delivered at any given time
        /// *Outside the main thread
        /// </summary>
        /// <param name="notification"></param>
        public void JoinNotificationHandler(JoinNotification notification)
        {
            Debug.Log("JoinNotificationHandler : " + notification.JoinUserId);
            
            _matchmakingModel.JoinedPlayerIds.Add(notification.JoinUserId);
            _joinUserId = notification.JoinUserId;
            _hasReceivedJoinNotification = true;
        }

        /// <summary>
        /// 任意のタイミングで届く通知
        /// ※メインスレッド外
        /// Notifications delivered at any given time
        /// *Outside the main thread
        /// </summary>
        /// <param name="notification"></param>
        public void LeaveNotificationHandler(LeaveNotification notification)
        {
            Debug.Log("LeaveNotificationHandler : " + notification.LeaveUserId);
            
            _matchmakingModel.JoinedPlayerIds.Remove(notification.LeaveUserId);
            _leaveUserId = notification.LeaveUserId;
            _hasReceivedJoinNotification = true;
        }
        
        /// <summary>
        /// 任意のタイミングで届く通知
        /// ※メインスレッド外
        /// Notifications delivered at any given time
        /// *Outside the main thread
        /// </summary>
        /// <param name="notification"></param>
        public void CompleteNotificationHandler(CompleteNotification notification)
        {
            Debug.Log("CompleteNotificationHandler : " + notification.GatheringName);
            
            _gatheringName = notification.GatheringName;
            _hasReceivedCompleteNotification = true;
        }
        
        private void Update()
        {
            if (_hasReceivedJoinNotification)
            {
                if (_matchmakingModel.Gathering != null)
                {
                    _matchmakingSetting.onJoinPlayer.Invoke(_matchmakingModel.Gathering, _joinUserId);
                    _matchmakingSetting.onUpdateJoinedPlayerIds.Invoke(_matchmakingModel.Gathering, _matchmakingModel.JoinedPlayerIds);

                    _joinUserId = String.Empty;
                    _hasReceivedJoinNotification = false;
                }
            }

            if (_hasReceivedLeaveNotification)
            {
                if (_matchmakingModel.Gathering != null)
                {
                    _matchmakingSetting.onLeavePlayer.Invoke(_matchmakingModel.Gathering, _leaveUserId);
                    _matchmakingSetting.onUpdateJoinedPlayerIds.Invoke(_matchmakingModel.Gathering, _matchmakingModel.JoinedPlayerIds);
                    
                    _leaveUserId = String.Empty;
                    _hasReceivedLeaveNotification = false;
                }
            }

            if (_hasReceivedCompleteNotification)
            {
                if (_matchmakingModel.Gathering != null)
                {
                    // Joinと同時にマッチメイキングが成立する場合
                    // DoMatchmaking の応答より先にマッチメイキング完了通知が届くことがある
                    SetState(State.Complete);
                    _matchmakingSetting.onMatchmakingComplete.Invoke(_gatheringName);
                        
                    _gatheringName = String.Empty;
                    _hasReceivedCompleteNotification = false;
                }
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
            SetState(State.CreateGatheringMenu);
        }

        /// <summary>
        /// 募集人数を確定してマッチメイキングを開始するボタンをクリック
        /// </summary>
        public void OnClickToSubmitCapacity()
        {
            _matchmakingModel.Capacity = int.Parse(_createGatheringView.capacityInputField.text);
            SetState(State.CreateGathering);
#if GS2_ENABLE_UNITASK
            SimpleMatchmakingCreateGatheringTaskAsync().Forget();
#else
            StartCoroutine(
                SimpleMatchmakingCreateGatheringTask()
            );
#endif
        }

        public IEnumerator SimpleMatchmakingCreateGatheringTask()
        {
            yield return _matchmakingModel.CreateGathering(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _matchmakingSetting.matchmakingNamespaceName,
                _matchmakingSetting.onUpdateJoinedPlayerIds,
                _matchmakingSetting.onError
            );
            
            if (_matchmakingModel.Gathering == null)
            {
                SetState(State.CreateGatheringFailed);
                yield break;
            }
            
            SetState(State.Matchmaking);
        }
#if GS2_ENABLE_UNITASK
        public async UniTask SimpleMatchmakingCreateGatheringTaskAsync()
        {
            await _matchmakingModel.CreateGatheringAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _matchmakingSetting.matchmakingNamespaceName,
                _matchmakingSetting.onUpdateJoinedPlayerIds,
                _matchmakingSetting.onError
            );
            
            if (_matchmakingModel.Gathering == null)
            {
                SetState(State.CreateGatheringFailed);
                return;
            }
            
            SetState(State.Matchmaking);
        }
#endif

        /// <summary>
        /// 既存のギャザリングへの参加ボタンをクリック
        /// </summary>
        public void OnClickToJoinGathering()
        {
            SetState(State.JoinGathering);
#if GS2_ENABLE_UNITASK
            SimpleMatchmakingJoinGatheringAsync().Forget();
#else
            StartCoroutine(
                SimpleMatchmakingJoinGathering()
            );
#endif
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
                yield return _matchmakingModel.JoinGathering(
                    GameManager.Instance.Domain,
                    GameManager.Instance.Session,
                    _matchmakingSetting.matchmakingNamespaceName,
                    _matchmakingSetting.onError
                );
                
                if (_matchmakingModel.Error != null)
                {
                    SetState(State.Error);
                    yield break;
                }

                if (_matchmakingModel.ResultGatherings.Count > 0 && _matchmakingModel.ResultGatherings[0] != null)
                {
                    _matchmakingModel.Gathering = _matchmakingModel.ResultGatherings[0];
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
                                    }
                                }
                            }
                        }
                    }
                    _matchmakingSetting.onUpdateJoinedPlayerIds.Invoke(_matchmakingModel.Gathering,
                        _matchmakingModel.JoinedPlayerIds);

                    if (_matchmakingModel.Error is NotFoundException)
                    {
                        SetState(State.GatheringNotFound);
                    }
                    else
                    {
                        if (_matchmakingModel.Error != null)
                        {
                            SetState(State.Error);
                        }
                        else if (matchmakingState != State.Complete)
                        {
                            SetState(State.Matchmaking);
                        }
                    }
                }

                yield return new WaitForSeconds(1);
            }
        }
#if GS2_ENABLE_UNITASK
        public async UniTask SimpleMatchmakingJoinGatheringAsync(
        )
        {
            UIManager.Instance.AddLog("SimpleMatchmakingJoinGatheringAsync");
            
            while (matchmakingState == State.JoinGathering)
            {
                await _matchmakingModel.JoinGatheringAsync(
                    GameManager.Instance.Domain,
                    GameManager.Instance.Session,
                    _matchmakingSetting.matchmakingNamespaceName,
                    _matchmakingSetting.onError
                );
                
                if (_matchmakingModel.Error != null)
                {
                    SetState(State.Error);
                    return;
                }

                if (_matchmakingModel.ResultGatherings.Count > 0 && _matchmakingModel.ResultGatherings[0] != null)
                {
                    _matchmakingModel.Gathering = _matchmakingModel.ResultGatherings[0];
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
                                        _matchmakingSetting.onJoinPlayer.Invoke(_matchmakingModel.Gathering, _joinUserId);
                                    }
                                }
                            }
                        }
                    }
                    _matchmakingSetting.onJoinPlayer.Invoke(_matchmakingModel.Gathering, _joinUserId);
                    _matchmakingSetting.onUpdateJoinedPlayerIds.Invoke(_matchmakingModel.Gathering,
                        _matchmakingModel.JoinedPlayerIds);

                    if (_matchmakingModel.Error is NotFoundException)
                    {
                        SetState(State.GatheringNotFound);
                    }
                    else
                    {
                        if (_matchmakingModel.Error != null)
                        {
                            SetState(State.Error);
                        }
                        else if (matchmakingState != State.Complete)
                        {
                            SetState(State.Matchmaking);
                        }
                    }
                }

                await UniTask.Delay(TimeSpan.FromSeconds(1));
            }
        }
#endif

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
#if GS2_ENABLE_UNITASK
            CancelMatchmakingAsync().Forget();
#else
            StartCoroutine(
                CancelMatchmaking()
            );
#endif
        }

        /// <summary>
        /// ギャザリングから離脱する
        /// </summary>
        /// <returns></returns>
        public IEnumerator CancelMatchmaking()
        {
            UIManager.Instance.AddLog("CancelMatchmaking");

            yield return _matchmakingModel.CancelMatchmaking(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _matchmakingSetting.matchmakingNamespaceName,
                _matchmakingSetting.onMatchmakingCancel,
                _matchmakingSetting.onError
            );
        
            if (_matchmakingModel.Error != null)
            {
                SetState(State.Error);
                yield break;
            }
            
            SetState(State.MainMenu);
        }
#if GS2_ENABLE_UNITASK
        public async UniTask CancelMatchmakingAsync()
        {
            UIManager.Instance.AddLog("CancelMatchmakingAsync");

            await _matchmakingModel.CancelMatchmakingAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _matchmakingSetting.matchmakingNamespaceName,
                _matchmakingSetting.onMatchmakingCancel,
                _matchmakingSetting.onError
            );
        
            if (_matchmakingModel.Error != null)
            {
                SetState(State.Error);
                return;
            }
            
            SetState(State.MainMenu);
        }
#endif
        
        /// <summary>
        /// エラー内容の確認ボタンをクリック
        /// </summary>
        public void OnClickToConfirmError()
        {
            SetState(State.MainMenu);
        }
    }
}