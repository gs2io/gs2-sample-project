using Gs2.Unity.Gs2Experience.Model;
using TMPro;
using UnityEngine;

namespace Gs2.Sample.Experience
{
    public class ExperienceView : MonoBehaviour
    {
        /// <summary>
        /// ランクの現在値表示
        /// </summary>
        public TextMeshProUGUI rankValue;

        /// <summary>
        /// 経験値の現在値表示
        /// </summary>
        public TextMeshProUGUI experienceValue;

        public void SetRank(EzStatus status)
        {
            if (rankValue != null)
            {
                rankValue.SetText(status.RankValue.ToString());
            }
        }

        public void SetRank(long RankValue)
        {
            if (rankValue != null)
            {
                rankValue.SetText(RankValue.ToString());
            }
        }

        public void SetExperience(EzStatus status, long nextRankExperience)
        {
            if (experienceValue != null)
            {
                experienceValue.SetText($"{status.ExperienceValue} / {nextRankExperience}");
            }
        }

        public void SetExperience(long experience, long nextRankExperience)
        {
            if (experienceValue != null)
            {
                experienceValue.SetText($"{experience} / {nextRankExperience}");
            }
        }
    }
}
