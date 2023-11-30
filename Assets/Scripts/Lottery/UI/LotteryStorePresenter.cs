using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gs2.Core.Exception;
using Gs2.Gs2Inventory.Request;
using Gs2.Sample.Core;
using Gs2.Sample.Money;
using Gs2.Sample.Unit;
using Gs2.Unity.Gs2Lottery.Model;
using Gs2.Unity.Gs2Showcase.Model;
using Gs2.Util.LitJson;
using UnityEngine;
using UnityEngine.Assertions;
using EzConfig = Gs2.Unity.Gs2Showcase.Model.EzConfig;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
#endif

namespace Gs2.Sample.Lottery
{
    public class LotteryStorePresenter : MonoBehaviour
    {
        [SerializeField]
        private LotterySetting _lotterySetting;
        
        [SerializeField]
        public LotteryStoreModel _lotteryStoreModel;
        
        [SerializeField]
        public LotteryStoreView _lotteryStoreView;
        [SerializeField]
        public GetItemDialog _getItemDialog;
        [SerializeField]
        public UnitModel _unitModel;
        
        [SerializeField]
        private MoneyPresenter _moneyPresenter;
        [SerializeField]
        private UnitPresenter _unitPresenter;
        
        public enum State
        {
            MainMenu,
            
            /// <summary>
            /// 商品棚情報を取得中
            /// Retrieving product shelf information
            /// </summary>
            GetShowcaseProcessing,
            /// <summary>
            /// 商品棚情報の取得に失敗
            /// Failure to acquire product shelf information
            /// </summary>
            GetShowcaseFailed,
            
            /// <summary>
            /// ストアを開く
            /// Open Store
            /// </summary>
            OpenLotteryStore,
            
            /// <summary>
            /// 商品を購入
            /// Buy Products
            /// </summary>
            BuyProcessing,
            /// <summary>
            /// 商品の購入に成功
            /// Successful purchase of products
            /// </summary>
            BuySucceed,
            /// <summary>
            /// 商品の購入に失敗
            /// Failure to purchase products
            /// </summary>
            BuyFailed,
        }
        
        /// <summary>
        /// 現在のステート
        /// Current State
        /// </summary>
        private State _lotteryStoreState = State.MainMenu;
        
        public void Start()
        {
            Assert.IsNotNull(_lotterySetting);
            Assert.IsNotNull(_lotteryStoreModel);
            Assert.IsNotNull(_lotteryStoreView);
            Assert.IsNotNull(_getItemDialog);
            Assert.IsNotNull(_unitModel);
            Assert.IsNotNull(_moneyPresenter);
            Assert.IsNotNull(_unitPresenter);
            
            _lotteryStoreView.OnCloseEvent();
        }

        private void SetState(State _state)
        {
            if (_lotteryStoreState != _state)
            {
                switch (_state)
                {
                    default:
                        _lotteryStoreView.OnCloseEvent();
                        break;
                    case State.GetShowcaseProcessing:
                        _lotteryStoreView.OnCloseEvent();
                        UIManager.Instance.OpenProcessing();
                        break;
                    case State.OpenLotteryStore:
                        UIManager.Instance.CloseProcessing();
                        _lotteryStoreView.OnOpenEvent();
                        break;

                    case State.BuyProcessing:
                        UIManager.Instance.OpenProcessing();
                        break;
                    
                    case State.BuySucceed:
                        UIManager.Instance.CloseProcessing();
                        _getItemDialog.OnOpenEvent();
                        _moneyPresenter.OnUpdateWallet();
                        break;
                    
                    case State.BuyFailed:
                        UIManager.Instance.CloseProcessing();
                        break;
                }
            }
            _lotteryStoreState = _state;
        }
        
        public void Initialize()
        {
            UIManager.Instance.AddLog("LotteryStorePresenter::Initialize");
            
            _lotterySetting.onError.AddListener(OnError);
        }
        
        public void Finish()
        {
            _lotterySetting.onError.RemoveListener(OnError);
        }

        public void OnError(Gs2Exception e, Func<IEnumerator> retry)
        {
            SetState(State.BuyFailed);
        }
        
        /// <summary>
        /// 商品ストアを開く
        /// Open Product Store
        /// </summary>
        public void ClickToOpenLotteryStore()
        {
            SetState(State.GetShowcaseProcessing);

#if GS2_ENABLE_UNITASK
            GetProductsTaskAsync().Forget();
#else
            StartCoroutine(
                GetProductsTask()
            );
#endif
        }

