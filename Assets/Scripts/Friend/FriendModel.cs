using System.Collections;
using System.Collections.Generic;
using Gs2.Core.Exception;
using Gs2.Unity.Core;
using Gs2.Unity.Gs2Friend.Model;
using Gs2.Unity.Util;
using UnityEngine;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
#endif

namespace Gs2.Sample.Friend
{
    public class FriendModel : MonoBehaviour
    {
	    /// <summary>
	    /// 自分のプロフィール
	    /// My Profile
	    /// </summary>
	    public EzProfile myProfile;
	    
	    /// <summary>
	    /// フレンドリスト
	    /// Friends List
	    /// </summary>
	    public List<EzFriendUser> Friends;
	    
	    /// <summary>
	    /// 送信済みフレンドリクエスト
	    /// Friend Requests Sent
	    /// </summary>
	    public List<EzFriendRequest> Requests;
	    
	    /// <summary>
	    /// ブラックリスト
	    /// blacklist
	    /// </summary>
	    public List<string> BlackList;
	    
	    /// <summary>
	    /// フォロワーリスト
	    /// follower list
	    /// </summary>
	    public List<EzFollowUser> FollowUsers;
	    
	    /// <summary>
	    /// 自分のプロフィールを取得
	    /// Get your own profile
	    /// </summary>
	    public IEnumerator GetProfile(
		    Gs2Domain gs2,
		    GameSession gameSession,
		    string friendNamespaceName,
		    GetProfileEvent onGetProfile,
		    ErrorEvent onError
	    )
	    {
		    var domain = gs2.Friend.Namespace(
			    namespaceName: friendNamespaceName
		    ).Me(
			    gameSession: gameSession
		    ).Profile();
		    var future = domain.Model();
		    yield return future;
		    if (future.Error != null)
		    {
			    onError.Invoke(future.Error, null);
			    yield break;
		    }

		    myProfile = future.Result;
		    
		    onGetProfile.Invoke(myProfile);
	    }
#if GS2_ENABLE_UNITASK
		public async UniTask GetProfileAsync(
		    Gs2Domain gs2,
		    GameSession gameSession,
		    string friendNamespaceName,
		    GetProfileEvent onGetProfile,
		    ErrorEvent onError
	    )
	    {
		    var domain = gs2.Friend.Namespace(
			    namespaceName: friendNamespaceName
		    ).Me(
			    gameSession: gameSession
		    ).Profile();
		    try
		    {
			    myProfile = await domain.ModelAsync();
			    
			    onGetProfile.Invoke(myProfile);
		    }
		    catch (Gs2Exception e)
		    {
			    onError.Invoke(e, null);
		    }
	    }
#endif
	    
	    /// <summary>
	    /// 自分のプロフィールを更新
	    /// Update own profile
	    /// </summary>
	    public IEnumerator UpdateProfile(
		    Gs2Domain gs2,
		    GameSession gameSession,
		    string friendNamespaceName,
		    string publicProfile,
		    string followerProfile,
		    string friendProfile,
		    UpdateProfileEvent onUpdateProfile,
		    ErrorEvent onError
	    )
	    {
		    var domain = gs2.Friend.Namespace(
			    namespaceName: friendNamespaceName
		    ).Me(
			    gameSession: gameSession
		    ).Profile(
		    );
		    var future = domain.UpdateProfile(
			    publicProfile: publicProfile,
			    followerProfile: followerProfile,
			    friendProfile: friendProfile
		    );
		    yield return future;
		    if (future.Error != null)
		    {
			    onError.Invoke(future.Error, null);
			    yield break;
		    }
		    
		    var result = future.Result;
		    var future2 = result.Model();
		    yield return future2;
		    if (future2.Error != null)
		    {
			    onError.Invoke(future2.Error, null);
			    yield break;
		    }
		    
		    myProfile = future2.Result;
		    
		    onUpdateProfile.Invoke(myProfile);
	    }
#if GS2_ENABLE_UNITASK
	    public async UniTask UpdateProfileAsync(
		    Gs2Domain gs2,
		    GameSession gameSession,
		    string friendNamespaceName,
		    string publicProfile,
		    string followerProfile,
		    string friendProfile,
		    UpdateProfileEvent onUpdateProfile,
		    ErrorEvent onError
	    )
	    {
		    var domain = gs2.Friend.Namespace(
			    namespaceName: friendNamespaceName
		    ).Me(
			    gameSession: gameSession
		    ).Profile();
		    try
		    {
			    var result = await domain.UpdateProfileAsync(
				    publicProfile: publicProfile,
				    followerProfile: followerProfile,
				    friendProfile: friendProfile
			    );
			    myProfile = await result.ModelAsync();
			    
			    onUpdateProfile.Invoke(myProfile);
		    }
		    catch (Gs2Exception e)
		    {
			    onError.Invoke(e, null);
		    }
	    }
#endif
	    
