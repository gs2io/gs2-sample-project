using TMPro;
using UnityEngine;

namespace Gs2.Sample.Money2
{
    public class Money2View : MonoBehaviour
    {
        /// <summary>
        /// 有償財貨の現在値表示
        /// </summary>
        public TextMeshProUGUI Value;

        public void SetMoney(int value)
        {
            if (Value != null)
            {
                Value.text = $"{value}";
            }
        }
    }
}