        public IEnumerator GetProductsTask()
        {
            yield return _lotteryStoreModel.GetShowcase(
                e =>
                {
                    if (e == null)
                    {
                        OnGetSalesItems(_lotteryStoreModel.Showcase, _lotteryStoreModel.DisplayItems);

                        SetState(State.OpenLotteryStore);
                    }
                    else
                    {
                        SetState(State.GetShowcaseFailed);
                    }
                },
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _lotterySetting.showcaseNamespaceName,
                _lotterySetting.lotteryName,
                _lotterySetting.onGetShowcase,
                _lotterySetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        public async UniTask GetProductsTaskAsync()
        {
            var e = await _lotteryStoreModel.GetShowcaseAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _lotterySetting.showcaseNamespaceName,
                _lotterySetting.lotteryName,
                _lotterySetting.onGetShowcase,
                _lotterySetting.onError
            );
            
            if (e == null)
            {
                OnGetSalesItems(_lotteryStoreModel.Showcase, _lotteryStoreModel.DisplayItems);

                SetState(State.OpenLotteryStore);
            }
            else
            {
                SetState(State.GetShowcaseFailed);
            }
        }
#endif
        
        /// <summary>
        /// 商品リストの初期化
        /// Initialization of product list
        /// </summary>
        /// <param name="showcase"></param>
        /// <param name="displayItems"></param>
        private void OnGetSalesItems(EzShowcase showcase, List<EzDisplayItem> displayItems)
        {
            for (var i = 0; i < _lotteryStoreView.contentTransform.childCount; i++)
            {
                var gameObject = _lotteryStoreView.contentTransform.GetChild(i).gameObject;
                if (gameObject == _lotteryStoreView.productPrefab.gameObject)
                    continue;
                
                Destroy(gameObject);
            }

            foreach (var displayItem in displayItems)
            {
                var item = Instantiate(_lotteryStoreView.productPrefab, _lotteryStoreView.contentTransform);
                item.Initialize(new SalesItem(displayItem.DisplayItemId, displayItem.SalesItem), _moneyPresenter.GetWalletBalance());
                item.onBuy.AddListener(OnBuyProduct);
                item.gameObject.SetActive(true);
            }
        }


        /// <summary>
        /// 商品を購入
        /// Buy Products
        /// </summary>
        /// <param name="salesItem"></param>
        public void OnBuyProduct(
            SalesItem salesItem
        )
        {
            UIManager.Instance.AddLog("LotteryStorePresenter::OnBuyProduct");

            _lotteryStoreModel.selectedItem = salesItem;
            SetState(State.BuyProcessing);
            
            var config = new Dictionary<string, string>
            {
                ["slot"] = MoneyModel.Slot.ToString()
            };
            
#if GS2_ENABLE_UNITASK
            _lotteryStoreModel.BuyAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _lotterySetting.showcaseNamespaceName,
                _lotterySetting.lotteryName,
                salesItem.DisplayItemId,
                _lotterySetting.onAcquireInventoryItem,
                _lotterySetting.onError,
                config.Select(item => new EzConfig
                {
                    Key = item.Key,
                    Value = item.Value
                }).ToList()
            ).Forget();
#else
            StartCoroutine(
                _lotteryStoreModel.Buy(
                    GameManager.Instance.Domain,
                    GameManager.Instance.Session,
                    _lotterySetting.showcaseNamespaceName,
                    _lotterySetting.lotteryName,
                    salesItem.DisplayItemId,
                    _lotterySetting.onAcquireInventoryItem,
                    _lotterySetting.onError,
                    config.Select(item => new EzConfig
                    {
                        Key = item.Key,
                        Value = item.Value
                    }).ToList()
                )
            );
#endif
        }
        
        /// <summary>
        /// 抽選で入手したアイテム ダイアログ
        /// </summary>
        /// <param name="requests"></param>
        public void OnAcquireInventoryItem(
            List<EzDrawnPrize> prizes
        )
        {
            if (prizes != null)
            {
                string text = "";
                foreach (var prize in prizes)
                {
                    if (prize.AcquireActions != null && prize.AcquireActions.Count > 0)
                    {
                        foreach (var action in prize.AcquireActions)
                        {
                            var request = action.Request;
                            var item = AcquireItemSetByUserIdRequest.FromJson(JsonMapper.ToObject(request));
                            if (item != null)
                            {
                                var itemModel = _unitModel.ItemModels.First(model => model.Name == item.ItemName);
                                var obtainText = UIManager.Instance.GetLocalizationText("UnitObtain");
                                text += $"{itemModel.Name} x {item.AcquireCount} {obtainText}\n";
                            }
                        }
                    }
                }
                _getItemDialog.SetText(text);
            }
            
            SetState(State.BuySucceed);
            _getItemDialog.OnOpenEvent();
        }
    }
}