	    /// <summary>
	    /// フレンドの一覧を取得
	    /// Get a list of friends
	    /// </summary>
	    public IEnumerator DescribeFriends(
		    Gs2Domain gs2,
		    GameSession gameSession,
		    string friendNamespaceName,
		    DescribeFriendsEvent onDescribeFriends,
		    ErrorEvent onError
	    )
	    {
		    Friends.Clear();
		    var domain = gs2.Friend.Namespace(
			    namespaceName: friendNamespaceName
		    ).Me(
			    gameSession: gameSession
		    );
		    var it = domain.Friends();
		    while (it.HasNext())
		    {
			    yield return it.Next();
			    if (it.Error != null)
			    {
				    onError.Invoke(it.Error, null);
				    break;
			    }

			    if (it.Current != null)
			    {
				    Friends.Add(it.Current);
			    }
		    }

		    onDescribeFriends.Invoke(Friends);
	    }
#if GS2_ENABLE_UNITASK
	    public async UniTask DescribeFriendsAsync(
		    Gs2Domain gs2,
		    GameSession gameSession,
		    string friendNamespaceName,
		    DescribeFriendsEvent onDescribeFriends,
		    ErrorEvent onError
	    )
	    {
		    var domain = gs2.Friend.Namespace(
			    namespaceName: friendNamespaceName
		    ).Me(
			    gameSession: gameSession
		    );
		    try
		    {
			    Friends = await domain.FriendsAsync().ToListAsync();
			    
			    onDescribeFriends.Invoke(Friends);
		    }
		    catch (Gs2Exception e)
		    {
			    onError.Invoke(e, null);
		    }
	    }
#endif
	    
	    /// <summary>
	    /// 送信したフレンドリクエストの一覧を取得
	    /// Get a list of friend requests you have sent
	    /// </summary>
	    public IEnumerator DescribeSendRequests(
		    Gs2Domain gs2,
		    GameSession gameSession,
		    string friendNamespaceName,
		    DescribeSendRequestsEvent onDescribeSendRequests,
		    ErrorEvent onError
	    )
	    {
		    Requests.Clear();
		    var domain = gs2.Friend.Namespace(
			    namespaceName: friendNamespaceName
		    ).Me(
			    gameSession: gameSession
		    );
		    var it = domain.SendRequests();
		    while (it.HasNext())
		    {
			    yield return it.Next();
			    if (it.Error != null)
			    {
				    onError.Invoke(it.Error, null);
				    break;
			    }

			    if (it.Current != null)
			    {
				    Requests.Add(it.Current);
			    }
		    }
		    
		    onDescribeSendRequests.Invoke(Requests);
	    }
#if GS2_ENABLE_UNITASK
	    public async UniTask DescribeSendRequestsAsync(
		    Gs2Domain gs2,
		    GameSession gameSession,
		    string friendNamespaceName,
		    DescribeSendRequestsEvent onDescribeSendRequests,
		    ErrorEvent onError
	    )
	    {
		    var domain = gs2.Friend.Namespace(
			    namespaceName: friendNamespaceName
		    ).Me(
			    gameSession: gameSession
		    );
		    try
		    {
			    Requests = await domain.SendRequestsAsync().ToListAsync();
			    
			    onDescribeSendRequests.Invoke(Requests);
		    }
		    catch (Gs2Exception e)
		    {
			    onError.Invoke(e, null);
		    }
	    }
#endif

