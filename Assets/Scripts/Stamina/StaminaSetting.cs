using Gs2.Unity.Util;
using Gs2.Unity.Gs2Stamina.Model;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Stamina
{
    [System.Serializable]
    public class GetStaminaModelEvent : UnityEvent<string, EzStaminaModel>
    {
    }

    [System.Serializable]
    public class GetStaminaEvent : UnityEvent<EzStaminaModel, EzStamina>
    {
    }

    [System.Serializable]
    public class RecoverStaminaEvent : UnityEvent<EzStaminaModel, EzStamina, int>
    {
    }

    [System.Serializable]
    public class ConsumeStaminaEvent : UnityEvent<EzStaminaModel, EzStamina, int>
    {
    }
    
    [System.Serializable]
    public class StaminaBuyEvent : UnityEvent
    {
    }

    [System.Serializable]
    public class StaminaSetting : MonoBehaviour
    {
        [SerializeField]
        public string staminaNamespaceName;

        [SerializeField]
        public string staminaModelName;

        [SerializeField] 
        public string staminaName;
        
        [SerializeField] 
        public string exchangeNamespaceName;

        [SerializeField] 
        public string exchangeRateName;

        [SerializeField] 
        public string exchangeKeyId;
        
        [SerializeField] 
        public string distributorNamespaceName;
        
        [SerializeField]
        public GetStaminaModelEvent onGetStaminaModel = new GetStaminaModelEvent();

        [SerializeField]
        public RecoverStaminaEvent onRecoverStamina = new RecoverStaminaEvent();

        [SerializeField]
        public ConsumeStaminaEvent onConsumeStamina = new ConsumeStaminaEvent();
        
        /// <summary>
        /// スタミナを取得したとき
        /// </summary>
        [SerializeField]
        public GetStaminaEvent onGetStamina = new GetStaminaEvent();
        
        /// <summary>
        /// スタミナを購入したとき
        /// </summary>
        [SerializeField]
        public StaminaBuyEvent onBuy = new StaminaBuyEvent();

        /// <summary>
        /// エラー発生時に発行されるイベント
        /// </summary>
        [SerializeField]
        public ErrorEvent onError = new ErrorEvent();
    }
}