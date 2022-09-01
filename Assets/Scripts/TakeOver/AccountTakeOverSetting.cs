using Gs2.Core.Exception;
using Gs2.Unity.Gs2Account.Model;
using Gs2.Unity.Util;
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

    public class AccountTakeOverSetting : MonoBehaviour
    {
        /// <summary>
        /// 引継ぎ設定を追加したとき
        /// When the takeover setting is added
        /// </summary>
        [SerializeField]
        public SetTakeOverEvent onSetTakeOver = new SetTakeOverEvent();
        
        /// <summary>
        /// 引継ぎを実行したとき
        /// When the handover is executed
        /// </summary>
        [SerializeField]
        public DoTakeOverEvent onDoTakeOver = new DoTakeOverEvent();

        /// <summary>
        /// エラー発生時に発行されるイベント
        /// Event issued when an error occurs
        /// </summary>
        [SerializeField]
        public ErrorEvent onError = new ErrorEvent();

    }
}