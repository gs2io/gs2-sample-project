using System;
using System.Collections.Generic;
using Gs2.Unity.Gs2Version.Model;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Version
{
    [System.Serializable]
    public class CheckVersionEvent : UnityEvent<string, List<EzStatus>, List<EzStatus>>
    {
    }
    
    [Serializable]
    public class VersionSetting : MonoBehaviour
    {
        [SerializeField]
        public string versionNamespaceName;
        [SerializeField]
        public string versionName;

        [SerializeField]
        public int currentVersionMajor;
        [SerializeField]
        public int currentVersionMinor;
        [SerializeField]
        public int currentVersionMicro;

        [SerializeField]
        public CheckVersionEvent onCheckVersion = new CheckVersionEvent();
        
        [SerializeField]
        public ErrorEvent onError = new ErrorEvent();
    }
}