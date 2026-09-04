using System;
using System.Collections.Generic;
using Gs2.Unity.Gs2Dictionary.Model;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Dictionary
{
    /// <summary>
    /// 図鑑のエントリーモデル（定義）を取得したときに呼び出されます
    /// Called when the entry models (definitions) of the dictionary are retrieved
    /// </summary>
    [Serializable]
    public class GetEntryModelsEvent : UnityEvent<List<EzEntryModel>>
    {
    }

    /// <summary>
    /// 取得済みの図鑑エントリーを取得したときに呼び出されます
    /// Called when the registered dictionary entries are retrieved
    /// </summary>
    [Serializable]
    public class GetEntriesEvent : UnityEvent<List<EzEntry>>
    {
    }

    /// <summary>
    /// 図鑑エントリーを登録したときに呼び出されます
    /// Called when a dictionary entry has been registered
    /// </summary>
    [Serializable]
    public class RegisterEntryEvent : UnityEvent<string>
    {
    }

    /// <summary>
    /// 図鑑エントリーをリセットしたときに呼び出されます
    /// Called when the dictionary entries have been reset
    /// </summary>
    [Serializable]
    public class ResetEntriesEvent : UnityEvent
    {
    }

    [Serializable]
    public class DictionarySetting : MonoBehaviour
    {
        /// <summary>
        /// GS2-Dictionary のネームスペース名
        /// Namespace name of GS2-Dictionary
        /// </summary>
        [SerializeField]
        public string dictionaryNamespaceName;

        /// <summary>
        /// GS2-Exchange のネームスペース名
        /// Namespace name of GS2-Exchange
        /// </summary>
        [SerializeField]
        public string exchangeNamespaceName;

        /// <summary>
        /// 図鑑エントリーを登録する交換レート名
        /// Exchange rate name that registers a dictionary entry
        /// </summary>
        [SerializeField]
        public string exchangeRateNameRegister;

        /// <summary>
        /// 図鑑エントリーを削除する交換レート名
        /// Exchange rate name that deletes a dictionary entry
        /// </summary>
        [SerializeField]
        public string exchangeRateNameReset;

        [SerializeField]
        public GetEntryModelsEvent onGetEntryModels = new GetEntryModelsEvent();

        [SerializeField]
        public GetEntriesEvent onGetEntries = new GetEntriesEvent();

        [SerializeField]
        public RegisterEntryEvent onRegisterEntry = new RegisterEntryEvent();

        [SerializeField]
        public ResetEntriesEvent onResetEntries = new ResetEntriesEvent();

        [SerializeField]
        public ErrorEvent onError = new ErrorEvent();
    }
}
