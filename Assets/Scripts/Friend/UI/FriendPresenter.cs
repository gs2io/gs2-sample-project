using System.Collections;
using System.Collections.Generic;
using Gs2.Core.Model;
using Gs2.Unity.Gs2Friend.Model;
using UnityEngine;
using UnityEngine.Assertions;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
#endif

namespace Gs2.Sample.Friend
{
    public class FriendPresenter : MonoBehaviour
    {
        [SerializeField] private FriendSetting _friendSetting;

        [SerializeField] private FriendModel _friendModel;
        
        [SerializeField] private FriendProfileEditView _friendProfileEditView;
        
        [SerializeField] private FriendListView _friendListView;
        [SerializeField] private FriendProfileView _friendProfileView;
        
        [SerializeField] private PlayerInfoView _playerInfoView;
        [SerializeField] private PublicProfileView _publicProfileView;
        
        [SerializeField] private FriendSendRequestsView _friendSendRequestsView;
        [SerializeField] private FriendReceiveRequestsView _friendReceiveRequestsView;
        
        [SerializeField] private FriendBlackListView _friendBlackListView;
        
        [SerializeField] private FriendFollowListView _friendFollowListView;
        [SerializeField] private FollowerProfileView _followerProfileView;
        
        /// <summary>
        /// 通知の種別
        /// </summary>
        private string _issuer;
        
