using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gs2.Sample.Lottery
{
    public class LotteryItemView : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI lotteryText;
        [SerializeField]
        private TextMeshProUGUI gemsText;

        [SerializeField]
        public BuyEvent onBuy = new BuyEvent();
        [SerializeField]
        private Button buyButton;
        
        private SalesItem salesItem;
        
        public void Initialize(SalesItem _salesItem, long balance)
        {
            salesItem = _salesItem;
            
            gemsText.text = gemsText.text.Replace("{price}", _salesItem.Price.ToString());
            lotteryText.text = lotteryText.text.Replace("{draw_count}", salesItem.LotteryCount.ToString()) ;

            var price = int.Parse(_salesItem.Price);

            if (balance < price)
            {
                buyButton.interactable = false;
            }
            else
            {
                buyButton.interactable = true;
            }
        }
        
        public void OnClickBuyButton()
        {
            onBuy.Invoke(salesItem);
        }
    }
}