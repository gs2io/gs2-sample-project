using System;
using System.Collections.Generic;
using Gs2.Unity.Gs2Experience.Model;
using Gs2.Unity.Gs2Inventory.Model;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Enhance
{
    /// <summary>
    /// 強化対象／強化素材のアイテムセットを取得したときに呼び出されます
    /// Called when the item sets of the enhancement targets / materials are retrieved
    /// </summary>
    [Serializable]
    public class GetItemSetsEvent : UnityEvent<string, List<EzItemSet>>
    {
    }

    /// <summary>
    /// 強化対象のステータス（ランク・経験値）を取得したときに呼び出されます
    /// Called when the statuses (rank and experience) of the enhancement targets are retrieved
    /// </summary>
    [Serializable]
    public class GetEnhanceStatusesEvent : UnityEvent<List<EzStatus>>
    {
    }

    /// <summary>
    /// 強化が完了したときに呼び出されます
    /// Called when the enhancement has been completed
    /// </summary>
    [Serializable]
    public class EnhanceEvent : UnityEvent<EzItemSet, int>
    {
    }

    /// <summary>
    /// 動作確認用にキャラクター／素材を入手したときに呼び出されます
    /// Called when a character / material has been obtained for verification purposes
    /// </summary>
    [Serializable]
    public class AcquireItemEvent : UnityEvent<string>
    {
    }

    [Serializable]
    public class EnhanceSetting : MonoBehaviour
    {
        /// <summary>
        /// GS2-Enhance のネームスペース名
        /// Namespace name of GS2-Enhance
        /// </summary>
        [SerializeField]
        public string enhanceNamespaceName;

        /// <summary>
        /// GS2-Enhance の強化レート名
        /// Rate model name of GS2-Enhance
        /// </summary>
        [SerializeField]
        public string enhanceRateName;

        /// <summary>
        /// GS2-Inventory のネームスペース名
        /// Namespace name of GS2-Inventory
        /// </summary>
        [SerializeField]
        public string inventoryNamespaceName;

        /// <summary>
        /// 強化対象のインベントリモデル名
        /// Inventory model name of the enhancement targets
        /// </summary>
        [SerializeField]
        public string characterInventoryModelName;

        /// <summary>
        /// 強化素材のインベントリモデル名
        /// Inventory model name of the enhancement materials
        /// </summary>
        [SerializeField]
        public string materialInventoryModelName;

        /// <summary>
        /// GS2-Experience のネームスペース名
        /// Namespace name of GS2-Experience
        /// </summary>
        [SerializeField]
        public string experienceNamespaceName;

        /// <summary>
        /// 強化対象の経験値モデル名
        /// Experience model name of the enhancement targets
        /// </summary>
        [SerializeField]
        public string experienceModelName;

        /// <summary>
        /// GS2-Exchange のネームスペース名（動作確認用の入手処理に使用）
        /// Namespace name of GS2-Exchange (used to obtain items for verification purposes)
        /// </summary>
        [SerializeField]
        public string exchangeNamespaceName;

        /// <summary>
        /// 強化対象を入手する交換レート名
        /// Exchange rate name that grants an enhancement target
        /// </summary>
        [SerializeField]
        public string exchangeRateNameGetCharacter;

        /// <summary>
        /// 強化素材を入手する交換レート名
        /// Exchange rate name that grants enhancement materials
        /// </summary>
        [SerializeField]
        public string exchangeRateNameGetMaterial;

        [SerializeField]
        public GetItemSetsEvent onGetItemSets = new GetItemSetsEvent();

        [SerializeField]
        public GetEnhanceStatusesEvent onGetStatuses = new GetEnhanceStatusesEvent();

        [SerializeField]
        public EnhanceEvent onEnhance = new EnhanceEvent();

        [SerializeField]
        public AcquireItemEvent onAcquireItem = new AcquireItemEvent();

        [SerializeField]
        public ErrorEvent onError = new ErrorEvent();
    }
}
