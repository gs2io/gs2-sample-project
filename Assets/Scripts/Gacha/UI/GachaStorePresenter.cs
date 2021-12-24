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

namespace Gs2.Sample.Gacha
{
    public class GachaStorePresenter : MonoBehaviour
    {
        [SerializeField]
        private GachaSetting _gachaSetting;
        
        [SerializeField]
        public GachaStoreModel _gachaStoreModel;
        
        [SerializeField]
        public GachaStoreView _gachaStoreView;
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
            /// </summary>
            GetShowcaseProcessing,
            /// <summary>
            /// 商品棚情報の取得に失敗
            /// </summary>
            GetShowcaseFailed,
            
            /// <summary>
            /// ガチャストアを開く
            /// </summary>
            OpenGachaStore,
            
            /// <summary>
            /// ガチャ商品を購入
            /// </summary>
            BuyProcessing,
            /// <summary>
            /// ガチャ商品の購入に成功
            /// </summary>
            BuySucceed,
            /// <summary>
            /// ガチャ商品の購入に失敗
            /// </summary>
            BuyFailed,
        }
        
        /// <summary>
        /// 現在のステート
        /// </summary>
        private State _gachaStoreState = State.MainMenu;
        
        public void Start()
        {
            Assert.IsNotNull(_gachaSetting);
            Assert.IsNotNull(_gachaStoreModel);
            Assert.IsNotNull(_gachaStoreView);
            Assert.IsNotNull(_getItemDialog);
            Assert.IsNotNull(_unitModel);
            Assert.IsNotNull(_moneyPresenter);
            Assert.IsNotNull(_unitPresenter);
            Assert.IsNotNull(_jobQueueModel);
            
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
        
        public void Initialize()
        {
            UIManager.Instance.AddLog("GachaStorePresenter::Initialize");
            
            // ガチャ抽選処理のスタンプシート
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
            _jobQueueModel.Initialize(
                _gachaSetting.jobQueueNamespaceName,
                _gachaSetting.onError
            );
            _jobQueueModel.onExecJob.AddListener(
                _unitPresenter.GetJobQueueAction()
            );
        }
        
        public void Finish()
        {
            _gachaSetting.onIssueBuyStampSheet.RemoveListener(OnIssueStampSheet);
            _gachaSetting.onError.RemoveListener(OnError);
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
        /// ガチャ商品を購入
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

        /// <summary>
        /// スタンプシートが発行された
        /// </summary>
        /// <param name="stampSheet"></param>
        public void OnIssueStampSheet(
            string stampSheet
        )
        {
            UIManager.Instance.AddLog("GachaStorePresenter::OnIssueStampSheet");

            // スタンプシートを実行
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

                // Lottery 抽選処理の結果を取得
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
                    _gachaSetting.onAcquireInventoryItem.Invoke(
                        mergedAcquireRequests
                    );
                    // スタンプシートを実行
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
                text += $"{itemModel.Name} x {request.AcquireCount} を入手しました。\n";
            }
            _getItemDialog.SetText(text);
            
            SetState(State.BuySucceed);
            _getItemDialog.OnOpenEvent();
        }
    }
}