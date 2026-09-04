using Gs2.Unity.Gs2Experience.Model;
using Gs2.Unity.Gs2Inventory.Model;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Gs2.Sample.Enhance
{
    /// <summary>
    /// 強化を実行するときに呼び出されます（強化対象・使用する素材の数）
    /// Called when the enhancement is executed (the target and the number of materials to use)
    /// </summary>
    public class ExecuteEnhanceEvent : UnityEvent<EzItemSet, int>
    {
    }

    /// <summary>
    /// ランクを上げるためのダイアログ
    /// Dialog used to raise the rank
    /// </summary>
    public class EnhanceDialog : MonoBehaviour
    {
        [SerializeField]
        public TextMeshProUGUI characterName;

        [SerializeField]
        public TextMeshProUGUI rank;

        [SerializeField]
        public TextMeshProUGUI experience;

        /// <summary>
        /// 使用する素材の数
        /// Number of materials to use
        /// </summary>
        [SerializeField]
        public TextMeshProUGUI useCount;

        /// <summary>
        /// 強化で加算される経験値
        /// Experience that will be added by the enhancement
        /// </summary>
        [SerializeField]
        public TextMeshProUGUI gainExperience;

        [SerializeField]
        public Button decreaseButton;

        [SerializeField]
        public Button increaseButton;

        [SerializeField]
        public Button executeButton;

        public ExecuteEnhanceEvent onExecuteEnhance = new ExecuteEnhanceEvent();

        private EzItemSet _target;
        private int _materialExperience;
        private int _ownedMaterialCount;
        private int _useCount;

        /// <summary>
        /// ダイアログを開く
        /// Open the dialog
        /// </summary>
        public void Open(
            EzItemSet target,
            EzStatus status,
            int ownedMaterialCount,
            int materialExperience,
            long firstRankUpExperience
        )
        {
            _target = target;
            _ownedMaterialCount = ownedMaterialCount;
            _materialExperience = materialExperience;
            _useCount = ownedMaterialCount > 0 ? 1 : 0;

            characterName.SetText(target.ItemName);
            rank.SetText(
                status == null
                    ? "Rank 1"
                    : $"Rank {status.RankValue}"
            );
            experience.SetText(
                status == null
                    ? $"0 / {firstRankUpExperience}"
                    : $"{status.ExperienceValue} / {status.NextRankUpExperienceValue}"
            );

            Refresh();

            gameObject.SetActive(true);
        }

        public void OnClickDecrease()
        {
            if (_useCount > 1)
            {
                _useCount--;
                Refresh();
            }
        }

        public void OnClickIncrease()
        {
            if (_useCount < _ownedMaterialCount)
            {
                _useCount++;
                Refresh();
            }
        }

        public void OnClickExecute()
        {
            if (_useCount <= 0)
            {
                return;
            }

            onExecuteEnhance.Invoke(_target, _useCount);
        }

        public void OnClickCancel()
        {
            Close();
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        private void Refresh()
        {
            useCount.SetText($"{_useCount} / {_ownedMaterialCount}");
            gainExperience.SetText($"+{_useCount * _materialExperience}");

            if (decreaseButton != null)
            {
                decreaseButton.interactable = _useCount > 1;
            }
            if (increaseButton != null)
            {
                increaseButton.interactable = _useCount < _ownedMaterialCount;
            }
            if (executeButton != null)
            {
                executeButton.interactable = _useCount > 0;
            }
        }
    }
}