	    /// <summary>
	    /// 受信したフレンドリクエスト一覧を取得
	    /// Get a list of received friend requests
	    /// </summary>
	    public IEnumerator DescribeReceiveRequests(
		    Gs2Domain gs2,
		    GameSession gameSession,
		    string friendNamespaceName,
		    DescribeReceiveRequestsEvent onDescribeReceiveRequests,
		    ErrorEvent onError
	    )
	    {
		    var domain = gs2.Friend.Namespace(
			    namespaceName: friendNamespaceName
		    ).Me(
			    gameSession: gameSession
		    );
		    var it = domain.ReceiveRequests();
		    while (it.HasNext())
		    {
			    yield return it.Next();
			    if (it.Error != null)
			    {
				    onError.Invoke(it.Error, null);
				    break;
			    }

			    if (it.Current != null)
			    {
				    Requests.Add(it.Current);
			    }
		    }
		    
		    onDescribeReceiveRequests.Invoke(Requests);
	    }
#if GS2_ENABLE_UNITASK
	    public async UniTask DescribeReceiveRequestsAsync(
		    Gs2Domain gs2,
		    GameSession gameSession,
		    string friendNamespaceName,
		    DescribeReceiveRequestsEvent onDescribeReceiveRequests,
		    ErrorEvent onError
	    )
	    {
		    var domain = gs2.Friend.Namespace(
			    namespaceName: friendNamespaceName
		    ).Me(
			    gameSession: gameSession
		    );
		    try
		    {
			    Requests = await domain.ReceiveRequestsAsync().ToListAsync();
			    
			    onDescribeReceiveRequests.Invoke(Requests);
		    }
		    catch (Gs2Exception e)
		    {
			    onError.Invoke(e, null);
		    }
	    }
#endif

	    /// <summary>
	    /// フレンド情報を取得
	    /// Get friend information
	    /// </summary>
	    public IEnumerator GetFriend(
		    Gs2Domain gs2,
		    GameSession gameSession,
		    string friendNamespaceName,
		    string targetUserId,
		    GetFriendEvent onGetFriend,
		    ErrorEvent onError
	    )
	    {
		    var domain = gs2.Friend.Namespace(
			    namespaceName: friendNamespaceName
		    ).Me(
			    gameSession: gameSession
		    ).Friend(
			    withProfile: false // プロフィールも一緒に取得するか / get a profile together?
		    ).FriendUser(
			    targetUserId: targetUserId
		    );
		    var future = domain.Model();
		    yield return future;
		    if (future.Error != null)
		    {
			    onError.Invoke(future.Error, null);
			    yield break;
		    }
 
		    var item = future.Result;
		    
		    onGetFriend.Invoke(item);
	    }
#if GS2_ENABLE_UNITASK
	    public async UniTask GetFriendAsync(
		    Gs2Domain gs2,
		    GameSession gameSession,
		    string friendNamespaceName,
		    string targetUserId,
		    GetFriendEvent onGetFriend,
		    ErrorEvent onError
	    )
	    {
		    var domain = gs2.Friend.Namespace(
			    namespaceName: friendNamespaceName
		    ).Me(
			    gameSession: gameSession
		    ).Friend(
			    withProfile: false // プロフィールも一緒に取得するか / get a profile together?
		    ).FriendUser(
			    targetUserId: targetUserId
		    );
		    try
		    {
			    var item = await domain.ModelAsync();
			    
			    onGetFriend.Invoke(item);
		    }
		    catch (Gs2Exception e)
		    {
			    onError.Invoke(e, null);
		    }
	    }
#endif

