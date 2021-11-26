using System;
using UnityEngine;

namespace Gs2.Sample.Login
{
    [Serializable]
    public class PersistAccount
    {
        [SerializeField]
        public string UserId;
        [SerializeField]
        public string Password;
    }
}