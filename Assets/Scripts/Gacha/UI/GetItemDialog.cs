using System.Collections.Generic;
using Gs2.Gs2Inventory.Request;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gs2.Sample.Gacha
{
    /// <summary>
    /// ガチャを回して入手したアイテムの表示
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