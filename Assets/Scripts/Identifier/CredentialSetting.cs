using System;
using Gs2.Unity.Core;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Credential
{
    [Serializable]
    public class InitializeGs2AccountEvent : UnityEvent<Gs2Domain>
    {
    }

    [Serializable]
    public class CredentialSetting : MonoBehaviour
    {
        [SerializeField]
        public string applicationClientId;
        
        [SerializeField]
        public string applicationClientSecret;

        [SerializeField]
        public string distributorNamespaceName;
        
        [SerializeField]
        public InitializeGs2AccountEvent onInitializeGs2 = new InitializeGs2AccountEvent();

        [SerializeField]
        public ErrorEvent onError = new ErrorEvent();
    }
}