using System.Collections.Generic;
using System.Linq;
using Gs2.Core.Exception;
using Gs2.Gs2Inventory.Request;
using Gs2.Gs2Lottery.Result;
using Gs2.Sample.Core;
using Gs2.Sample.Core.Runtime;
using Gs2.Sample.Money;
using Gs2.Sample.Quest;
using Gs2.Sample.Unit;
using Gs2.Unity.Gs2Distributor.Result;
using Gs2.Unity.Gs2Showcase.Model;
using Gs2.Unity.Util;
using Gs2.Util.LitJson;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Gs2.Sample.Gacha
{
    public class GachaStorePresenter : MonoBehaviour
    {
        [SerializeField]
        private GachaSetting _gachaSetting;
        
        [SerializeField]
        public GachaStoreView _gachaStoreView;
        [SerializeField]
        public GetItemDialog _getItemDialog;
        [SerializeField]
        public GachaStoreModel _gachaStoreModel;
        [SerializeField]
        public UnitModel _unitModel;
        
        [SerializeField] private MoneyPresenter _moneyPresenter;

        private StampSheetRunner _stampSheetRunner;
        
        public enum State
        {
            MainMenu,
            
            GetShowcaseProcessing,
            OpenGachaStore,
            GetShowcaseFailed,
            
            SelectGacha,
            
            BuyProcessing,
            BuySucceed,
            BuyFailed,
        }
        
        /// <summary>
        /// 現在のステータス
        /// </summary>
        private State _gachaStoreState = State.MainMenu;
        
        public void Start()
        {
            Assert.IsNotNull(_gachaSetting);

            _gachaStoreView.OnCloseEvent();
            
            _gachaSetting.onIssueBuyStampSheet.AddListener(OnIssueStampSheet);
            _gachaSetting.onError.AddListener(OnError);
        }

        private void SetState(State _state)
        {
            if (_gachaStoreState != _state)
            {
                switch (_state)
                {
                    default:
                        _gachaStoreView.OnCloseEvent();
                        break;
                    case State.GetShowcaseProcessing:
                        _gachaStoreView.OnCloseEvent();
                        UIManager.Instance.OpenProcessing();
                        break;
                    case State.OpenGachaStore:
                        UIManager.Instance.CloseProcessing();
                        _gachaStoreView.OnOpenEvent();
                        break;
                    
                    case State.SelectGacha:
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
            _gachaStoreState = _state;
        }
        
        public void OnDestroy()
        {
            _gachaSetting.onIssueBuyStampSheet.RemoveListener(OnIssueStampSheet);
            _gachaSetting.onError.RemoveListener(OnError);
        }

        public void Initialize(
            StampSheetRunner stampSheetRunner
        )
        {
            Debug.Log("GachaStorePresenter::Initialize");

　           _stampSheetRunner = stampSheetRunner;
        }

        public void OnError(Gs2Exception e)
        {
            SetState(State.BuyFailed);
        }
        
        /// <summary>
        /// ガチャ商品ストアを開く
        /// </summary>
        public void ClickToOpenGachaStore()
        {
            SetState(State.GetShowcaseProcessing);
            
            StartCoroutine(
                _gachaStoreModel.GetShowcase(
                    r =>
                    {
                        if (r.Error == null)
                        {
                            OnGetSalesItems(_gachaStoreModel.Showcase, _gachaStoreModel.DisplayItems);
                            
                            SetState(State.OpenGachaStore);
                        }
                        else
                        {
                            SetState(State.GetShowcaseFailed);
                        }
                    },
                    GameManager.Instance.Cllient.Client,
                    GameManager.Instance.Session.Session,
                    _gachaSetting.showcaseNamespaceName,
                    _gachaSetting.showcaseName,
                    _gachaSetting.onGetShowcase,
                    _gachaSetting.onError
                )
            );
        }
        
        /// <summary>
        /// 商品リストの初期化
        /// </summary>
        /// <param name="showcase"></param>
        /// <param name="displayItems"></param>
        private void OnGetSalesItems(EzShowcase showcase, List<EzDisplayItem> displayItems)
        {
            for (var i = 0; i < _gachaStoreView.contentTransform.childCount; i++)
            {
                var gameObject = _gachaStoreView.contentTransform.GetChild(i).gameObject;
                if (gameObject == _gachaStoreView.productPrefab.gameObject)
                    continue;
                
                Destroy(gameObject);
            }

            foreach (var displayItem in displayItems)
            {
                var item = Instantiate(_gachaStoreView.productPrefab, _gachaStoreView.contentTransform);
                item.Initialize(new SalesItem(displayItem.DisplayItemId, displayItem.SalesItem));
                item.onBuy.AddListener(OnBuyGacha);
                item.gameObject.SetActive(true);
            }
        }


        /// <summary>
        /// </summary>
        /// <param name="salesItem"></param>
        public void OnBuyGacha(
            SalesItem salesItem
        )
        {
            UIManager.Instance.AddLog("GachaStorePresenter::OnBuyGacha");

            _gachaStoreModel.selectedItem = salesItem;
            SetState(State.BuyProcessing);
            
            var config = new Dictionary<string, string>
            {
                ["slot"] = MoneyModel.Slot.ToString()
            };
            
            StartCoroutine(
                _gachaStoreModel.Buy(
                    GameManager.Instance.Cllient.Client,
                    GameManager.Instance.Session.Session,
                    _gachaSetting.showcaseNamespaceName,
                    _gachaSetting.showcaseName,
                    salesItem.DisplayItemId,
                    _gachaSetting.onIssueBuyStampSheet,
                    _gachaSetting.onError,
                    config.Select(item => new EzConfig
                    {
                        Key = item.Key,
                        Value = item.Value
                    }).ToList()
                )
            );
        }

        public void OnIssueStampSheet(
            string stampSheet
        )
        {
            Debug.Log("GachaStorePresenter::OnIssueStampSheet");

            StartCoroutine(
                _stampSheetRunner.Run(
                    stampSheet,
                    _gachaSetting.showcaseKeyId,
                    _gachaSetting.onError
                )
            );
        }

        public UnityAction<EzStampTask, EzRunStampTaskResult> GetTaskCompleteAction()
        {
            return (task, taskResult) =>
            {
                Debug.Log("GachaStorePresenter::StateMachineOnDoneStampTask");
            };
        }

        public UnityAction<EzStampSheet, EzRunStampSheetResult> GetSheetCompleteAction()
        {
            return (sheet, sheetResult) =>
            {
                Debug.Log("GachaStorePresenter::StateMachineOnCompleteStampSheet");

                if (sheet.Action == "Gs2Lottery:DrawByUserId")
                {
                    var json = JsonMapper.ToObject(sheetResult.Result);
                    var result = DrawByUserIdResult.FromJson(json);
                    var mergedAcquireRequests = new List<AcquireItemSetByUserIdRequest>();
                    foreach (var acquireRequests in result.Items.Select(item => (
                        from acquireAction in item.AcquireActions 
                        where acquireAction.Action == "Gs2Inventory:AcquireItemSetByUserId" 
                        select JsonMapper.ToObject(acquireAction.Request) into acquireJson 
                        select AcquireItemSetByUserIdRequest.FromJson(acquireJson)
                    ).ToList()))
                    {
                        mergedAcquireRequests.AddRange(acquireRequests);
                    }
                    _gachaSetting.onAcquireInventoryItem.Invoke(
                        mergedAcquireRequests
                    );
                    StartCoroutine(
                        _stampSheetRunner.Run(
                            result.StampSheet,
                            _gachaSetting.lotteryKeyId,
                            _gachaSetting.onError
                        )
                    );
                }
            };
        }
        
        public void OnAcquireInventoryItem(
            List<AcquireItemSetByUserIdRequest> requests
        )
        {
            string text = "";
            foreach (var request in requests)
            {
                var itemModel = _unitModel.ItemModels.First(model => model.Name == request.ItemName);
                text += $"{itemModel.Name} x {request.AcquireCount} を入手しました。\n";
            }
            _getItemDialog.SetText(text);
            
            SetState(State.BuySucceed);
            _getItemDialog.OnOpenEvent();
        }
    }
}