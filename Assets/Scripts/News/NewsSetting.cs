using System;
using System.Collections.Generic;
using Gs2.Unity.Gs2News.Model;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.News
{
    [System.Serializable]
    public class GetContentsUrlEvent : UnityEvent<List<EzSetCookieRequestEntry>, string, string>
    {
    }
    
    [System.Serializable]
    public class GetListNewsesEvent : UnityEvent<List<EzNews>, string, string>
    {
    }
    
    [Serializable]
    public class NewsSetting : MonoBehaviour
    {
        [SerializeField]
        public string newsNamespaceName;

        [SerializeField]
        public GetContentsUrlEvent onGetContentsUrl = new GetContentsUrlEvent();

        [SerializeField]
        public GetListNewsesEvent onGetListNewses = new GetListNewsesEvent();

        [SerializeField]
        public ErrorEvent onError = new ErrorEvent();
    }
}