using TMPro;
using UnityEngine;

namespace Gs2.Sample.Gold
{
    public class GoldView : MonoBehaviour
    {
        /// <summary>
        /// スタミナの現在値表示
        /// </summary>
        public TextMeshProUGUI goldValue;

        public void SetGold(long gold)
        {
            if (goldValue != null)
            {
                goldValue.text = $"{gold}";
            }
        }
    }
}