	    /// <summary>
	    /// フレンドを削除
	    /// Remove Friend
	    /// </summary>
	    public IEnumerator DeleteFriend(
		    Gs2Domain gs2,
		    GameSession gameSession,
		    string friendNamespaceName,
		    string targetUserId,
		    DeleteFriendEvent onDeleteFriend,
		    ErrorEvent onError
	    )
	    {
		    var domain = gs2.Friend.Namespace(
			    namespaceName: friendNamespaceName
		    ).Me(
			    gameSession: gameSession
		    ).Friend(
			    withProfile: false // プロフィールも一緒に取得するか / get a profile together?
		    ).FriendUser(
			    targetUserId: targetUserId
		    );
		    var future = domain.DeleteFriend();
		    yield return future;
		    if (future.Error != null)
		    {
			    onError.Invoke(future.Error, null);
			    yield break;
		    }
 
		    var result = future.Result;
		    var future2 = result.Model();
		    yield return future2;
		    if (future2.Error != null)
		    {
			    onError.Invoke(future2.Error, null);
			    yield break;
		    }
		    
		    var item = future2.Result;
		    onDeleteFriend.Invoke(item);
	    }
#if GS2_ENABLE_UNITASK
	    public async UniTask DeleteFriendAsync(
		    Gs2Domain gs2,
		    GameSession gameSession,
		    string friendNamespaceName,
		    string targetUserId,
		    DeleteFriendEvent onDeleteFriend,
		    ErrorEvent onError
	    )
	    {
		    var domain = gs2.Friend.Namespace(
			    namespaceName: friendNamespaceName
		    ).Me(
			    gameSession: gameSession
		    ).Friend(
			    withProfile: false // プロフィールも一緒に取得するか / get a profile together?
		    ).FriendUser(
			    targetUserId: targetUserId
		    );
		    try
		    {
			    var result = await domain.DeleteFriendAsync();
			    var item = await result.ModelAsync();
			    
			    onDeleteFriend.Invoke(item);
		    }
		    catch (Gs2Exception e)
		    {
			    onError.Invoke(e, null);
		    }
	    }
#endif

	    /// <summary>
	    /// 他プレイヤーの公開プロフィールを取得
	    /// Get public profiles of other players
	    /// </summary>
	    public IEnumerator GetPublicProfile(
		    Gs2Domain gs2,
		    string friendNamespaceName,
		    string targetUserId,
		    GetPublicProfileEvent onGetPublicProfile,
		    ErrorEvent onError
	    )
	    {
		    var domain = gs2.Friend.Namespace(
			    namespaceName: friendNamespaceName
		    ).User(
			    userId: targetUserId
		    ).PublicProfile(
		    );
		    var future = domain.Model();
		    yield return future;
		    if (future.Error != null)
		    {
			    onError.Invoke(future.Error, null);
			    yield break;
		    }
 
		    var item = future.Result;
		    onGetPublicProfile.Invoke(item);
	    }
#if GS2_ENABLE_UNITASK
	    public async UniTask GetPublicProfileAsync(
		    Gs2Domain gs2,
		    string friendNamespaceName,
		    string targetUserId,
		    GetPublicProfileEvent onGetPublicProfile,
		    ErrorEvent onError
	    )
	    {
		    var domain = gs2.Friend.Namespace(
			    namespaceName: friendNamespaceName
		    ).User(
			    userId: targetUserId
		    ).PublicProfile(
		    );
		    try
		    {
			    var item = await domain.ModelAsync();

			    onGetPublicProfile.Invoke(item);
		    }
		    catch (Gs2Exception e)
		    {
			    onError.Invoke(e, null);
		    }
	    }
#endif

	    /// <summary>
	    /// フレンドリクエストを送信
	    /// Send a friend request
	    /// </summary>
        public IEnumerator SendRequest(
		    Gs2Domain gs2,
		    GameSession gameSession,
            string friendNamespaceName,
            string targetUserId,
	        SendRequestEvent onSendRequest,
	        ErrorEvent onError
        )
        {
	        var domain = gs2.Friend.Namespace(
		        namespaceName: friendNamespaceName
	        ).Me(
		        gameSession: gameSession
	        );
	        var future = domain.SendRequest(
		        targetUserId: targetUserId
	        );
	        yield return future;
	        if (future.Error != null)
	        {
		        onError.Invoke(future.Error, null);
		        yield break;
	        }
	        var result = future.Result;
	        var future2 = result.Model();
	        yield return future2;
	        var item = future2.Result;
		    onSendRequest.Invoke(item);
        }
#if GS2_ENABLE_UNITASK
	    public async UniTask SendRequestAsync(
		    Gs2Domain gs2,
		    GameSession gameSession,
		    string friendNamespaceName,
		    string targetUserId,
		    SendRequestEvent onSendRequest,
		    ErrorEvent onError
	    )
	    {
		    var domain = gs2.Friend.Namespace(
			    namespaceName: friendNamespaceName
		    ).Me(
			    gameSession: gameSession
		    );
		    try
		    {
			    var result = await domain.SendRequestAsync(
				    targetUserId: targetUserId
			    );
			    var item = await result.ModelAsync();

			    onSendRequest.Invoke(item);
		    }
		    catch (Gs2Exception e)
		    {
			    onError.Invoke(e, null);
		    }
	    }
#endif

