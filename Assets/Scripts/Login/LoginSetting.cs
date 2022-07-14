using System;
using Gs2.Unity.Util;
using Gs2.Unity.Gs2Account.Model;
using UnityEngine;
using UnityEngine.Events;

 namespace Gs2.Sample.Login
{
    [System.Serializable]
    public class LoadAccountEvent : UnityEvent<EzAccount>
    {
    }

    [System.Serializable]
    public class SaveAccountEvent : UnityEvent<EzAccount>
    {
    }

    [System.Serializable]
    public class CreateAccountEvent : UnityEvent<EzAccount>
    {
    }
    
    [System.Serializable]
    public class LoginEvent : UnityEvent<GameSession>
    {
    }
    
    [Serializable]
    public class LoginSetting : MonoBehaviour
    {
        /// <summary>
        /// GS2-Account のネームスペース名
        /// </summary>
        [SerializeField]
        public string accountNamespaceName;

        /// <summary>
        /// GS2-Account でアカウント情報の暗号化に使用する GS2-Key の暗号鍵GRN
        /// </summary>
        [SerializeField]
        public string accountEncryptionKeyId;
        
        /// <summary>
        /// GS2-Gateway のネームスペース名
        /// </summary>
        [SerializeField]
        public string gatewayNamespaceName;
        
        /// <summary>
        /// アカウント作成時に発行されるイベント
        /// </summary>
        [SerializeField]
        public CreateAccountEvent onCreateAccount = new CreateAccountEvent();

        /// <summary>
        /// アカウントロード時に発行されるイベント
        /// </summary>
        [SerializeField]
        public LoadAccountEvent onLoadAccount = new LoadAccountEvent();

        /// <summary>
        /// アカウントセーブ時に発行されるイベント
        /// </summary>
        [SerializeField]
        public SaveAccountEvent onSaveAccount = new SaveAccountEvent();
        
        /// <summary>
        /// ログイン時に発行されるイベント
        /// </summary>
        [SerializeField]
        public LoginEvent onLogin = new LoginEvent();

        /// <summary>
        /// エラー発生時に発行されるイベント
        /// </summary>
        [SerializeField]
        public ErrorEvent onError = new ErrorEvent();
    }
}