using System;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Version
{
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
        public UnityEvent onAcceptTerm = new UnityEvent();
        
        [SerializeField]
        public ErrorEvent onError = new ErrorEvent();
    }
}