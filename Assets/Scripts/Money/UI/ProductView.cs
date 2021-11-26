using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Gs2.Sample.Money
{
    public class ProductView : MonoBehaviour
    {
        public TextMeshProUGUI gemsText;

        public TextMeshProUGUI priceText;

        public Button buyButton;

        public TextMeshProUGUI buyButtonLabel;

        public void Initialize(Product product, UnityAction onClick)
        {
            gemsText.text = gemsText.text.Replace("{gems_count}", product.CurrencyCount.ToString()) ;
            priceText.text = priceText.text.Replace("{price}", product.Price.ToString());

            if (product.BoughtLimit != null)
            {
                gemsText.text += " (" + product.BoughtCount + "/" + product.BoughtLimit + ")";
                if (product.BoughtCount == product.BoughtLimit)
                {
                    buyButtonLabel.text = "Sold";
                }
            }
            buyButton.onClick.AddListener(onClick);
        }
    }
}