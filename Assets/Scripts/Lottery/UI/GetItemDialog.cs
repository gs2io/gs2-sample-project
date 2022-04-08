using UnityEngine;
using TMPro;

namespace Gs2.Sample.Lottery
{
    /// <summary>
    /// 抽選して入手したアイテムの表示
    /// </summary>
    public class GetItemDialog : MonoBehaviour
    {
        public TextMeshProUGUI itemName;
        
        public void OnOpenEvent()
        {
            gameObject.SetActive(true);
        }
        
        public void OnCloseEvent()
        {
            gameObject.SetActive(false);
        }
        
        public void SetText(string text)
        {
            itemName.SetText(text);
        }
    }
}