	    /// <summary>
	    /// フレンドリクエストを承諾
	    /// Accept Friend Request
	    /// </summary>
        public IEnumerator Accept(
		    Gs2Domain gs2,
		    GameSession gameSession,
	        string friendNamespaceName,
	        string fromUserId,
	        AcceptEvent onAccept,
	        ErrorEvent onError
        )
        {
	        var domain = gs2.Friend.Namespace(
		        namespaceName: friendNamespaceName
	        ).Me(
		        gameSession: gameSession
	        ).ReceiveFriendRequest(
		        fromUserId: fromUserId
	        );
	        var future = domain.Accept();
	        yield return future;
	        if (future.Error != null)
	        {
		        onError.Invoke(future.Error, null);
		        yield break;
	        }
	        
	        var result = future.Result;
	        var future2 = result.Model();
	        yield return future2;
	        var item = future2.Result;
	        onAccept.Invoke(item);
        }
#if GS2_ENABLE_UNITASK
	    public async UniTask AcceptAsync(
		    Gs2Domain gs2,
		    GameSession gameSession,
		    string friendNamespaceName,
		    string fromUserId,
		    AcceptEvent onAccept,
		    ErrorEvent onError
	    )
	    {
		    var domain = gs2.Friend.Namespace(
			    namespaceName: friendNamespaceName
		    ).Me(
			    gameSession: gameSession
		    ).ReceiveFriendRequest(
			    fromUserId: fromUserId
		    );
		    try
		    {
			    var result  = await domain.AcceptAsync();
			    var item = await result.ModelAsync();
			    
			    onAccept.Invoke(item);
		    }
		    catch (Gs2Exception e)
		    {
			    onError.Invoke(e, null);
		    }
	    }
#endif

	    /// <summary>
	    /// フレンドリクエストを拒否
	    /// Deny friend request
	    /// </summary>
        public IEnumerator Reject(
		    Gs2Domain gs2,
		    GameSession gameSession,
	        string friendNamespaceName,
	        string fromUserId,
	        RejectEvent onReject,
	        ErrorEvent onError
        )
        {
	        var domain = gs2.Friend.Namespace(
		        namespaceName: friendNamespaceName
	        ).Me(
		        gameSession: gameSession
	        ).ReceiveFriendRequest(
		        fromUserId: fromUserId
	        );
	        var future = domain.Reject();
	        yield return future;
	        if (future.Error != null)
	        {
		        onError.Invoke(future.Error, null);
		        yield break;
	        }
	        
	        var result = future.Result;
	        var future2 = result.Model();
	        yield return future2;
	        var item = future2.Result;
	        onReject.Invoke(item);
        }
#if GS2_ENABLE_UNITASK
	    public async UniTask RejectAsync(
		    Gs2Domain gs2,
		    GameSession gameSession,
		    string friendNamespaceName,
		    string fromUserId,
		    RejectEvent onReject,
		    ErrorEvent onError
	    )
	    {
		    var domain = gs2.Friend.Namespace(
			    namespaceName: friendNamespaceName
		    ).Me(
			    gameSession: gameSession
		    ).ReceiveFriendRequest(
			    fromUserId: fromUserId
		    );
		    try
		    {
			    var result = await domain.RejectAsync();
			    var item = await result.ModelAsync();
			    onReject.Invoke(item);
		    }
		    catch (Gs2Exception e)
		    {
			    onError.Invoke(e, null);
		    }
	    }
#endif

