using TMPro;
using UnityEngine;

namespace Gs2.Sample.Lottery
{
    public class LotteryItemView : MonoBehaviour
    {
        [SerializeField]
        public TextMeshProUGUI lotteryText;
        [SerializeField]
        public TextMeshProUGUI gemsText;

        [SerializeField]
        public BuyEvent onBuy = new BuyEvent();
        
        public SalesItem salesItem;
        
        public void Initialize(SalesItem _salesItem)
        {
            salesItem = _salesItem;
            
            gemsText.text = gemsText.text.Replace("{price}", _salesItem.Price.ToString());
            lotteryText.text = lotteryText.text.Replace("{draw_count}", salesItem.LotteryCount.ToString()) ;
        }
        
        public void OnClickBuyButton()
        {
            onBuy.Invoke(salesItem);
        }
    }
}