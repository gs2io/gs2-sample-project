using System;
using Gs2.Unity.Gs2Version.Model;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Version
{
    [System.Serializable]
    public class AcceptTermEvent : UnityEvent<EzAcceptVersion>
    {
    }
    
    [Serializable]
    public class TermSetting : MonoBehaviour
    {
        [SerializeField]
        public string versionNamespaceName;

        [SerializeField]
        public string versionName;

        [SerializeField]
        public CheckVersionEvent onCheckVersion = new CheckVersionEvent();
        
        [SerializeField]
        public AcceptTermEvent onAcceptTerm = new AcceptTermEvent();
        
        [SerializeField]
        public ErrorEvent onError = new ErrorEvent();
    }
}