	    /// <summary>
	    /// 送信したフレンドリクエストを削除
	    /// Delete a friend request you have sent
	    /// </summary>
        public IEnumerator DeleteRequest(
		    Gs2Domain gs2,
		    GameSession gameSession,
	        string friendNamespaceName,
	        string targetUserId,
	        DeleteRequestEvent onDeleteRequest,
	        ErrorEvent onError
        )
        {
	        var domain = gs2.Friend.Namespace(
		        namespaceName: friendNamespaceName
	        ).Me(
		        gameSession: gameSession
	        ).SendFriendRequest(
		        targetUserId: targetUserId
	        );
	        var future = domain.DeleteRequest();
	        yield return future;
	        if (future.Error != null)
	        {
		        onError.Invoke(future.Error, null);
		        yield break;
	        }
	        var result = future.Result;
	        var future2 = result.Model();
	        yield return future2;
	        var item = future2.Result;
	        onDeleteRequest.Invoke(item);
        }
#if GS2_ENABLE_UNITASK
	    public async UniTask DeleteRequestAsync(
		    Gs2Domain gs2,
		    GameSession gameSession,
		    string friendNamespaceName,
		    string targetUserId,
		    DeleteRequestEvent onDeleteRequest,
		    ErrorEvent onError
	    )
	    {
		    var domain = gs2.Friend.Namespace(
			    namespaceName: friendNamespaceName
		    ).Me(
			    gameSession: gameSession
		    ).SendFriendRequest(
			    targetUserId: targetUserId
		    );
		    try
		    {
			    var result = await domain.DeleteRequestAsync();
			    var item = await result.ModelAsync();
			    onDeleteRequest.Invoke(item);
		    }
		    catch (Gs2Exception e)
		    {
			    onError.Invoke(e, null);
		    }
	    }
#endif

        /// <summary>
        /// ブラックリストを取得
        /// Get Blacklist
        /// </summary>
        public IEnumerator GetBlackList(
	        Gs2Domain gs2,
	        GameSession gameSession,
	        string friendNamespaceName,
	        GetBlackListEvent onGetBlackList,
	        ErrorEvent onError
        )
        {
	        BlackList.Clear();
	        var domain = gs2.Friend.Namespace(
		        namespaceName: friendNamespaceName
	        ).Me(
		        gameSession: gameSession
	        );
	        var it = domain.BlackLists();
	        while (it.HasNext())
	        {
		        yield return it.Next();
		        if (it.Error != null)
		        {
			        onError.Invoke(it.Error, null);
			        break;
		        }

		        if (it.Current != null)
		        {
			        BlackList.Add(it.Current);
		        }
	        }
		    
	        onGetBlackList.Invoke(BlackList);
        }
#if GS2_ENABLE_UNITASK
	    public async UniTask GetBlackListAsync(
		    Gs2Domain gs2,
		    GameSession gameSession,
		    string friendNamespaceName,
		    GetBlackListEvent onGetBlackList,
		    ErrorEvent onError
	    )
	    {
		    var domain = gs2.Friend.Namespace(
			    namespaceName: friendNamespaceName
		    ).Me(
			    gameSession: gameSession
		    );
		    try
		    {
			    BlackList = await domain.BlackListsAsync().ToListAsync();
			    onGetBlackList.Invoke(BlackList);
		    }
		    catch (Gs2Exception e)
		    {
			    onError.Invoke(e, null);
		    }
	    }
#endif