        // Start is called before the first frame update
        void Start()
        {
            Assert.IsNotNull(_friendModel);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <returns></returns>
        public void Initialize()
        {
            GameManager.Instance.Domain.Connection.WebSocketSession.OnNotificationMessage += PushNotificationHandler;
        }
        
        public void Finish()
        {
            GameManager.Instance.Domain.Connection.WebSocketSession.OnNotificationMessage -= PushNotificationHandler;
        }

        /// <summary>
        /// 任意のタイミングで届く通知
        /// ※メインスレッド外
        /// </summary>
        public void PushNotificationHandler(NotificationMessage message)
        {
            Debug.Log("PushNotificationHandler :" + message.issuer);
            
            if (!message.issuer.StartsWith("Gs2Friend")) return;

            _issuer = message.issuer;

            if (message.issuer.EndsWith(":Post"))
            {

            }
        }
        
        public void OnClickProfile()
        {
#if GS2_ENABLE_UNITASK
            GetProfileAsync().Forget();
#else
            StartCoroutine(
                GetProfile()
            );
#endif
        }
        
        /// <summary>
        /// 自分のプロフィールを取得
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetProfile()
        {
            void OnGetProfile(
                EzProfile profile
            )
            {
                _friendSetting.onGetProfile.RemoveListener(OnGetProfile);

                _friendProfileEditView.SetPublicProfile(profile.PublicProfile);
                _friendProfileEditView.SetFollowProfile(profile.FollowerProfile);
                _friendProfileEditView.SetFriendProfile(profile.FriendProfile);
                
                _friendProfileEditView.OnOpenEvent();
            }
            
            _friendSetting.onGetProfile.AddListener(OnGetProfile);
            
            yield return _friendModel.GetProfile(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                _friendSetting.onGetProfile,
                _friendSetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        public async UniTask GetProfileAsync()
        {
            void OnGetProfile(
                EzProfile profile
            )
            {
                _friendSetting.onGetProfile.RemoveListener(OnGetProfile);

                _friendProfileEditView.SetPublicProfile(profile.PublicProfile);
                _friendProfileEditView.SetFollowProfile(profile.FollowerProfile);
                _friendProfileEditView.SetFriendProfile(profile.FriendProfile);
                
                _friendProfileEditView.OnOpenEvent();
            }
            
            _friendSetting.onGetProfile.AddListener(OnGetProfile);
            
            await _friendModel.GetProfileAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                _friendSetting.onGetProfile,
                _friendSetting.onError
            );
        }
#endif
        
        public void OnClickUpdateProfile()
        {
#if GS2_ENABLE_UNITASK
            UpdateProfileAsync().Forget();
#else
            StartCoroutine(
                UpdateProfile()
            );
#endif
            _friendProfileEditView.OnCloseEvent();
        }
        
        public IEnumerator UpdateProfile()
        {
            void OnUpdateProfile(
                EzProfile profile
            )
            {
                _friendProfileView.OnCloseEvent();
            }
            
            _friendSetting.onGetProfile.AddListener(OnUpdateProfile);
            
            yield return _friendModel.UpdateProfile(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                _friendProfileEditView.publicProfile.text,
                _friendProfileEditView.followerProfile.text,
                _friendProfileEditView.friendProfile.text,
                _friendSetting.onUpdateProfile,
                _friendSetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        public async UniTask UpdateProfileAsync()
        {
            void OnUpdateProfile(
                EzProfile profile
            )
            {
                _friendProfileView.OnCloseEvent();
            }
            
            _friendSetting.onGetProfile.AddListener(OnUpdateProfile);
            
            await _friendModel.UpdateProfileAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                _friendProfileEditView.publicProfile.text,
                _friendProfileEditView.followerProfile.text,
                _friendProfileEditView.friendProfile.text,
                _friendSetting.onUpdateProfile,
                _friendSetting.onError
            );
        }
#endif
        
        public void OnClickSendRequest(string targetUserId)
        {
#if GS2_ENABLE_UNITASK
            SendRequestAsync(targetUserId).Forget();
#else
            StartCoroutine(
                SendRequest(targetUserId)
            );
#endif
        }
        
        /// <summary>
        /// フレンドリクエストを送信
        /// </summary>
        public IEnumerator SendRequest(string targetUserId)
        {
            void OnSendRequest(
                EzFriendRequest request
            )
            {
                _friendSetting.onSendRequest.RemoveListener(OnSendRequest);
                
                UIManager.Instance.OpenDialog1("Notice", "FriendRequestSend");
            }
            
            _friendSetting.onSendRequest.AddListener(OnSendRequest);
            
            yield return _friendModel.SendRequest(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                targetUserId,
                _friendSetting.onSendRequest,
                _friendSetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        public async UniTask SendRequestAsync(string targetUserId)
        {
            void OnSendRequest(
                EzFriendRequest request
            )
            {
                _friendSetting.onSendRequest.RemoveListener(OnSendRequest);
                
                UIManager.Instance.OpenDialog1("Notice", "FriendRequestSend");
            }
            
            _friendSetting.onSendRequest.AddListener(OnSendRequest);
            
            await _friendModel.SendRequestAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                targetUserId,
                _friendSetting.onSendRequest,
                _friendSetting.onError
            );
        }
#endif
        
        public void OnClickAccept(string targetUserId)
        {
#if GS2_ENABLE_UNITASK
            AcceptAsync(targetUserId).Forget();
#else
            StartCoroutine(
                Accept(targetUserId)
            );
#endif
        }
        
        private IEnumerator Accept(string targetUserId)
        {
            void OnAccept(
                EzFriendRequest request
            )
            {
                _friendSetting.onAccept.RemoveListener(OnAccept);
                
                UIManager.Instance.OpenDialog1("Notice", "FriendRequestAccept");

                OnOpenReceiveRequests();
            }
            
            _friendSetting.onAccept.AddListener(OnAccept);
            
            yield return _friendModel.Accept(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                targetUserId,
                _friendSetting.onAccept,
                _friendSetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        private async UniTask AcceptAsync(string targetUserId)
        {
            void OnAccept(
                EzFriendRequest request
            )
            {
                _friendSetting.onAccept.RemoveListener(OnAccept);
                
                UIManager.Instance.OpenDialog1("Notice", "FriendRequestAccept");

                OnOpenReceiveRequests();
            }
            
            _friendSetting.onAccept.AddListener(OnAccept);
            
            await _friendModel.AcceptAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                targetUserId,
                _friendSetting.onAccept,
                _friendSetting.onError
            );
        }
#endif
        
        public void OnClickReject(string targetUserId)
        {
#if GS2_ENABLE_UNITASK
            RejectAsync(targetUserId).Forget();
#else
            StartCoroutine(
                Reject(targetUserId)
            );
#endif
        }
        
        private IEnumerator Reject(string targetUserId)
        {
            void OnReject(
                EzFriendRequest request
            )
            {
                _friendSetting.onReject.RemoveListener(OnReject);
                
                UIManager.Instance.OpenDialog1("Notice", "FriendRequestReject");

                OnOpenReceiveRequests();
            }
            
            _friendSetting.onReject.AddListener(OnReject);
            
            yield return _friendModel.Reject(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                targetUserId,
                _friendSetting.onReject,
                _friendSetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        private async UniTask RejectAsync(string targetUserId)
        {
            void OnReject(
                EzFriendRequest request
            )
            {
                _friendSetting.onReject.RemoveListener(OnReject);
                
                UIManager.Instance.OpenDialog1("Notice", "FriendRequestReject");

                OnOpenReceiveRequests();
            }
            
            _friendSetting.onReject.AddListener(OnReject);
            
            await _friendModel.RejectAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                targetUserId,
                _friendSetting.onReject,
                _friendSetting.onError
            );
        }
#endif
        
        public void OnOpenFriendList()
        {
#if GS2_ENABLE_UNITASK
            OpenFriendListAsync().Forget();
#else
            StartCoroutine(
                OpenFriendList()
            );
#endif
        }
        
        private IEnumerator OpenFriendList()
        {
            yield return _friendModel.DescribeFriends(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                _friendSetting.onDescribeFriends,
                _friendSetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        private async UniTask OpenFriendListAsync()
        {
            await _friendModel.DescribeFriendsAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                _friendSetting.onDescribeFriends,
                _friendSetting.onError
            );
        }
#endif
        
        public void OnUpdateFriendList(List<EzFriendUser> friends)
        {
            _friendListView.friendNamePrefab.SetActive(false);

            if (_friendListView.friendListContent != null)
            {
                foreach (Transform child in _friendListView.friendListContent.transform)
                {
                    if (child != null && child.gameObject != _friendListView.friendNamePrefab)
                    {
                        Destroy(child.gameObject);
                    }
                }

                foreach (var friend in friends)
                {
                    var friendNameObject = Instantiate(_friendListView.friendNamePrefab,
                        _friendListView.friendListContent.transform);
                    var friendNameView = friendNameObject.GetComponent<FriendNameView>();
                    friendNameView.userId.SetText(friend.UserId);
                    friendNameView.profile.onClick.AddListener(
                        () => { OnOpenFriendProfile(friend.UserId); }
                    );
                    friendNameView.delete.onClick.AddListener(
                        () => { OnClickDeleteFriend(friend.UserId); }
                    );
                    friendNameView.gameObject.SetActive(true);
                }
            }
            
            _friendListView.OnOpenEvent();
        }

        public void OnOpenSendRequests()
        {
#if GS2_ENABLE_UNITASK
            OpenSendRequestsAsync().Forget();
#else
            StartCoroutine(
                OpenSendRequests()
            );
#endif
        }
        
        /// <summary>
        /// 
        /// </summary>
        private IEnumerator OpenSendRequests()
        {
            yield return _friendModel.DescribeSendRequests(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                _friendSetting.onDescribeSendRequests,
                _friendSetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        private async UniTask OpenSendRequestsAsync()
        {
            await _friendModel.DescribeSendRequestsAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                _friendSetting.onDescribeSendRequests,
                _friendSetting.onError
            );
        }
#endif
        
        public void OnUpdateSendRequests(List<EzFriendRequest> friends)
        {
            _friendSendRequestsView.friendNamePrefab.SetActive(false);

            if (_friendSendRequestsView.friendListContent != null)
            {
                foreach (Transform child in _friendSendRequestsView.friendListContent.transform)
                {
                    if (child != null && child.gameObject != _friendSendRequestsView.friendNamePrefab)
                    {
                        Destroy(child.gameObject);
                    }
                }

                foreach (var friend in friends)
                {
                    var playerNameObject = Instantiate<GameObject>(_friendSendRequestsView.friendNamePrefab,
                        _friendSendRequestsView.friendListContent.transform);
                    var friendNameView = playerNameObject.GetComponent<FriendNameView>();
                    friendNameView.userId.SetText(friend.TargetUserId);
                    friendNameView.profile.onClick.AddListener(
                        () => { OnOpenPublicProfile(friend.TargetUserId); }
                    );
                    friendNameView.delete.onClick.AddListener(
                        () => { OnClickDeleteRequest(friend.TargetUserId); }
                    );
                    friendNameView.gameObject.SetActive(true);
                }
            }
            
            _friendSendRequestsView.OnOpenEvent();
        }
        
        public void OnOpenReceiveRequests()
        {
#if GS2_ENABLE_UNITASK
            OpenReceiveRequestsAsync().Forget();
#else
            StartCoroutine(
                OpenReceiveRequests()
            );
#endif
        }
        
        private IEnumerator OpenReceiveRequests()
        {
            yield return _friendModel.DescribeReceiveRequests(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                _friendSetting.onDescribeReceiveRequests,
                _friendSetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        private async UniTask OpenReceiveRequestsAsync()
        {
            await _friendModel.DescribeReceiveRequestsAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                _friendSetting.onDescribeReceiveRequests,
                _friendSetting.onError
            );
        }
#endif
        
        public void OnUpdateReceiveRequests(List<EzFriendRequest> friends)
        {
            _friendReceiveRequestsView.friendNamePrefab.SetActive(false);

            if (_friendListView.friendListContent != null)
            {
                foreach (Transform child in _friendReceiveRequestsView.friendListContent.transform)
                {
                    if (child != null && child.gameObject != _friendReceiveRequestsView.friendNamePrefab)
                    {
                        Destroy(child.gameObject);
                    }
                }

                foreach (var friend in friends)
                {
                    var playerNameObject = Instantiate<GameObject>(_friendReceiveRequestsView.friendNamePrefab,
                        _friendReceiveRequestsView.friendListContent.transform);
                    var friendRequestView = playerNameObject.GetComponent<FriendRequestView>();
                    friendRequestView.userId.SetText(friend.UserId);
                    friendRequestView.profile.onClick.AddListener(
                        () => { OnOpenPublicProfile(friend.UserId); }
                    );
                    friendRequestView.accept.onClick.AddListener(
                        () => { OnClickAccept(friend.UserId); }
                    );
                    friendRequestView.reject.onClick.AddListener(
                        () => { OnClickReject(friend.UserId); }
                    );
                    friendRequestView.gameObject.SetActive(true);
                }
            }
            
            _friendReceiveRequestsView.OnOpenEvent();
        }
        
        public void OnOpenFriendProfile(string targetUserId)
        {
#if GS2_ENABLE_UNITASK
            OpenFriendProfileAsync(targetUserId).Forget();
#else
            StartCoroutine(
                OpenFriendProfile(targetUserId)
            );
#endif
        }
        
        private IEnumerator OpenFriendProfile(string targetUserId)
        {
            void GetFriend(
                EzFriendUser profile
            )
            {
                _friendSetting.onGetFriend.RemoveListener(GetFriend);

                _friendProfileView.SetUserId(profile.UserId);
                _friendProfileView.SetPublicProfile(profile.PublicProfile);
                _friendProfileView.SetFriendProfile(profile.FriendProfile);
                
                _friendProfileView.OnOpenEvent();
            }
            
            _friendSetting.onGetFriend.AddListener(GetFriend);
            
            yield return _friendModel.GetFriend(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                targetUserId,
                _friendSetting.onGetFriend,
                _friendSetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        private async UniTask OpenFriendProfileAsync(string targetUserId)
        {
            void GetFriend(
                EzFriendUser profile
            )
            {
                _friendSetting.onGetFriend.RemoveListener(GetFriend);

                _friendProfileView.SetUserId(profile.UserId);
                _friendProfileView.SetPublicProfile(profile.PublicProfile);
                _friendProfileView.SetFriendProfile(profile.FriendProfile);
                
                _friendProfileView.OnOpenEvent();
            }
            
            _friendSetting.onGetFriend.AddListener(GetFriend);
            
            await _friendModel.GetFriendAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                targetUserId,
                _friendSetting.onGetFriend,
                _friendSetting.onError
            );
        }
#endif
        
        public void OnClickDeleteFriend(string targetUserId)
        {
#if GS2_ENABLE_UNITASK
            DeleteFriendAsync(targetUserId).Forget();
#else
            StartCoroutine(
                DeleteFriend(targetUserId)
            );
#endif
        }

        private IEnumerator DeleteFriend(string targetUserId)
        {
            void OnDeleteFriend(
                EzFriendUser profile
            )
            {
                _friendSetting.onDeleteFriend.RemoveListener(OnDeleteFriend);
                
                UIManager.Instance.OpenDialog1("Notice", "FriendRemove");

                OnOpenFriendList();
            }
            
            _friendSetting.onDeleteFriend.AddListener(OnDeleteFriend);
            
            yield return _friendModel.DeleteFriend(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                targetUserId,
                _friendSetting.onDeleteFriend,
                _friendSetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        private async UniTask DeleteFriendAsync(string targetUserId)
        {
            void OnDeleteFriend(
                EzFriendUser profile
            )
            {
                _friendSetting.onDeleteFriend.RemoveListener(OnDeleteFriend);
                
                UIManager.Instance.OpenDialog1("Notice", "FriendRemove");

                OnOpenFriendList();
            }
            
            _friendSetting.onDeleteFriend.AddListener(OnDeleteFriend);
            
            await _friendModel.DeleteFriendAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                targetUserId,
                _friendSetting.onDeleteFriend,
                _friendSetting.onError
            );
        }
#endif
        
        public void OnOpenPlayerInfo(string userId)
        {
            _playerInfoView.SetUserId(userId);
            
            _playerInfoView.profile.onClick.RemoveAllListeners();
            _playerInfoView.follow.onClick.RemoveAllListeners();
            _playerInfoView.friendRequest.onClick.RemoveAllListeners();
            _playerInfoView.blackList.onClick.RemoveAllListeners();
            
            _playerInfoView.profile.onClick.AddListener(
                () => { OnOpenPublicProfile(userId); }
            );
            _playerInfoView.follow.onClick.AddListener(
                () => { OnClickFollow(userId); }
            );
            _playerInfoView.friendRequest.onClick.AddListener(
                () => { OnClickSendRequest(userId); }
            );
            _playerInfoView.blackList.onClick.AddListener(
                () => { OnClickRegisterBlackList(userId); }
            );
            _playerInfoView.OnOpenEvent();
        }
        
        public void OnOpenPublicProfile(string targetUserId)
        {
#if GS2_ENABLE_UNITASK
            OpenPublicProfileAsync(targetUserId).Forget();
#else
            StartCoroutine(
                OpenPublicProfile(targetUserId)
            );
#endif
        }
        
        private IEnumerator OpenPublicProfile(string userId)
        {
            void OnGetPublicProfile(
                EzPublicProfile profile
            )
            {
                _friendSetting.onGetPublicProfile.RemoveListener(OnGetPublicProfile);

                _publicProfileView.SetUserId(profile.UserId);
                _publicProfileView.SetPublicProfile(profile.PublicProfile);
                
                _publicProfileView.OnOpenEvent();
            }
            
            _friendSetting.onGetPublicProfile.AddListener(OnGetPublicProfile);
            
            yield return _friendModel.GetPublicProfile(
                GameManager.Instance.Domain,
                _friendSetting.friendNamespaceName,
                userId,
                _friendSetting.onGetPublicProfile,
                _friendSetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        private async UniTask OpenPublicProfileAsync(string userId)
        {
            void OnGetPublicProfile(
                EzPublicProfile profile
            )
            {
                _friendSetting.onGetPublicProfile.RemoveListener(OnGetPublicProfile);

                _publicProfileView.SetUserId(profile.UserId);
                _publicProfileView.SetPublicProfile(profile.PublicProfile);
                
                _publicProfileView.OnOpenEvent();
            }
            
            _friendSetting.onGetPublicProfile.AddListener(OnGetPublicProfile);
            
            await _friendModel.GetPublicProfile(
                GameManager.Instance.Domain,
                _friendSetting.friendNamespaceName,
                userId,
                _friendSetting.onGetPublicProfile,
                _friendSetting.onError
            );
        }
#endif
        
        public void OnClickDeleteRequest(string targetUserId)
        {
#if GS2_ENABLE_UNITASK
            DeleteRequestAsync(targetUserId).Forget();
#else
            StartCoroutine(
                DeleteRequest(targetUserId)
            );
#endif
        }
        
        private IEnumerator DeleteRequest(string userId)
        {
            void OnDeleteRequest(
                EzFriendRequest request
            )
            {
                _friendSetting.onDeleteRequest.RemoveListener(OnDeleteRequest);
                
                UIManager.Instance.OpenDialog1("Notice", "FriendRequestDelete");

                OnOpenSendRequests();
            }
            
            _friendSetting.onDeleteRequest.AddListener(OnDeleteRequest);
            
            yield return _friendModel.DeleteRequest(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                userId,
                _friendSetting.onDeleteRequest,
                _friendSetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        private async UniTask DeleteRequestAsync(string userId)
        {
            void OnDeleteRequest(
                EzFriendRequest request
            )
            {
                _friendSetting.onDeleteRequest.RemoveListener(OnDeleteRequest);
                
                UIManager.Instance.OpenDialog1("Notice", "FriendRequestDelete");

                OnOpenSendRequests();
            }
            
            _friendSetting.onDeleteRequest.AddListener(OnDeleteRequest);
            
            await _friendModel.DeleteRequestAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                userId,
                _friendSetting.onDeleteRequest,
                _friendSetting.onError
            );
        }
#endif
        
        public void OnOpenBlackList()
        {
#if GS2_ENABLE_UNITASK
            OpenBlackListAsync().Forget();
#else
            StartCoroutine(
                OpenBlackList()
            );
#endif
        }
        
        private IEnumerator OpenBlackList()
        {
            yield return _friendModel.GetBlackList(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                _friendSetting.onGetBlackList,
                _friendSetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        private async UniTask OpenBlackListAsync()
        {
            await _friendModel.GetBlackListAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                _friendSetting.onGetBlackList,
                _friendSetting.onError
            );
        }
#endif
        
        public void OnUpdateBlackList(List<string> blackList)
        {
            _friendBlackListView.friendNamePrefab.SetActive(false);

            if (_friendBlackListView.friendListContent != null)
            {
                foreach (Transform child in _friendBlackListView.friendListContent.transform)
                {
                    if (child != null && child.gameObject != _friendBlackListView.friendNamePrefab)
                    {
                        Destroy(child.gameObject);
                    }
                }

                foreach (var userId in blackList)
                {
                    var friendNameObject = Instantiate(_friendBlackListView.friendNamePrefab,
                        _friendBlackListView.friendListContent.transform);
                    var friendNameView = friendNameObject.GetComponent<FriendNameView>();
                    friendNameView.userId.SetText(userId);
                    friendNameView.profile.onClick.AddListener(
                        () => { OnOpenPublicProfile(userId); }
                    );
                    friendNameView.delete.onClick.AddListener(
                        () => { OnClickUnregisterBlackList(userId); }
                    );
                    friendNameView.gameObject.SetActive(true);
                }
            }
            
            _friendBlackListView.OnOpenEvent();
        }
        
        public void OnClickUnregisterBlackList(string targetUserId)
        {
#if GS2_ENABLE_UNITASK
            UnregisterBlackListAsync(targetUserId).Forget();
#else
            StartCoroutine(
                UnregisterBlackList(targetUserId)
            );
#endif
        }
        
        private IEnumerator UnregisterBlackList(string targetUserId)
        {
            void OnUnregisterBlackList(
                EzBlackList request
            )
            {
                _friendSetting.onUnregisterBlackList.RemoveListener(OnUnregisterBlackList);
                
                UIManager.Instance.OpenDialog1("Notice", "FriendRemoveBlacklist");

                OnOpenBlackList();
            }
            
            _friendSetting.onUnregisterBlackList.AddListener(OnUnregisterBlackList);
            
            yield return _friendModel.UnregisterBlackList(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                targetUserId,
                _friendSetting.onUnregisterBlackList,
                _friendSetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        private async UniTask UnregisterBlackListAsync(string targetUserId)
        {
            void OnUnregisterBlackList(
                EzBlackList request
            )
            {
                _friendSetting.onUnregisterBlackList.RemoveListener(OnUnregisterBlackList);
                
                UIManager.Instance.OpenDialog1("Notice", "FriendRemoveBlacklist");

                OnOpenBlackList();
            }
            
            _friendSetting.onUnregisterBlackList.AddListener(OnUnregisterBlackList);
            
            await _friendModel.UnregisterBlackListAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                targetUserId,
                _friendSetting.onUnregisterBlackList,
                _friendSetting.onError
            );
        }
#endif
        
        public void OnClickRegisterBlackList(string targetUserId)
        {
#if GS2_ENABLE_UNITASK
            RegisterBlackListAsync(targetUserId).Forget();
#else
            StartCoroutine(
                RegisterBlackList(targetUserId)
            );
#endif
        }
        
        private IEnumerator RegisterBlackList(string targetUserId)
        {
            void OnRegisterBlackList(
                EzBlackList request
            )
            {
                _friendSetting.onUnregisterBlackList.RemoveListener(OnRegisterBlackList);
                
                UIManager.Instance.OpenDialog1("Notice", "FriendAddBlacklist");
            }
            
            _friendSetting.onUnregisterBlackList.AddListener(OnRegisterBlackList);
            
            yield return _friendModel.RegisterBlackList(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                targetUserId,
                _friendSetting.onRegisterBlackList,
                _friendSetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        private async UniTask RegisterBlackListAsync(string targetUserId)
        {
            void OnRegisterBlackList(
                EzBlackList request
            )
            {
                _friendSetting.onUnregisterBlackList.RemoveListener(OnRegisterBlackList);
                
                UIManager.Instance.OpenDialog1("Notice", "FriendAddBlacklist");
            }
            
            _friendSetting.onUnregisterBlackList.AddListener(OnRegisterBlackList);
            
            await _friendModel.RegisterBlackListAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                targetUserId,
                _friendSetting.onRegisterBlackList,
                _friendSetting.onError
            );
        }
#endif
        
        public void OnClickFollow(string targetUserId)
        {
#if GS2_ENABLE_UNITASK
            FollowAsync(targetUserId).Forget();
#else
            StartCoroutine(
                Follow(targetUserId)
            );
#endif
        }
        
        public IEnumerator Follow(string targetUserId)
        {
            void OnFollow(
                EzFollowUser user
            )
            {
                _friendSetting.onFollow.RemoveListener(OnFollow);
                
                UIManager.Instance.OpenDialog1("Notice", "Follow");
            }
            
            _friendSetting.onFollow.AddListener(OnFollow);
            
            yield return _friendModel.Follow(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                targetUserId,
                _friendSetting.onFollow,
                _friendSetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        public async UniTask FollowAsync(string targetUserId)
        {
            void OnFollow(
                EzFollowUser user
            )
            {
                _friendSetting.onFollow.RemoveListener(OnFollow);
                
                UIManager.Instance.OpenDialog1("Notice", "Follow");
            }
            
            _friendSetting.onFollow.AddListener(OnFollow);
            
            await _friendModel.FollowAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                targetUserId,
                _friendSetting.onFollow,
                _friendSetting.onError
            );
        }
#endif
        
        public void OnClickUnfollow(string targetUserId)
        {
#if GS2_ENABLE_UNITASK
            UnfollowAsync(targetUserId).Forget();
#else
            StartCoroutine(
                Unfollow(targetUserId)
            );
#endif
        }
        
        public IEnumerator Unfollow(string targetUserId)
        {
            void OnUnfollow()
            {
                _friendSetting.onUnfollow.RemoveListener(OnUnfollow);
                
                UIManager.Instance.OpenDialog1("Notice", "Unfollow");
                
                OnOpenFollowList();
            }
            
            _friendSetting.onUnfollow.AddListener(OnUnfollow);
            
            yield return _friendModel.Unfollow(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                targetUserId,
                _friendSetting.onUnfollow,
                _friendSetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        public async UniTask UnfollowAsync(string targetUserId)
        {
            void OnUnfollow()
            {
                _friendSetting.onUnfollow.RemoveListener(OnUnfollow);
                
                UIManager.Instance.OpenDialog1("Notice", "Unfollow");
                
                OnOpenFollowList();
            }
            
            _friendSetting.onUnfollow.AddListener(OnUnfollow);
            
            await _friendModel.UnfollowAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                targetUserId,
                _friendSetting.onUnfollow,
                _friendSetting.onError
            );
        }
#endif
        
        public void OnOpenFollowList()
        {
#if GS2_ENABLE_UNITASK
            OpenFollowListAsync().Forget();
#else
            StartCoroutine(
                OpenFollowList()
            );
#endif
        }
        
        private IEnumerator OpenFollowList()
        {
            yield return _friendModel.DescribeFollowUsers(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                _friendSetting.onDescribeFollowUsers,
                _friendSetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        private async UniTask OpenFollowListAsync()
        {
            await _friendModel.DescribeFollowUsersAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _friendSetting.friendNamespaceName,
                _friendSetting.onDescribeFollowUsers,
                _friendSetting.onError
            );
        }
#endif
        
        public void OnUpdateFollowList(List<EzFollowUser> followList)
        {
            _friendFollowListView.friendNamePrefab.SetActive(false);

            if (_friendFollowListView.friendListContent != null)
            {
                foreach (Transform child in _friendFollowListView.friendListContent.transform)
                {
                    if (child != null && child.gameObject != _friendFollowListView.friendNamePrefab)
                    {
                        Destroy(child.gameObject);
                    }
                }

                foreach (var user in followList)
                {
                    var friendNameObject = Instantiate(_friendFollowListView.friendNamePrefab,
                        _friendFollowListView.friendListContent.transform);
                    var friendNameView = friendNameObject.GetComponent<FriendNameView>();
                    friendNameView.userId.SetText(user.UserId);
                    friendNameView.profile.onClick.AddListener(
                        () => { OnOpenFollowerProfile(user.UserId); }
                    );
                    friendNameView.delete.onClick.AddListener(
                        () => { OnClickUnfollow(user.UserId); }
                    );
                    friendNameView.gameObject.SetActive(true);
                }
            }
            
            _friendFollowListView.OnOpenEvent();
        }

        private void OnOpenFollowerProfile(string userId)
        {
            if (_friendModel.FollowUsers != null)
            {
                var followUser = _friendModel.FollowUsers.Find(f => f.UserId == userId);
                if (followUser != null)
                {
                    _followerProfileView.SetUserId(followUser.UserId);
                    _followerProfileView.SetPublicProfile(followUser.PublicProfile);
                    _followerProfileView.SetFollowerProfile(followUser.FollowerProfile);

                    _followerProfileView.OnOpenEvent();
                }
            }
        }
    }
}