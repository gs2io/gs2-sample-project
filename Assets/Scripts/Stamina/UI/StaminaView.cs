using Gs2.Unity.Gs2Stamina.Model;
using TMPro;
using UnityEngine;

namespace Gs2.Sample.Stamina
{
    public class StaminaView : MonoBehaviour
    {
        /// <summary>
        /// スタミナの現在値表示
        /// </summary>
        public TextMeshProUGUI staminaValue;

        /// <summary>
        /// スタミナの次回回復時刻表示
        /// </summary>
        public TextMeshProUGUI recoveryTime;

        public void SetStamina(EzStamina stamina)
        {
            if (staminaValue != null)
            {
                staminaValue.text = $"{stamina.Value:00} / {stamina.MaxValue:00}";
            }
        }

        public void SetStamina(string text)
        {
            if (staminaValue != null)
            {
                staminaValue.text = text;
            }
        }

        public void SetRecoveryTime(string text)
        {
            if (recoveryTime != null)
            {
                recoveryTime.text = text;
            }
        }
    }
}
