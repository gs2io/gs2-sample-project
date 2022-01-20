using System;
using System.Collections.Generic;
using Gs2.Unity.Gs2Friend.Model;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Friend
{
    [System.Serializable]
    public class GetProfileEvent : UnityEvent<EzProfile>
    {
    }
    
    [System.Serializable]
    public class UpdateProfileEvent : UnityEvent<EzProfile>
    {
    }
    
    [System.Serializable]
    public class DescribeFriendsEvent : UnityEvent<List<EzFriendUser>>
    {
    }

    [System.Serializable]
    public class SendRequestEvent : UnityEvent<EzFriendRequest>
    {
    }
    
    [System.Serializable]
    public class AcceptEvent : UnityEvent<EzFriendRequest>
    {
    }
    
    [System.Serializable]
    public class RejectEvent : UnityEvent<EzFriendRequest>
    {
    }
    
    [System.Serializable]
    public class DeleteRequestEvent : UnityEvent<EzFriendRequest>
    {
    }
    
    [System.Serializable]
    public class GetFriendEvent : UnityEvent<EzFriendUser>
    {
    }
    
    [System.Serializable]
    public class DeleteFriendEvent : UnityEvent<EzFriendUser>
    {
    }
    
    [System.Serializable]
    public class DescribeSendRequestsEvent : UnityEvent<List<EzFriendRequest>>
    {
    }
    
    [System.Serializable]
    public class DescribeReceiveRequestsEvent : UnityEvent<List<EzFriendRequest>>
    {
    }
    
    [System.Serializable]
    public class GetPublicProfileEvent : UnityEvent<EzPublicProfile>
    {
    }
    
    [System.Serializable]
    public class GetBlackListEvent : UnityEvent<List<string>>
    {
    }
    
    [System.Serializable]
    public class RegisterBlackListEvent : UnityEvent<EzBlackList>
    {
    }
    
    [System.Serializable]
    public class UnregisterBlackListEvent : UnityEvent<EzBlackList>
    {
    }
    
    [System.Serializable]
    public class FollowEvent : UnityEvent<EzFollowUser>
    {
    }
    
    [System.Serializable]
    public class UnfollowEvent : UnityEvent<EzFollowUser>
    {
    }
    
    [System.Serializable]
    public class DescribeFollowUsersEvent : UnityEvent<List<EzFollowUser>>
    {
    }
    
    [Serializable]
    public class FriendSetting : MonoBehaviour
    {
        [SerializeField]
        public string friendNamespaceName;
        
        [SerializeField]
        public GetProfileEvent onGetProfile = new GetProfileEvent();
        
        [SerializeField]
        public UpdateProfileEvent onUpdateProfile = new UpdateProfileEvent();
        
        [SerializeField]
        public DescribeFriendsEvent onDescribeFriends = new DescribeFriendsEvent();
        
        [SerializeField]
        public SendRequestEvent onSendRequest = new SendRequestEvent();
        
        [SerializeField]
        public AcceptEvent onAccept = new AcceptEvent();
        
        [SerializeField]
        public RejectEvent onReject = new RejectEvent();
        
        [SerializeField]
        public DeleteRequestEvent onDeleteRequest = new DeleteRequestEvent();
        
        [SerializeField]
        public DescribeSendRequestsEvent onDescribeSendRequests = new DescribeSendRequestsEvent();
        
        [SerializeField]
        public DescribeReceiveRequestsEvent onDescribeReceiveRequests = new DescribeReceiveRequestsEvent();
        
        [SerializeField]
        public GetFriendEvent onGetFriend = new GetFriendEvent();

        [SerializeField]
        public DeleteFriendEvent onDeleteFriend = new DeleteFriendEvent();
        
        [SerializeField]
        public GetPublicProfileEvent onGetPublicProfile = new GetPublicProfileEvent();
        
        [SerializeField]
        public GetBlackListEvent onGetBlackList = new GetBlackListEvent();
        
        [SerializeField]
        public RegisterBlackListEvent onRegisterBlackList = new RegisterBlackListEvent();
        
        [SerializeField]
        public UnregisterBlackListEvent onUnregisterBlackList = new UnregisterBlackListEvent();
        
        [SerializeField]
        public FollowEvent onFollow = new FollowEvent();
        
        [SerializeField]
        public UnfollowEvent onUnfollow = new UnfollowEvent();
        
        [SerializeField]
        public DescribeFollowUsersEvent onDescribeFollowUsers = new DescribeFollowUsersEvent();
        
        [SerializeField]
        public ErrorEvent onError = new ErrorEvent();
    }
}