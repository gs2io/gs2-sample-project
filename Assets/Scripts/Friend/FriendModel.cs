using System.Collections;
using System.Collections.Generic;
using Gs2.Core;
using Gs2.Unity;
using Gs2.Unity.Gs2Friend.Model;
using Gs2.Unity.Gs2Friend.Result;
using Gs2.Unity.Util;
using UnityEngine;

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
	    /// <param name="client"></param>
	    /// <param name="session"></param>
	    /// <param name="friendNamespaceName"></param>
	    /// <param name="onGetProfile"></param>
	    /// <param name="onError"></param>
	    /// <returns></returns>
	    public IEnumerator GetProfile(
		    Client client,
		    GameSession session,
		    string friendNamespaceName,
		    GetProfileEvent onGetProfile,
		    ErrorEvent onError
	    )
	    {
		    AsyncResult<EzGetProfileResult> result = null;
		    yield return client.Friend.GetProfile(
			    callback: r => { result = r; },
			    session,
			    friendNamespaceName
		    );

		    if (result.Error != null)
		    {
			    onError.Invoke(result.Error);
			    yield break;
		    }

		    myProfile = result.Result.Item;
		    
		    onGetProfile.Invoke(myProfile);
	    }
	    
	    /// <summary>
	    /// 自分のプロフィールを更新
	    /// Update own profile
	    /// </summary>
	    /// <param name="client"></param>
	    /// <param name="session"></param>
	    /// <param name="friendNamespaceName"></param>
	    /// <param name="publicProfile"></param>
	    /// <param name="followerProfile"></param>
	    /// <param name="friendProfile"></param>
	    /// <param name="onUpdateProfile"></param>
	    /// <param name="onError"></param>
	    /// <returns></returns>
	    public IEnumerator UpdateProfile(
		    Client client,
		    GameSession session,
		    string friendNamespaceName,
		    string publicProfile,
		    string followerProfile,
		    string friendProfile,
		    UpdateProfileEvent onUpdateProfile,
		    ErrorEvent onError
	    )
	    {
		    AsyncResult<EzUpdateProfileResult> result = null;
		    yield return client.Friend.UpdateProfile(
			    callback: r => { result = r; },
			    session,
			    friendNamespaceName,
			    publicProfile,
			    followerProfile,
			    friendProfile
		    );

		    if (result.Error != null)
		    {
			    onError.Invoke(result.Error);
			    yield break;
		    }

		    myProfile = result.Result.Item;
		    
		    onUpdateProfile.Invoke(myProfile);
	    }
	    
	    /// <summary>
	    /// フレンドの一覧を取得
	    /// Get a list of friends
	    /// </summary>
	    /// <param name="client"></param>
	    /// <param name="session"></param>
	    /// <param name="friendNamespaceName"></param>
	    /// <param name="onDescribeFriends"></param>
	    /// <param name="onError"></param>
	    /// <returns></returns>
	    public IEnumerator DescribeFriends(
		    Client client,
		    GameSession session,
		    string friendNamespaceName,
		    DescribeFriendsEvent onDescribeFriends,
		    ErrorEvent onError
	    )
	    {
		    AsyncResult<EzDescribeFriendsResult> result = null;
		    yield return client.Friend.DescribeFriends(
			    callback: r => { result = r; },
			    session,
			    friendNamespaceName,
			    false,
			    30,
			    null
		    );

		    if (result.Error != null)
		    {
			    onError.Invoke(result.Error);
			    yield break;
		    }

		    Friends = result.Result.Items;
		    
		    onDescribeFriends.Invoke(Friends);
	    }
	    
	    /// <summary>
	    /// 送信したフレンドリクエストの一覧を取得
	    /// Get a list of friend requests you have sent
	    /// </summary>
	    /// <param name="client"></param>
	    /// <param name="session"></param>
	    /// <param name="friendNamespaceName"></param>
	    /// <param name="onDescribeSendRequests"></param>
	    /// <param name="onError"></param>
	    /// <returns></returns>
	    public IEnumerator DescribeSendRequests(
		    Client client,
		    GameSession session,
		    string friendNamespaceName,
		    DescribeSendRequestsEvent onDescribeSendRequests,
		    ErrorEvent onError
	    )
	    {
		    AsyncResult<EzDescribeSendRequestsResult> result = null;
		    yield return client.Friend.DescribeSendRequests(
			    callback: r => { result = r; },
			    session,
			    friendNamespaceName
		    );

		    if (result.Error != null)
		    {
			    onError.Invoke(result.Error);
			    yield break;
		    }

		    Requests = result.Result.Items;
		    
		    onDescribeSendRequests.Invoke(Requests);
	    }
	    
	    /// <summary>
	    /// 受信したフレンドリクエスト一覧を取得
	    /// Get a list of received friend requests
	    /// </summary>
	    /// <param name="client"></param>
	    /// <param name="session"></param>
	    /// <param name="friendNamespaceName"></param>
	    /// <param name="onDescribeReceiveRequests"></param>
	    /// <param name="onError"></param>
	    /// <returns></returns>
	    public IEnumerator DescribeReceiveRequests(
		    Client client,
		    GameSession session,
		    string friendNamespaceName,
		    DescribeReceiveRequestsEvent onDescribeReceiveRequests,
		    ErrorEvent onError
	    )
	    {
		    AsyncResult<EzDescribeReceiveRequestsResult> result = null;
		    yield return client.Friend.DescribeReceiveRequests(
			    callback: r => { result = r; },
			    session,
			    friendNamespaceName
		    );

		    if (result.Error != null)
		    {
			    onError.Invoke(result.Error);
			    yield break;
		    }

		    Requests = result.Result.Items;
		    
		    onDescribeReceiveRequests.Invoke(Requests);
	    }
	    
	    /// <summary>
	    /// フレンド情報を取得
	    /// Get friend information
	    /// </summary>
	    /// <param name="client"></param>
	    /// <param name="session"></param>
	    /// <param name="friendNamespaceName"></param>
	    /// <param name="targetUserId"></param>
	    /// <param name="onGetFriend"></param>
	    /// <param name="onError"></param>
	    /// <returns></returns>
	    public IEnumerator GetFriend(
		    Client client,
		    GameSession session,
		    string friendNamespaceName,
		    string targetUserId,
		    GetFriendEvent onGetFriend,
		    ErrorEvent onError
	    )
	    {
		    AsyncResult<EzGetFriendResult> result = null;
		    yield return client.Friend.GetFriend(
			    callback: r => { result = r; },
			    session,
			    friendNamespaceName,
			    targetUserId,
			    true // プロフィールも一緒に取得するか / get a profile together?
		    );

		    if (result.Error != null)
		    {
			    onError.Invoke(result.Error);
			    yield break;
		    }

		    var item = result.Result.Item;
		    
		    onGetFriend.Invoke(item);
	    }
	    
	    /// <summary>
	    /// フレンドを削除
	    /// Remove Friend
	    /// </summary>
	    /// <param name="client"></param>
	    /// <param name="session"></param>
	    /// <param name="friendNamespaceName"></param>
	    /// <param name="targetUserId"></param>
	    /// <param name="onDeleteFriend"></param>
	    /// <param name="onError"></param>
	    /// <returns></returns>
	    public IEnumerator DeleteFriend(
		    Client client,
		    GameSession session,
		    string friendNamespaceName,
		    string targetUserId,
		    DeleteFriendEvent onDeleteFriend,
		    ErrorEvent onError
	    )
	    {
		    AsyncResult<EzDeleteFriendResult> result = null;
		    yield return client.Friend.DeleteFriend(
			    callback: r => { result = r; },
			    session,
			    friendNamespaceName,
			    targetUserId
		    );

		    if (result.Error != null)
		    {
			    onError.Invoke(result.Error);
			    yield break;
		    }

		    var item = result.Result.Item;
		    onDeleteFriend.Invoke(item);
	    }
	    
	    /// <summary>
	    /// 他プレイヤーの公開プロフィールを取得
	    /// Get public profiles of other players
	    /// </summary>
	    /// <param name="client"></param>
	    /// <param name="friendNamespaceName"></param>
	    /// <param name="targetUserId"></param>
	    /// <param name="onGetPublicProfile"></param>
	    /// <param name="onError"></param>
	    /// <returns></returns>
	    public IEnumerator GetPublicProfile(
		    Client client,
		    string friendNamespaceName,
		    string targetUserId,
		    GetPublicProfileEvent onGetPublicProfile,
		    ErrorEvent onError
	    )
	    {
		    AsyncResult<EzGetPublicProfileResult> result = null;
		    yield return client.Friend.GetPublicProfile(
			    callback: r => { result = r; },
			    friendNamespaceName,
			    targetUserId
		    );

		    if (result.Error != null)
		    {
			    onError.Invoke(result.Error);
			    yield break;
		    }

		    var item = result.Result.Item;
		    onGetPublicProfile.Invoke(item);
	    }
	    
	    /// <summary>
	    /// フレンドリクエストを送信
	    /// Send a friend request
	    /// </summary>
	    /// <param name="client"></param>
	    /// <param name="session"></param>
	    /// <param name="friendNamespaceName"></param>
	    /// <param name="targetUserId"></param>
	    /// <param name="onSendRequest"></param>
	    /// <param name="onError"></param>
	    /// <returns></returns>
        public IEnumerator SendRequest(
	        Client client,
            GameSession session,
            string friendNamespaceName,
            string targetUserId,
	        SendRequestEvent onSendRequest,
	        ErrorEvent onError
        )
        {
		    AsyncResult<EzSendRequestResult> result = null;
		    yield return client.Friend.SendRequest(
		        callback: r => { result = r; },
		        session,
		        friendNamespaceName,
		        targetUserId
		    );

		    if (result.Error != null)
		    {
			    onError.Invoke(result.Error);
		        yield break;
		    }

		    var item = result.Result.Item;
		    onSendRequest.Invoke(item);
        }
        
	    /// <summary>
	    /// フレンドリクエストを承諾
	    /// Accept Friend Request
	    /// </summary>
	    /// <param name="client"></param>
	    /// <param name="session"></param>
	    /// <param name="friendNamespaceName"></param>
	    /// <param name="fromUserId"></param>
	    /// <param name="onAccept"></param>
	    /// <param name="onError"></param>
	    /// <returns></returns>
        public IEnumerator Accept(
	        Client client,
	        GameSession session,
	        string friendNamespaceName,
	        string fromUserId,
	        AcceptEvent onAccept,
	        ErrorEvent onError
        )
        {
	        AsyncResult<EzAcceptResult> result = null;
	        yield return client.Friend.Accept(
		        callback: r => { result = r; },
		        session,
		        friendNamespaceName,
		        fromUserId
	        );

	        if (result.Error != null)
	        {
		        onError.Invoke(result.Error);
		        yield break;
	        }

	        var item = result.Result.Item;
	        onAccept.Invoke(item);
        }
        
	    /// <summary>
	    /// フレンドリクエストを拒否
	    /// Deny friend request
	    /// </summary>
	    /// <param name="client"></param>
	    /// <param name="session"></param>
	    /// <param name="friendNamespaceName"></param>
	    /// <param name="fromUserId"></param>
	    /// <param name="onReject"></param>
	    /// <param name="onError"></param>
	    /// <returns></returns>
        public IEnumerator Reject(
	        Client client,
	        GameSession session,
	        string friendNamespaceName,
	        string fromUserId,
	        RejectEvent onReject,
	        ErrorEvent onError
        )
        {
	        AsyncResult<EzRejectResult> result = null;
	        yield return client.Friend.Reject(
		        callback: r => { result = r; },
		        session,
		        friendNamespaceName,
		        fromUserId
	        );

	        if (result.Error != null)
	        {
		        onError.Invoke(result.Error);
		        yield break;
	        }

	        var item = result.Result.Item;
	        onReject.Invoke(item);
        }
        
	    /// <summary>
	    /// 送信したフレンドリクエストを削除
	    /// Delete a friend request you have sent
	    /// </summary>
	    /// <param name="client"></param>
	    /// <param name="session"></param>
	    /// <param name="friendNamespaceName"></param>
	    /// <param name="targetUserId"></param>
	    /// <param name="onDeleteRequest"></param>
	    /// <param name="onError"></param>
	    /// <returns></returns>
        public IEnumerator DeleteRequest(
	        Client client,
	        GameSession session,
	        string friendNamespaceName,
	        string targetUserId,
	        DeleteRequestEvent onDeleteRequest,
	        ErrorEvent onError
        )
        {
	        AsyncResult<EzDeleteRequestResult> result = null;
	        yield return client.Friend.DeleteRequest(
		        callback: r => { result = r; },
		        session,
		        friendNamespaceName,
		        targetUserId
	        );

	        if (result.Error != null)
	        {
		        onError.Invoke(result.Error);
		        yield break;
	        }

	        var item = result.Result.Item;
	        onDeleteRequest.Invoke(item);
        }
        
        /// <summary>
        /// ブラックリストを取得
        /// Get Blacklist
        /// </summary>
        /// <param name="client"></param>
        /// <param name="session"></param>
        /// <param name="friendNamespaceName"></param>
        /// <param name="onGetBlackList"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator GetBlackList(
	        Client client,
	        GameSession session,
	        string friendNamespaceName,
	        GetBlackListEvent onGetBlackList,
	        ErrorEvent onError
        )
        {
	        AsyncResult<EzGetBlackListResult> result = null;
	        yield return client.Friend.GetBlackList(
		        callback: r => { result = r; },
		        session,
		        friendNamespaceName
	        );

	        if (result.Error != null)
	        {
		        onError.Invoke(result.Error);
		        yield break;
	        }

	        BlackList = result.Result.Items;
		    
	        onGetBlackList.Invoke(BlackList);
        }
        
        /// <summary>
        /// ブラックリストにユーザーを登録
        /// Register users on the blacklist
        /// </summary>
        /// <param name="client"></param>
        /// <param name="session"></param>
        /// <param name="friendNamespaceName"></param>
        /// <param name="targetUserId"></param>
        /// <param name="onRegisterBlackList"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator RegisterBlackList(
	        Client client,
	        GameSession session,
	        string friendNamespaceName,
	        string targetUserId,
	        RegisterBlackListEvent onRegisterBlackList,
	        ErrorEvent onError
        )
        {
	        AsyncResult<EzRegisterBlackListResult> result = null;
	        yield return client.Friend.RegisterBlackList(
		        callback: r => { result = r; },
		        session,
		        friendNamespaceName,
		        targetUserId
	        );

	        if (result.Error != null)
	        {
		        onError.Invoke(result.Error);
		        yield break;
	        }

	        var item = result.Result.Item;
	        onRegisterBlackList.Invoke(item);
        }
        
        /// <summary>
        /// ブラックリストからユーザーを削除
        /// Remove users from blacklist
        /// </summary>
        /// <param name="client"></param>
        /// <param name="session"></param>
        /// <param name="friendNamespaceName"></param>
        /// <param name="targetUserId"></param>
        /// <param name="onUnregisterBlackList"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator UnregisterBlackList(
	        Client client,
	        GameSession session,
	        string friendNamespaceName,
	        string targetUserId,
	        UnregisterBlackListEvent onUnregisterBlackList,
	        ErrorEvent onError
        )
        {
	        AsyncResult<EzUnregisterBlackListResult> result = null;
	        yield return client.Friend.UnregisterBlackList(
		        callback: r => { result = r; },
		        session,
		        friendNamespaceName,
		        targetUserId
	        );

	        if (result.Error != null)
	        {
		        onError.Invoke(result.Error);
		        yield break;
	        }

	        var item = result.Result.Item;
	        onUnregisterBlackList.Invoke(item);
        }
        
        /// <summary>
        /// 他プレイヤーをフォローする
        /// Follow other players
        /// </summary>
        /// <param name="client"></param>
        /// <param name="session"></param>
        /// <param name="friendNamespaceName"></param>
        /// <param name="targetUserId"></param>
        /// <param name="onFollow"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator Follow(
	        Client client,
	        GameSession session,
	        string friendNamespaceName,
	        string targetUserId,
	        FollowEvent onFollow,
	        ErrorEvent onError
        )
        {
	        AsyncResult<EzFollowResult> result = null;
	        yield return client.Friend.Follow(
		        callback: r => { result = r; },
		        session,
		        friendNamespaceName,
		        targetUserId
	        );

	        if (result.Error != null)
	        {
		        onError.Invoke(result.Error);
		        yield break;
	        }

	        var item = result.Result.Item;
	        onFollow.Invoke(item);
        }
        
        /// <summary>
        /// フォローしている相手をアンフォローする
        /// Unfollowing someone you are following
        /// </summary>
        /// <param name="client"></param>
        /// <param name="session"></param>
        /// <param name="friendNamespaceName"></param>
        /// <param name="targetUserId"></param>
        /// <param name="onUnfollow"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator Unfollow(
	        Client client,
	        GameSession session,
	        string friendNamespaceName,
	        string targetUserId,
	        UnfollowEvent onUnfollow,
	        ErrorEvent onError
        )
        {
	        AsyncResult<EzUnfollowResult> result = null;
	        yield return client.Friend.Unfollow(
		        callback: r => { result = r; },
		        session,
		        friendNamespaceName,
		        targetUserId
	        );

	        if (result.Error != null)
	        {
		        onError.Invoke(result.Error);
		        yield break;
	        }

	        var item = result.Result.Item;
	        onUnfollow.Invoke(item);
        }
        
        /// <summary>
        /// フォローしているユーザー一覧を取得
        /// Get a list of users you are following
        /// </summary>
        /// <param name="client"></param>
        /// <param name="session"></param>
        /// <param name="friendNamespaceName"></param>
        /// <param name="onDescribeFollowUsers"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator DescribeFollowUsers(
	        Client client,
	        GameSession session,
	        string friendNamespaceName,
	        DescribeFollowUsersEvent onDescribeFollowUsers,
	        ErrorEvent onError
        )
        {
	        AsyncResult<EzDescribeFollowUsersResult> result = null;
	        yield return client.Friend.DescribeFollowUsers(
		        callback: r => { result = r; },
		        session,
		        friendNamespaceName,
		        true
	        );

	        if (result.Error != null)
	        {
		        onError.Invoke(result.Error);
		        yield break;
	        }

	        FollowUsers = result.Result.Items;
		    
	        onDescribeFollowUsers.Invoke(FollowUsers);
        }
    }
}