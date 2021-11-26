using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gs2.Core;
using Gs2.Core.Exception;
using Gs2.Gs2Limit.Request;
using Gs2.Gs2Money.Request;
using Gs2.Sample.Core;
using Gs2.Sample.Core.Runtime;
using Gs2.Unity.Gs2Limit.Result;
using Gs2.Unity.Gs2Showcase.Model;
using Gs2.Unity.Gs2Showcase.Result;
using Gs2.Unity.Util;
using Gs2.Util.LitJson;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Gs2.Sample.Money
{
    public class MoneyStorePresenter : MonoBehaviour
    {
        [SerializeField] public MoneySetting _moneySetting;

        [SerializeField] private MoneyModel _moneyModel;
        [SerializeField] private MoneyStoreView _moneyStoreView;

        [SerializeField] private MoneyPresenter _moneyPresenter;
        
        /// <summary>
        /// Gs2Client
        /// </summary>
        private Gs2Client _gs2Client;
        /// <summary>
        /// Gs2GameSession
        /// </summary>
        private Gs2GameSession _session;
        
        public enum State
        {
            MainMenu,
            
            GetProductsProcessing,
            OpenMoneyStore,
            GetProductsFailed,
            
            SelectProduct,
            
            BuyProcessing,
            BuySucceed,
            BuyFailed,
        }
        
        /// <summary>
        /// 現在のステータス
        /// </summary>
        private State _moneyStoreState = State.MainMenu;

        private void Validate()
        {
            if (_gs2Client == null)
            {
                _gs2Client = GameManager.Instance.Cllient;
            }
            if (_session == null)
            {
                _session = GameManager.Instance.Session;
            }
        }
        
        private void Start()
        {
            Assert.IsNotNull(_moneySetting);
            Assert.IsNotNull(_moneyModel);
            
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
                    
                    case State.SelectProduct:
                        break;

                    case State.BuyProcessing:
                        UIManager.Instance.OpenProcessing();
                        break;
                    
                    case State.BuySucceed:
                        UIManager.Instance.CloseProcessing();
                        UIManager.Instance.OpenDialog1("Notice","商品を購入しました。");
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

        public void ClickToOpenMoneyStore()
        {
            Validate();
            
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
                    _gs2Client.Client,
                    _session.Session,
                    _moneySetting.showcaseNamespaceName,
                    _moneySetting.showcaseName,
                    _moneySetting.onGetProducts,
                    _moneySetting.onError
                )
            );
        }



        /// <summary>
        /// 購入する
        /// </summary>
        public void ClickToBuy(Product product)
        {
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
            Validate();
            
            yield return _moneyModel.Buy(
                r =>
                {
                    SetState(r.Error == null
                        ? State.BuySucceed
                        : State.BuyFailed);
                },
                _gs2Client.Client,
                _session.Session,
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