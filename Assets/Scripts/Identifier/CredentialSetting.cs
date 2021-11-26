using System;
using Gs2.Unity;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Credential
{
    [Serializable]
    public class InitializeGs2AccountEvent : UnityEvent<Profile, Client>
    {
    }

    [Serializable]
    public class FinalizeGs2AccountEvent : UnityEvent<Profile>
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
        public InitializeGs2AccountEvent onInitializeGs2 = new InitializeGs2AccountEvent();

        [SerializeField]
        public FinalizeGs2AccountEvent onFinalizeGs2 = new FinalizeGs2AccountEvent();

        [SerializeField]
        public ErrorEvent onError = new ErrorEvent();
    }
}