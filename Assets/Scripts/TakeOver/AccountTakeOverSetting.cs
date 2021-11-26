using Gs2.Core.Exception;
using Gs2.Unity.Gs2Account.Model;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.AccountTakeOver
{
    [System.Serializable]
    public class SetTakeOverEvent : UnityEvent<EzTakeOver>
    {
    }

    [System.Serializable]
    public class DoTakeOverEvent : UnityEvent<EzAccount>
    {
    }

    [System.Serializable]
    public class ErrorEvent : UnityEvent<Gs2Exception>
    {
    }

    public class AccountTakeOverSetting : MonoBehaviour
    {
        /// <summary>
        /// 引継ぎ設定を追加したとき
        /// </summary>
        [SerializeField]
        public SetTakeOverEvent onSetTakeOver = new SetTakeOverEvent();
        
        /// <summary>
        /// 引継ぎを実行したとき
        /// </summary>
        [SerializeField]
        public DoTakeOverEvent onDoTakeOver = new DoTakeOverEvent();

        /// <summary>
        /// エラー発生時に発行されるイベント
        /// </summary>
        [SerializeField]
        public ErrorEvent onError = new ErrorEvent();

    }
}