        /// <summary>
        /// ブラックリストにユーザーを登録
        /// Register users on the blacklist
        /// </summary>
        public IEnumerator RegisterBlackList(
	        Gs2Domain gs2,
	        GameSession gameSession,
	        string friendNamespaceName,
	        string targetUserId,
	        RegisterBlackListEvent onRegisterBlackList,
	        ErrorEvent onError
        )
        {
	        var domain = gs2.Friend.Namespace(
		        namespaceName: friendNamespaceName
	        ).Me(
		        gameSession: gameSession
	        ).BlackList();
	        var future = domain.RegisterBlackList(
		        targetUserId: targetUserId
	        );
	        yield return future;
	        if (future.Error != null)
	        {
		        onError.Invoke(future.Error, null);
		        yield break;
	        }
	        var result = future.Result;
	        var future2 = result.Model();
	        yield return future2;
	        var item = future2.Result;
	        onRegisterBlackList.Invoke(item);
        }
#if GS2_ENABLE_UNITASK
	    public async UniTask RegisterBlackListAsync(
		    Gs2Domain gs2,
		    GameSession gameSession,
		    string friendNamespaceName,
		    string targetUserId,
		    RegisterBlackListEvent onRegisterBlackList,
		    ErrorEvent onError
	    )
	    {
		    var domain = gs2.Friend.Namespace(
			    namespaceName: friendNamespaceName
		    ).Me(
			    gameSession: gameSession
		    ).BlackList();
		    try
		    {
			    var result = await domain.RegisterBlackListAsync(
				    targetUserId: targetUserId
			    );
			    var item = await result.ModelAsync();
			    onRegisterBlackList.Invoke(item);
		    }
		    catch (Gs2Exception e)
		    {
			    onError.Invoke(e, null);
		    }
	    }
#endif

        /// <summary>
        /// ブラックリストからユーザーを削除
        /// Remove users from blacklist
        /// </summary>
        public IEnumerator UnregisterBlackList(
	        Gs2Domain gs2,
	        GameSession gameSession,
	        string friendNamespaceName,
	        string targetUserId,
	        UnregisterBlackListEvent onUnregisterBlackList,
	        ErrorEvent onError
        )
        {
	        var domain = gs2.Friend.Namespace(
		        namespaceName: friendNamespaceName
	        ).Me(
		        gameSession: gameSession
	        ).BlackList();
	        var future = domain.UnregisterBlackList(
		        targetUserId: targetUserId
	        );
	        yield return future;
	        if (future.Error != null)
	        {
		        onError.Invoke(future.Error, null);
		        yield break;
	        }
	        
	        var result = future.Result;
	        var future2 = result.Model();
	        yield return future2;
	        if (future2.Error != null)
	        {
		        onError.Invoke(future2.Error, null);
		        yield break;
	        }
	        
	        var item = future2.Result;
	        onUnregisterBlackList.Invoke(item);
        }
#if GS2_ENABLE_UNITASK
	    public async UniTask UnregisterBlackListAsync(
		    Gs2Domain gs2,
		    GameSession gameSession,
		    string friendNamespaceName,
		    string targetUserId,
		    UnregisterBlackListEvent onUnregisterBlackList,
		    ErrorEvent onError
	    )
	    {
		    var domain = gs2.Friend.Namespace(
			    namespaceName: friendNamespaceName
		    ).Me(
			    gameSession: gameSession
		    ).BlackList();
		    try
		    {
			    var result = await domain.UnregisterBlackListAsync(
				    targetUserId: targetUserId
			    );
			    var item = await result.ModelAsync();
			    onUnregisterBlackList.Invoke(item);
		    }
		    catch (Gs2Exception e)
		    {
			    onError.Invoke(e, null);
		    }
	    }
#endif

        /// <summary>
        /// 他プレイヤーをフォローする
        /// Follow other players
        /// </summary>
        public IEnumerator Follow(
	        Gs2Domain gs2,
	        GameSession gameSession,
	        string friendNamespaceName,
	        string targetUserId,
	        FollowEvent onFollow,
	        ErrorEvent onError
        )
        {
	        var domain = gs2.Friend.Namespace(
		        namespaceName: friendNamespaceName
	        ).Me(
		        gameSession: gameSession
	        ).FollowUser(
		        targetUserId: targetUserId,
		        withProfile: false
	        );
	        var future = domain.Follow();
	        yield return future;
	        if (future.Error != null)
	        {
		        onError.Invoke(future.Error, null);
		        yield break;
	        }
	        
	        var result = future.Result;
	        var future2 = result.Model();
	        yield return future2;
	        var item = future2.Result;
	        onFollow.Invoke(item);
        }
#if GS2_ENABLE_UNITASK
	    public async UniTask FollowAsync(
		    Gs2Domain gs2,
		    GameSession gameSession,
		    string friendNamespaceName,
		    string targetUserId,
		    FollowEvent onFollow,
		    ErrorEvent onError
	    )
	    {
		    var domain = gs2.Friend.Namespace(
			    namespaceName: friendNamespaceName
		    ).Me(
			    gameSession: gameSession
		    ).FollowUser(
			    targetUserId: targetUserId,
			    withProfile: false
		    );
		    try
		    {
			    var result = await domain.FollowAsync();
			    var item = await result.ModelAsync();
			    onFollow.Invoke(item);
		    }
		    catch (Gs2Exception e)
		    {
			    onError.Invoke(e, null);
		    }
	    }
#endif

