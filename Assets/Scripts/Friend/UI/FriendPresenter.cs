using System.Collections;
using System.Collections.Generic;
using Gs2.Core.Model;
using Gs2.Gs2Chat.Model;
using Gs2.Unity.Gs2Friend.Model;
using Gs2.Util.LitJson;
using UnityEngine;
using UnityEngine.Assertions;

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
            GameManager.Instance.Cllient.Profile.Gs2Session.OnNotificationMessage += PushNotificationHandler;
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
            
            if (!message.issuer.StartsWith("Gs2Friend")) return;

            _issuer = message.issuer;

            if (message.issuer.EndsWith(":Post"))
            {

            }
        }
        
        public void OnClickProfile()
        {
            StartCoroutine(
                GetProfile()
            );
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
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                _friendSetting.friendNamespaceName,
                _friendSetting.onGetProfile,
                _friendSetting.onError
            );
        }
        
        public void OnClickUpdateProfile()
        {
            StartCoroutine(
                UpdateProfile()
            );
            
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
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                _friendSetting.friendNamespaceName,
                _friendProfileEditView.publicProfile.text,
                _friendProfileEditView.followerProfile.text,
                _friendProfileEditView.friendProfile.text,
                _friendSetting.onUpdateProfile,
                _friendSetting.onError
            );
        }

        public void OnClickSendRequest(string targetUserId)
        {
            StartCoroutine(
                SendRequest(targetUserId)
            );
        }
        
        /// <summary>
        /// フレンドリクエストを送信
        /// </summary>
        /// <param name="targetUserId"></param>
        /// <returns></returns>
        public IEnumerator SendRequest(string targetUserId)
        {
            void OnSendRequest(
                EzFriendRequest request
            )
            {
                _friendSetting.onSendRequest.RemoveListener(OnSendRequest);
                
                UIManager.Instance.OpenDialog1("Notice", "フレンドリクエストを送信しました。");
            }
            
            _friendSetting.onSendRequest.AddListener(OnSendRequest);
            
            yield return _friendModel.SendRequest(
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                _friendSetting.friendNamespaceName,
                targetUserId,
                _friendSetting.onSendRequest,
                _friendSetting.onError
            );
        }
        
        public void OnClickAccept(string targetUserId)
        {
            StartCoroutine(
                Accept(targetUserId)
            );
        }
        
        private IEnumerator Accept(string targetUserId)
        {
            void OnAccept(
                EzFriendRequest request
            )
            {
                _friendSetting.onAccept.RemoveListener(OnAccept);
                
                UIManager.Instance.OpenDialog1("Notice", "フレンドリクエストを承認しました。");

                OnOpenReceiveRequests();
            }
            
            _friendSetting.onAccept.AddListener(OnAccept);
            
            yield return _friendModel.Accept(
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                _friendSetting.friendNamespaceName,
                targetUserId,
                _friendSetting.onAccept,
                _friendSetting.onError
            );
        }
        
        public void OnClickReject(string targetUserId)
        {
            StartCoroutine(
                Reject(targetUserId)
            );
        }
        
        private IEnumerator Reject(string targetUserId)
        {
            void OnReject(
                EzFriendRequest request
            )
            {
                _friendSetting.onReject.RemoveListener(OnReject);
                
                UIManager.Instance.OpenDialog1("Notice", "フレンドリクエストを拒否しました。");

                OnOpenReceiveRequests();
            }
            
            _friendSetting.onReject.AddListener(OnReject);
            
            yield return _friendModel.Reject(
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                _friendSetting.friendNamespaceName,
                targetUserId,
                _friendSetting.onReject,
                _friendSetting.onError
            );
        }
        
        public void OnOpenFriendList()
        {
            StartCoroutine(
                OpenFriendList()
            );
        }
        
        private IEnumerator OpenFriendList()
        {
            yield return _friendModel.DescribeFriends(
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                _friendSetting.friendNamespaceName,
                _friendSetting.onDescribeFriends,
                _friendSetting.onError
            );
        }
        
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
            StartCoroutine(
                OpenSendRequests()
            );
        }
        
        private IEnumerator OpenSendRequests()
        {
            yield return _friendModel.DescribeSendRequests(
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                _friendSetting.friendNamespaceName,
                _friendSetting.onDescribeSendRequests,
                _friendSetting.onError
            );
        }
        
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
            StartCoroutine(
                OpenReceiveRequests()
            );
        }
        
        private IEnumerator OpenReceiveRequests()
        {
            yield return _friendModel.DescribeReceiveRequests(
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                _friendSetting.friendNamespaceName,
                _friendSetting.onDescribeReceiveRequests,
                _friendSetting.onError
            );
        }
        
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
        
        public void OnOpenFriendProfile(string userId)
        {
            StartCoroutine(
                OpenFriendProfile(userId)
            );
        }
        
        private IEnumerator OpenFriendProfile(string userId)
        {
            void OnGetFriend(
                EzFriendUser profile
            )
            {
                _friendSetting.onGetFriend.RemoveListener(OnGetFriend);

                _friendProfileView.SetUserId(profile.UserId);
                _friendProfileView.SetPublicProfile(profile.PublicProfile);
                _friendProfileView.SetFriendProfile(profile.FriendProfile);
                
                _friendProfileView.OnOpenEvent();
            }
            
            _friendSetting.onGetFriend.AddListener(OnGetFriend);
            
            yield return _friendModel.GetFriend(
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                _friendSetting.friendNamespaceName,
                userId,
                _friendSetting.onGetFriend,
                _friendSetting.onError
            );
        }
        
        public void OnClickDeleteFriend(string userId)
        {
            StartCoroutine(
                DeleteFriend(userId)
            );
        }
        
        private IEnumerator DeleteFriend(string userId)
        {
            void OnDeleteFriend(
                EzFriendUser profile
            )
            {
                _friendSetting.onDeleteFriend.RemoveListener(OnDeleteFriend);
                
                UIManager.Instance.OpenDialog1("Notice", "フレンドから削除しました。");

                OnOpenFriendList();
            }
            
            _friendSetting.onDeleteFriend.AddListener(OnDeleteFriend);
            
            yield return _friendModel.DeleteFriend(
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                _friendSetting.friendNamespaceName,
                userId,
                _friendSetting.onDeleteFriend,
                _friendSetting.onError
            );
        }
        
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
        
        public void OnOpenPublicProfile(string userId)
        {
            StartCoroutine(
                OpenPublicProfile(userId)
            );
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
                GameManager.Instance.Cllient.Client,
                _friendSetting.friendNamespaceName,
                userId,
                _friendSetting.onGetPublicProfile,
                _friendSetting.onError
            );
        }
        
        public void OnClickDeleteRequest(string userId)
        {
            StartCoroutine(
                DeleteRequest(userId)
            );
        }
        
        private IEnumerator DeleteRequest(string userId)
        {
            void OnDeleteRequest(
                EzFriendRequest request
            )
            {
                _friendSetting.onDeleteRequest.RemoveListener(OnDeleteRequest);
                
                UIManager.Instance.OpenDialog1("Notice", "フレンドリクエストを削除しました。");

                OnOpenSendRequests();
            }
            
            _friendSetting.onDeleteRequest.AddListener(OnDeleteRequest);
            
            yield return _friendModel.DeleteRequest(
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                _friendSetting.friendNamespaceName,
                userId,
                _friendSetting.onDeleteRequest,
                _friendSetting.onError
            );
        }
        
        public void OnOpenBlackList()
        {
            StartCoroutine(
                OpenBlackList()
            );
        }
        
        private IEnumerator OpenBlackList()
        {
            yield return _friendModel.GetBlackList(
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                _friendSetting.friendNamespaceName,
                _friendSetting.onGetBlackList,
                _friendSetting.onError
            );
        }
        
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
        
        public void OnClickUnregisterBlackList(string userId)
        {
            StartCoroutine(
                UnregisterBlackList(userId)
            );
        }
        
        private IEnumerator UnregisterBlackList(string userId)
        {
            void OnUnregisterBlackList(
                EzBlackList request
            )
            {
                _friendSetting.onUnregisterBlackList.RemoveListener(OnUnregisterBlackList);
                
                UIManager.Instance.OpenDialog1("Notice", "ブラックリストから削除しました。");

                OnOpenBlackList();
            }
            
            _friendSetting.onUnregisterBlackList.AddListener(OnUnregisterBlackList);
            
            yield return _friendModel.UnregisterBlackList(
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                _friendSetting.friendNamespaceName,
                userId,
                _friendSetting.onUnregisterBlackList,
                _friendSetting.onError
            );
        }
        
        public void OnClickRegisterBlackList(string userId)
        {
            StartCoroutine(
                RegisterBlackList(userId)
            );
        }
        
        private IEnumerator RegisterBlackList(string userId)
        {
            void OnRegisterBlackList(
                EzBlackList request
            )
            {
                _friendSetting.onUnregisterBlackList.RemoveListener(OnRegisterBlackList);
                
                UIManager.Instance.OpenDialog1("Notice", "ブラックリストに追加しました。");
            }
            
            _friendSetting.onUnregisterBlackList.AddListener(OnRegisterBlackList);
            
            yield return _friendModel.RegisterBlackList(
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                _friendSetting.friendNamespaceName,
                userId,
                _friendSetting.onRegisterBlackList,
                _friendSetting.onError
            );
        }
        
        public void OnClickFollow(string targetUserId)
        {
            StartCoroutine(
                Follow(targetUserId)
            );
        }
        
        public IEnumerator Follow(string targetUserId)
        {
            void OnFollow(
                EzFollowUser user
            )
            {
                _friendSetting.onFollow.RemoveListener(OnFollow);
                
                UIManager.Instance.OpenDialog1("Notice", "フォローしました。");
            }
            
            _friendSetting.onFollow.AddListener(OnFollow);
            
            yield return _friendModel.Follow(
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                _friendSetting.friendNamespaceName,
                targetUserId,
                _friendSetting.onFollow,
                _friendSetting.onError
            );
        }
        
        public void OnClickUnfollow(string targetUserId)
        {
            StartCoroutine(
                Unfollow(targetUserId)
            );
        }
        
        public IEnumerator Unfollow(string targetUserId)
        {
            void OnUnfollow(
                EzFollowUser u
            )
            {
                _friendSetting.onUnfollow.RemoveListener(OnUnfollow);
                
                UIManager.Instance.OpenDialog1("Notice", "フォローを解除しました。");
                
                OnOpenFollowList();
            }
            
            _friendSetting.onUnfollow.AddListener(OnUnfollow);
            
            yield return _friendModel.Unfollow(
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                _friendSetting.friendNamespaceName,
                targetUserId,
                _friendSetting.onUnfollow,
                _friendSetting.onError
            );
        }
        
        public void OnOpenFollowList()
        {
            StartCoroutine(
                OpenFollowList()
            );
        }
        
        private IEnumerator OpenFollowList()
        {
            yield return _friendModel.DescribeFollowUsers(
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                _friendSetting.friendNamespaceName,
                _friendSetting.onDescribeFollowUsers,
                _friendSetting.onError
            );
        }
        
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
                        () => { OnOpenPublicProfile(user.UserId); }
                    );
                    friendNameView.delete.onClick.AddListener(
                        () => { OnClickUnfollow(user.UserId); }
                    );
                    friendNameView.gameObject.SetActive(true);
                }
            }
            
            _friendFollowListView.OnOpenEvent();
        }
    }
}