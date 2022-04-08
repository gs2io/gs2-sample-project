using System.Collections.Generic;
using Gs2.Sample.Realtime;
using Gs2.Unity.Gs2Showcase.Model;
using UnityEngine;
using UnityEngine.UI;

namespace Gs2.Sample.Lottery
{
    public class LotteryStoreView : MonoBehaviour
    {
        /// <summary>
        /// ストア表示する際に商品を並べる親
        /// </summary>
        public Transform contentTransform;

        /// <summary>
        /// 商品項目 クローン元のGameObject
        /// </summary>
        /// <returns></returns>
        public LotteryItemView productPrefab;

        [SerializeField]
        public BuyEvent onBuy = new BuyEvent();

        [SerializeField]
        public CloseEvent onClose = new CloseEvent();
        
        public void OnClickBuyButton(SalesItem salesItem)
        {
            onBuy.Invoke(salesItem);
            
            gameObject.SetActive(false);
        }

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