        /// <summary>
        /// フォローしている相手をアンフォローする
        /// Unfollowing someone you are following
        /// </summary>
        public IEnumerator Unfollow(
	        Gs2Domain gs2,
	        GameSession gameSession,
	        string friendNamespaceName,
	        string targetUserId,
	        UnfollowEvent onUnfollow,
	        ErrorEvent onError
        )
        {
	        var domain = gs2.Friend.Namespace(
		        namespaceName: friendNamespaceName
	        ).Me(
		        gameSession: gameSession
	        ).FollowUser(
		        targetUserId: targetUserId,
		        withProfile: false
	        );
	        var future = domain.Unfollow();
	        yield return future;
	        if (future.Error != null)
	        {
		        onError.Invoke(future.Error, null);
		        yield break;
	        }
 
	        onUnfollow.Invoke();
        }
#if GS2_ENABLE_UNITASK
	    public async UniTask UnfollowAsync(
		    Gs2Domain gs2,
		    GameSession gameSession,
		    string friendNamespaceName,
		    string targetUserId,
		    UnfollowEvent onUnfollow,
		    ErrorEvent onError
	    )
	    {
		    var domain = gs2.Friend.Namespace(
			    namespaceName: friendNamespaceName
		    ).Me(
			    gameSession: gameSession
		    ).FollowUser(
			    targetUserId: targetUserId,
			    withProfile: false
		    );
		    try
		    {
			    var item = await domain.UnfollowAsync();
			    onUnfollow.Invoke();
		    }
		    catch (Gs2Exception e)
		    {
			    onError.Invoke(e, null);
		    }
	    }
#endif

        /// <summary>
        /// フォローしているユーザー一覧を取得
        /// Get a list of users you are following
        /// </summary>
        public IEnumerator DescribeFollowUsers(
	        Gs2Domain gs2,
	        GameSession gameSession,
	        string friendNamespaceName,
	        DescribeFollowUsersEvent onDescribeFollowUsers,
	        ErrorEvent onError
        )
        {
	        FollowUsers.Clear();
	        var domain = gs2.Friend.Namespace(
		        namespaceName: friendNamespaceName
	        ).Me(
		        gameSession: gameSession
	        );
	        var it = domain.Follows();
	        while (it.HasNext())
	        {
		        yield return it.Next();
		        if (it.Error != null)
		        {
			        onError.Invoke(it.Error, null);
			        break;
		        }

		        if (it.Current != null)
		        {
			        FollowUsers.Add(it.Current);
		        }
	        }
		    
	        onDescribeFollowUsers.Invoke(FollowUsers);
        }
#if GS2_ENABLE_UNITASK
	    public async UniTask DescribeFollowUsersAsync(
		    Gs2Domain gs2,
		    GameSession gameSession,
		    string friendNamespaceName,
		    DescribeFollowUsersEvent onDescribeFollowUsers,
		    ErrorEvent onError
	    )
	    {
		    var domain = gs2.Friend.Namespace(
			    namespaceName: friendNamespaceName
		    ).Me(
			    gameSession: gameSession
		    );
		    try
		    {
			    FollowUsers = await domain.FollowsAsync().ToListAsync();

			    onDescribeFollowUsers.Invoke(FollowUsers);
		    }
		    catch (Gs2Exception e)
		    {
			    onError.Invoke(e, null);
		    }
	    }
#endif

    }
}