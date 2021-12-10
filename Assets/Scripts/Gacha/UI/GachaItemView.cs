using TMPro;
using UnityEngine;

namespace Gs2.Sample.Gacha
{
    public class GachaItemView : MonoBehaviour
    {
        [SerializeField]
        public TextMeshProUGUI gachaText;
        [SerializeField]
        public TextMeshProUGUI gemsText;

        [SerializeField]
        public BuyEvent onBuy = new BuyEvent();
        
        public SalesItem salesItem;
        
        public void Initialize(SalesItem _salesItem)
        {
            salesItem = _salesItem;
            gemsText.SetText(_salesItem.Price + " Gems") ;
            gachaText.SetText(_salesItem.LotteryCount + "回 まわす");
        }
        
        public void OnClickBuyButton()
        {
            onBuy.Invoke(salesItem);
        }
    }
}