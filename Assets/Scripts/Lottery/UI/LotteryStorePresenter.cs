using System.Collections.Generic;
using System.Linq;
using Gs2.Core.Exception;
using Gs2.Gs2Inventory.Request;
using Gs2.Gs2Lottery.Result;
using Gs2.Sample.Core;
using Gs2.Sample.JobQueue;
using Gs2.Sample.Money;
using Gs2.Sample.Unit;
using Gs2.Unity.Gs2Distributor.Result;
using Gs2.Unity.Gs2Showcase.Model;
using Gs2.Unity.Util;
using Gs2.Util.LitJson;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

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
        
        private StampSheetRunner _stampSheetRunner;
        
        [SerializeField]
        private JobQueueModel _jobQueueModel;
        
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
            Assert.IsNotNull(_jobQueueModel);
            
            _lotteryStoreView.OnCloseEvent();
            
            _lotterySetting.onIssueBuyStampSheet.AddListener(OnIssueStampSheet);
            _lotterySetting.onError.AddListener(OnError);
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
            
            // 抽選処理のスタンプシート
            // Stamp sheet for lottery processing
            _stampSheetRunner = new StampSheetRunner(
                GameManager.Instance.Cllient.Client
            );
            _stampSheetRunner.AddDoneStampTaskEventHandler(
                _moneyPresenter.GetTaskCompleteAction(),
                _unitPresenter.GetTaskCompleteAction(),
                _jobQueueModel.GetTaskCompleteAction(),
                GetTaskCompleteAction()
            );
            _stampSheetRunner.AddCompleteStampSheetEvent(
                _moneyPresenter.GetSheetCompleteAction(),
                _unitPresenter.GetSheetCompleteAction(),
                _jobQueueModel.GetSheetCompleteAction(),
                GetSheetCompleteAction()
            );
            
            // 商品受け取りのJobQueue
            // JobQueue for goods receipt
            _jobQueueModel.Initialize(
                _lotterySetting.jobQueueNamespaceName,
                _lotterySetting.onError
            );
            _jobQueueModel.onExecJob.AddListener(
                _unitPresenter.GetJobQueueAction()
            );
        }
        
        public void Finish()
        {
            _lotterySetting.onIssueBuyStampSheet.RemoveListener(OnIssueStampSheet);
            _lotterySetting.onError.RemoveListener(OnError);
        }

        public void OnError(Gs2Exception e)
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
            
            StartCoroutine(
                _lotteryStoreModel.GetShowcase(
                    r =>
                    {
                        if (r.Error == null)
                        {
                            OnGetSalesItems(_lotteryStoreModel.Showcase, _lotteryStoreModel.DisplayItems);
                            
                            SetState(State.OpenLotteryStore);
                        }
                        else
                        {
                            SetState(State.GetShowcaseFailed);
                        }
                    },
                    GameManager.Instance.Cllient.Client,
                    GameManager.Instance.Session.Session,
                    _lotterySetting.showcaseNamespaceName,
                    _lotterySetting.showcaseName,
                    _lotterySetting.onGetShowcase,
                    _lotterySetting.onError
                )
            );
        }
        
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
                item.Initialize(new SalesItem(displayItem.DisplayItemId, displayItem.SalesItem));
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
            
            StartCoroutine(
                _lotteryStoreModel.Buy(
                    GameManager.Instance.Cllient.Client,
                    GameManager.Instance.Session.Session,
                    _lotterySetting.showcaseNamespaceName,
                    _lotterySetting.showcaseName,
                    salesItem.DisplayItemId,
                    _lotterySetting.onIssueBuyStampSheet,
                    _lotterySetting.onError,
                    config.Select(item => new EzConfig
                    {
                        Key = item.Key,
                        Value = item.Value
                    }).ToList()
                )
            );
        }

        /// <summary>
        /// スタンプシートが発行された
        /// Stamp sheet issued
        /// </summary>
        /// <param name="stampSheet"></param>
        public void OnIssueStampSheet(
            string stampSheet
        )
        {
            UIManager.Instance.AddLog("LotteryStorePresenter::OnIssueStampSheet");

            // スタンプシートを実行
            // Execute stamp sheet
            StartCoroutine(
                _stampSheetRunner.Run(
                    stampSheet,
                    _lotterySetting.showcaseKeyId,
                    _lotterySetting.onError
                )
            );
        }

        public UnityAction<EzStampTask, EzRunStampTaskResult> GetTaskCompleteAction()
        {
            return (task, taskResult) =>
            {
                Debug.Log("LotteryStorePresenter::StateMachineOnDoneStampTask");
            };
        }

        public UnityAction<EzStampSheet, EzRunStampSheetResult> GetSheetCompleteAction()
        {
            return (sheet, sheetResult) =>
            {
                Debug.Log("LotteryStorePresenter::StateMachineOnCompleteStampSheet");

                // 抽選処理の結果を取得
                // Obtain the results of the lottery process
                if (sheet.Action == "Gs2Lottery:DrawByUserId")
                {
                    // 抽選によって取得したアイテムがインベントリに追加される
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
                    _lotterySetting.onAcquireInventoryItem.Invoke(
                        mergedAcquireRequests
                    );
                    // スタンプシートを実行
                    StartCoroutine(
                        _stampSheetRunner.Run(
                            result.StampSheet,
                            _lotterySetting.lotteryKeyId,
                            _lotterySetting.onError
                        )
                    );
                }
            };
        }
        
        /// <summary>
        /// ガチャで入手したアイテム ダイアログ
        /// </summary>
        /// <param name="requests"></param>
        public void OnAcquireInventoryItem(
            List<AcquireItemSetByUserIdRequest> requests
        )
        {
            string text = "";
            foreach (var request in requests)
            {
                var itemModel = _unitModel.ItemModels.First(model => model.Name == request.ItemName);
                var obtainText = UIManager.Instance.GetLocalizationText("UnitObtain");
                text += $"{itemModel.Name} x {request.AcquireCount} {obtainText}\n";
            }
            _getItemDialog.SetText(text);
            
            SetState(State.BuySucceed);
            _getItemDialog.OnOpenEvent();
        }
    }
}