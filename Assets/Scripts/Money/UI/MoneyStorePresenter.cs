using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Gs2.Sample.Money
{
    public class MoneyStorePresenter : MonoBehaviour
    {
        [SerializeField]
        public MoneySetting _moneySetting;

        [SerializeField]
        private MoneyModel _moneyModel;
        [SerializeField]
        private MoneyStoreView _moneyStoreView;

        [SerializeField]
        private MoneyPresenter _moneyPresenter;
        
        public enum State
        {
            MainMenu,
            
            /// <summary>
            /// 商品情報を取得中
            /// </summary>
            GetProductsProcessing,
            /// <summary>
            /// 商品情報の取得に失敗
            /// </summary>
            GetProductsFailed,
            
            /// <summary>
            /// 課金通貨ストアを開く
            /// </summary>
            OpenMoneyStore,
            
            /// <summary>
            /// 課金通貨商品を購入
            /// </summary>     
            BuyProcessing,
            /// <summary>
            /// 課金通貨商品の購入に成功
            /// </summary>
            BuySucceed,
            /// <summary>
            /// 課金通貨商品の購入に失敗
            /// </summary>
            BuyFailed,
        }
        
        /// <summary>
        /// 現在のステート
        /// </summary>
        private State _moneyStoreState = State.MainMenu;

        private void Start()
        {
            Assert.IsNotNull(_moneySetting);
            Assert.IsNotNull(_moneyModel);
            Assert.IsNotNull(_moneyStoreView);
            Assert.IsNotNull(_moneyPresenter);
            
            _moneyStoreView.OnCloseEvent();
        }

        private void SetState(State _state)
        {
            if (_moneyStoreState != _state)
            {
                switch (_state)
                {
                    default:
                        _moneyStoreView.OnCloseEvent();
                        break;
                    case State.GetProductsProcessing:
                        _moneyStoreView.OnCloseEvent();
                        UIManager.Instance.OpenProcessing();
                        break;
                    case State.OpenMoneyStore:
                        UIManager.Instance.CloseProcessing();
                        _moneyStoreView.OnOpenEvent();
                        break;
                    
                    case State.BuyProcessing:
                        UIManager.Instance.OpenProcessing();
                        break;
                    
                    case State.BuySucceed:
                        UIManager.Instance.CloseProcessing();
                        
                        UIManager.Instance.OpenDialog1("Notice", "ProductPurchase");

                        _moneyPresenter.OnUpdateWallet();
                        break;
                    
                    case State.BuyFailed:
                        UIManager.Instance.CloseProcessing();
                        break;
                }
            }
            _moneyStoreState = _state;
        }

        /// <summary>
        /// 課金通貨商品ストアを開く
        /// </summary>
        public void ClickToOpenMoneyStore()
        {
            SetState(State.GetProductsProcessing);
            
            StartCoroutine(
                _moneyModel.GetListProducts(
                    r =>
                    {
                        _moneyModel.products = r.Result;

                        if (r.Error == null)
                        {
                            OnGetProducts(_moneyModel.products);
                            
                            SetState(State.OpenMoneyStore);
                        }
                        else
                        {
                            SetState(State.GetProductsFailed);
                        }
                    },
                    GameManager.Instance.Cllient.Client,
                    GameManager.Instance.Session.Session,
                    _moneySetting.showcaseNamespaceName,
                    _moneySetting.showcaseName,
                    _moneySetting.onGetProducts,
                    _moneySetting.onError
                )
            );
        }
        
        /// <summary>
        /// 商品リストの初期化
        /// </summary>
        /// <param name="products"></param>
        private void OnGetProducts(List<Product> products)
        {
            _moneyModel.products = products;
            
            _moneyStoreView.productPrefab.SetActive(false);
            
            if (_moneyStoreView.productsContent != null)
            {
                foreach (Transform child in _moneyStoreView.productsContent.transform)
                {
                    if (child != null && child.gameObject != _moneyStoreView.productPrefab)
                    {
                        Destroy(child.gameObject);
                    }
                }
                
                foreach (var product in products)
                {
                    var productObject = Instantiate<GameObject>(_moneyStoreView.productPrefab,
                        _moneyStoreView.productsContent.transform);
                    var productView = productObject.GetComponent<ProductView>();
                    productView.Initialize(product,
                        () =>
                        {
                            ClickToBuy(product);
                        }
                    );
                    productObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// 購入する
        /// </summary>
        public void ClickToBuy(Product product)
        {
            UIManager.Instance.AddLog("MoneyStorePresenter::ClickToBuy");
            
            _moneyModel.selectedProduct = product;
            SetState(State.BuyProcessing);

            StartCoroutine(
                BuyTask()
            );
        }
        
        /// <summary>
        /// 課金通貨を購入
        /// </summary>
        /// <returns></returns>
        private IEnumerator BuyTask()
        {
            yield return _moneyModel.Buy(
                r =>
                {
                    SetState(r.Error == null
                        ? State.BuySucceed
                        : State.BuyFailed);
                },
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                _moneySetting.showcaseNamespaceName,
                _moneySetting.showcaseName,
                _moneySetting.distributorNamespaceName,
                _moneySetting.showcaseKeyId,
                _moneySetting.onBuy,
                _moneySetting.onError
            );

            _moneySetting.onBuy.Invoke(_moneyModel.selectedProduct);
        }

        public void ClickToClose()
        {
            SetState(State.MainMenu);
        }
    }
}