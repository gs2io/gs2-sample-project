using System;
using System.Collections;
using System.Collections.Generic;
using Gs2.Core.Util;
using Gs2.Unity.Gs2Stamina.Model;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Money
{
    public class MoneyStoreView : MonoBehaviour
    {
        /// <summary>
        /// ストア表示する際に商品を並べる親
        /// </summary>
        public GameObject productsContent;

        /// <summary>
        /// 商品項目 クローン元のGameObject
        /// </summary>
        /// <returns></returns>
        public GameObject productPrefab;
        
        public void OnOpenEvent()
        {
            gameObject.SetActive(true);
        }
        
        public void OnCloseEvent()
        {
            gameObject.SetActive(false);
        }
    }
}
