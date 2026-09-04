using Gs2.Unity.Gs2Experience.Model;
using Gs2.Unity.Gs2Inventory.Model;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Gs2.Sample.Enhance
{
    public class ClickCharacterEvent : UnityEvent<EzItemSet>
    {
    }

    /// <summary>
    /// 強化対象（キャラクター）の一覧の1行
    /// One row of the list of enhancement targets (characters)
    /// </summary>
    public class CharacterItem : MonoBehaviour
    {
        public Image image;
        public TextMeshProUGUI characterName;
        public TextMeshProUGUI rank;
        public TextMeshProUGUI experience;

        public ClickCharacterEvent onClickCharacter = new ClickCharacterEvent();

        private EzItemSet _itemSet;
        private EzStatus _status;
        private long _firstRankUpExperience;

        public void Initialize(
            EzItemSet itemSet,
            EzStatus status,
            long firstRankUpExperience
        )
        {
            _itemSet = itemSet;
            _status = status;
            _firstRankUpExperience = firstRankUpExperience;
        }

        public void Start()
        {
            characterName.SetText(_itemSet.ItemName);

            // ステータスは強化を1度も行っていない場合には存在しない
            // The status does not exist until the target has been enhanced at least once
            rank.SetText(
                _status == null
                    ? "Rank 1"
                    : $"Rank {_status.RankValue}"
            );

            experience.SetText(
                _status == null
                    ? $"0 / {_firstRankUpExperience}"
                    : $"{_status.ExperienceValue} / {_status.NextRankUpExperienceValue}"
            );
        }

        public void OnClick()
        {
            onClickCharacter.Invoke(
                _itemSet
            );
        }
    }
}
