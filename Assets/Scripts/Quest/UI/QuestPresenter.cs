using System.Collections;
using System.Collections.Generic;
using Gs2.Core.Exception;
using Gs2.Sample.Money;
using Gs2.Sample.Stamina;
using Gs2.Unity.Gs2Quest.Model;
using UnityEngine;
using UnityEngine.Assertions;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
#endif

namespace Gs2.Sample.Quest
{
    public class QuestPresenter : MonoBehaviour
    {
        [SerializeField]
        private QuestSetting _questSetting;
        
        [SerializeField]
        private QuestModel _questModel;
        
        [SerializeField]
        private SelectQuestView _selectQuestView;
        [SerializeField]
        private QuestClearView _questClearView;

        [SerializeField]
        private StaminaPresenter _staminaPresenter;
        [SerializeField]
        private MoneyPresenter _moneyPresenter;
        
        public enum State
        {
            MainMenu,
            
            /// <summary>
            /// クエストグループ情報を取得中
            /// </summary>
            GetQuestGroupProcessing,
            /// <summary>
            /// クエストグループ情報の取得に失敗
            /// </summary>
            GetQuestGroupFailed,
            
            /// <summary>
            /// クエストグループメニューを開く
            /// </summary>
            OpenQuestGroupMenu,

            /// <summary>
            /// クエストグループ選択
            /// </summary>
            SelectQuestGroup,

            /// <summary>
            /// クエスト情報を取得中
            /// </summary>
            GetQuestProcessing,
            /// <summary>
            /// クエスト情報の取得に失敗
            /// </summary>
            GetQuestFailed,
            
            /// <summary>
            /// クエスト選択メニューを開く
            /// </summary>
            OpenQuestMenu,
            /// <summary>
            /// 開始するクエストを選択
            /// </summary>
            SelectQuest,
            /// <summary>
            /// クエストの開始に成功
            /// </summary>
            StartQuestSuccess,
            /// <summary>
            /// クエストの開始に失敗
            /// </summary>
            StartQuestFailed,
            
            /// <summary>
            /// クエスト完了情報を取得中
            /// </summary>
            GetQuestClearProcessing,
            /// <summary>
            /// クエスト完了ダイアログを開く
            /// </summary>
            OpenQuestClear,

            /// <summary>
            /// 進行中のクエストが見つからない
            /// </summary>
            QuestProgressError,
            
            /// <summary>
            /// クエストを完了をリクエスト
            /// </summary>
            SendCompleteResult,
            /// <summary>
            /// クエストを失敗をリクエスト
            /// </summary>
            SendFailedResult,
            
            Error,
        }
        
        /// <summary>
        /// 現在のUIのステート
        /// </summary>
        private State _questSelectState = State.MainMenu;

        public enum QuestState
        {
            /// <summary>
            /// 開始しているクエストなし
            /// </summary>
            None,

            /// <summary>
            /// クエストを進行中
            /// </summary>
            QuestStarted,
        }
        /// <summary>
        /// 現在のクエストのステート
        /// </summary>
        private QuestState _questState = QuestState.None;
        
        // Start is called before the first frame update
        void Start()
        {
            Assert.IsNotNull(_questSetting);
            Assert.IsNotNull(_questModel);
            Assert.IsNotNull(_selectQuestView);
            Assert.IsNotNull(_questClearView);
            Assert.IsNotNull(_staminaPresenter);
            Assert.IsNotNull(_moneyPresenter);
            
            _selectQuestView.OnCloseEvent();
        }
        
        private void SetState(State _state)
        {
            if (_questSelectState != _state)
            {
                switch (_state)
                {
                    default:
                        UIManager.Instance.CloseProcessing();
                        _selectQuestView.OnCloseEvent();
                        _selectQuestView.OnCloseQuestGroupMenuEvent();
                        _selectQuestView.OnCloseQuestMenuEvent();
                        _selectQuestView.OnCloseQuestClearEvent();
                        break;
                    
                    // クエストグループ情報を取得中
                    case State.GetQuestGroupProcessing:
                        UIManager.Instance.OpenProcessing();
                        _selectQuestView.OnCloseQuestGroupMenuEvent();
                        _selectQuestView.OnCloseQuestMenuEvent();
                        break;
                        
                    // クエストグループ メニューを開く
                    case State.OpenQuestGroupMenu:
                        UIManager.Instance.CloseProcessing();
                        _selectQuestView.OnOpenQuestGroupMenuEvent();
                        break;
                    
                    // クエストグループを選択
                    case State.SelectQuestGroup:
                        _selectQuestView.OnCloseQuestGroupMenuEvent();
                        UIManager.Instance.OpenProcessing();
                        break;
                    
                    // クエスト メニューを開く
                    case State.OpenQuestMenu:
                        UIManager.Instance.CloseProcessing();
                        _selectQuestView.OnOpenQuestMenuEvent();
                        break;
                    
                    // クエストを選択
                    case State.SelectQuest:
                        _selectQuestView.OnCloseQuestMenuEvent();
                        break;
                    
                    // クエスト完了 情報を取得中
                    case State.GetQuestClearProcessing:
                        UIManager.Instance.OpenProcessing();
                        break;
                    
                    // クエスト 完了を開く
                    case State.OpenQuestClear:
                        UIManager.Instance.CloseProcessing();
                        _selectQuestView.OnOpenQuestClearEvent();
                        break;
                    case State.QuestProgressError:
                        break;
                    
                    // クエストを完了をリクエスト
                    case State.SendCompleteResult:
                        _selectQuestView.OnCloseQuestClearEvent();
                        UIManager.Instance.OpenProcessing();
                        break;
                    // クエストを失敗をリクエスト
                    case State.SendFailedResult:
                        _selectQuestView.OnCloseQuestClearEvent();
                        UIManager.Instance.OpenProcessing();
                        break;
                }
            }
            
            _questSelectState = _state;
        }

        private void SetQuestState(QuestState _state)
        {
            switch (_state)
            {
                case QuestState.None:
                    _selectQuestView.questStart.interactable = true;
                    _selectQuestView.questEnd.interactable = false;
                    UIManager.Instance.SetQuestStateText("Quest not received");
                    break;
                case QuestState.QuestStarted:
                    _selectQuestView.questStart.interactable = false;
                    _selectQuestView.questEnd.interactable = true;
                    UIManager.Instance.SetQuestStateText("Quest in progress");
                    break;
            }

            _questState = _state;
        }

        /// <summary>
        /// クエストの初期化　現在進行しているクエストのチェック
        /// </summary>
        public IEnumerator Initialize()
        {
            UIManager.Instance.AddLog("QuestPresenter::Initialize");
            
            // クエストグループリストを取得
            {
                List<EzQuestGroupModel> result = null;
                yield return _questModel.GetQuestGroups(
                    r => result = r,
                    GameManager.Instance.Domain,
                    _questSetting.questNamespaceName,
                    _questSetting.onListGroupQuestModel,
                    _questSetting.onError
                );
            }
            // クエストの進行情報を取得
            {
                EzProgress result = null;
                yield return _questModel.GetProgress(
                    r => result = r,
                    GameManager.Instance.Domain,
                    GameManager.Instance.Session,
                    _questSetting.questNamespaceName,
                    _questSetting.onGetProgress,
                    _questSetting.onError
                );

                if (result == null)
                {
                    SetQuestState(QuestState.None);
                    yield break;
                }
            }
            SetQuestState(QuestState.QuestStarted);
        }
#if GS2_ENABLE_UNITASK
        /// <summary>
        /// クエストの初期化　現在進行しているクエストのチェック
        /// </summary>
        public async UniTask InitializeAsync()
        {
            UIManager.Instance.AddLog("QuestPresenter::InitializeAsync");
            
            // クエストグループリストを取得
            {
                List<EzQuestGroupModel> result = await _questModel.GetQuestGroupsAsync(
                    GameManager.Instance.Domain,
                    _questSetting.questNamespaceName,
                    _questSetting.onListGroupQuestModel,
                    _questSetting.onError
                );
            }
            // クエストの進行情報を取得
            {
                EzProgress result = await _questModel.GetProgressAsync(
                    GameManager.Instance.Domain,
                    GameManager.Instance.Session,
                    _questSetting.questNamespaceName,
                    _questSetting.onGetProgress,
                    _questSetting.onError
                );

                if (result == null)
                {
                    SetQuestState(QuestState.None);
                    return;
                }
            }
            SetQuestState(QuestState.QuestStarted);
        }
#endif
        
        public void Finish()
        {
        }
        
        /// <summary>
        /// 現在進行しているクエストのチェック
        /// </summary>
        public IEnumerator CheckCurrentProgressTask()
        {
            EzProgress result = null;
            yield return _questModel.GetProgress(
                r => result = r,
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _questSetting.questNamespaceName,
                _questSetting.onGetProgress,
                _questSetting.onError
            );
            
            if (result == null)
            {
                _questClearView.SetProgress(UIManager.Instance.GetLocalizationText("QuestNotFound"));
                _questClearView.SetInteractable(false);

                SetState(State.QuestProgressError);
                yield break;
            }

            UIManager.Instance.AddLog("QuestModel::Progress::RandomSeed:" + _questModel.progress.RandomSeed);
            _questClearView.SetProgress(_questModel.selectedQuest.Metadata);

            _questClearView.SetInteractable(true);
            
            SetState(State.OpenQuestClear);
        }
        
#if GS2_ENABLE_UNITASK
        /// <summary>
        /// 現在進行しているクエストのチェック
        /// </summary>
        public async UniTask CheckCurrentProgressTaskAsync()
        {
            EzProgress result = await _questModel.GetProgressAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _questSetting.questNamespaceName,
                _questSetting.onGetProgress,
                _questSetting.onError
            );
            
            if (result == null)
            {
                _questClearView.SetProgress(UIManager.Instance.GetLocalizationText("QuestNotFound"));
                _questClearView.SetInteractable(false);

                SetState(State.QuestProgressError);
                return;
            }

            UIManager.Instance.AddLog("QuestModel::Progress::RandomSeed:" + _questModel.progress.RandomSeed);
            _questClearView.SetProgress(_questModel.selectedQuest.Metadata);

            _questClearView.SetInteractable(true);
            
            SetState(State.OpenQuestClear);
        }
#endif
        
        /// <summary>
        /// クエストグループ選択メニューを開く
        /// </summary>
        public void OnOpenQuestGroupMenu()
        {
            gameObject.SetActive(true);
            SetState(State.GetQuestGroupProcessing);
            
#if GS2_ENABLE_UNITASK
            GetQuestGroupsTaskAsync().Forget();
#else
            StartCoroutine(
                GetQuestGroupsTask()
            );
#endif
        }
        
        /// <summary>
        /// クエストグループ選択メニューを閉じる
        /// </summary>
        public void OnCloseQuestGeoupMenu()
        {
            SetState(State.MainMenu);
        }
        
        /// <summary>
        /// クエストグループとクリア状況を取得
        /// </summary>
        public IEnumerator GetQuestGroupsTask()
        {
            {
                List<EzQuestGroupModel> result = null;
                yield return _questModel.GetQuestGroups(
                    r => result = r,
                    GameManager.Instance.Domain,
                    _questSetting.questNamespaceName,
                    _questSetting.onListGroupQuestModel,
                    _questSetting.onError
                );

                if (result == null)
                {
                    SetState(State.GetQuestGroupFailed);
                    yield break;
                }

                OnGetListGroupQuestModelFunc(_questModel.questGroups);
                SetState(State.OpenQuestGroupMenu);
            }

            {
                List<EzCompletedQuestList> result = null;
                yield return _questModel.GetCompleteQuests(
                    r => result = r,
                    GameManager.Instance.Domain,
                    GameManager.Instance.Session,
                    _questSetting.questNamespaceName,
                    _questSetting.onListCompletedQuestsModel,
                    _questSetting.onError
                );

                if (result == null)
                {
                    SetState(State.GetQuestGroupFailed);
                    yield break;
                }
            }

            SetState(State.OpenQuestGroupMenu);
        }
        
#if GS2_ENABLE_UNITASK
        /// <summary>
        /// クエストグループとクリア状況を取得
        /// </summary>
        public async UniTask GetQuestGroupsTaskAsync()
        {
            {
                List<EzQuestGroupModel> result = await _questModel.GetQuestGroupsAsync(
                    GameManager.Instance.Domain,
                    _questSetting.questNamespaceName,
                    _questSetting.onListGroupQuestModel,
                    _questSetting.onError
                );

                if (result == null)
                {
                    SetState(State.GetQuestGroupFailed);
                    return;
                }

                OnGetListGroupQuestModelFunc(_questModel.questGroups);
                SetState(State.OpenQuestGroupMenu);
            }

            {
                List<EzCompletedQuestList> result = await _questModel.GetCompleteQuestsAsync(
                    GameManager.Instance.Domain,
                    GameManager.Instance.Session,
                    _questSetting.questNamespaceName,
                    _questSetting.onListCompletedQuestsModel,
                    _questSetting.onError
                );

                if (result == null)
                {
                    SetState(State.GetQuestGroupFailed);
                    return;
                }
            }

            SetState(State.OpenQuestGroupMenu);
        }
#endif
        
        /// <summary>
        /// クエストグループの項目UI操作
        /// </summary>
        private void OnGetListGroupQuestModelFunc(List<EzQuestGroupModel> questGroups)
        {
            _selectQuestView.questGroupInfo.SetActive(false);

            if (_selectQuestView.questGroupsContent != null)
            {
                foreach (Transform child in _selectQuestView.questGroupsContent.transform)
                {
                    if (child != null && child.gameObject != _selectQuestView.questGroupInfo)
                    {
                        Destroy(child.gameObject);
                    }
                }

                foreach (var questGroup in questGroups)
                {
                    var questGroupInfo = Instantiate<GameObject>(_selectQuestView.questGroupInfo,
                        _selectQuestView.questGroupsContent.transform);
                    var questGroupView = questGroupInfo.GetComponent<QuestGroupView>();
                    questGroupView.Initialize(new QuestGroupInformation(questGroup),
                        () =>
                        {
                            _questModel.currentCompletedQuestList = _questModel.completedQuests.Find(completedQuestList =>
                                completedQuestList.QuestGroupName == questGroup.Name);

                            ClickToSelect(questGroup);
                        }
                    );
                    questGroupInfo.gameObject.SetActive(true);
                }
            }
        }
        
        /// <summary>
        /// クエストグループを選択する
        /// </summary>
        public void ClickToSelect(EzQuestGroupModel questGroup)
        {
            if (_questSelectState == State.OpenQuestGroupMenu)
            {
                _questModel.selectedQuestGroup = questGroup;
                SetState(State.SelectQuestGroup);
                
#if GS2_ENABLE_UNITASK
                GetQuestsTaskAsync().Forget();
#else
                StartCoroutine(
                    GetQuestsTask()
                );
#endif
            }
        }

        /// <summary>
        /// クエストの一覧を取得
        /// </summary>
        public IEnumerator GetQuestsTask()
        {
            List<EzQuestModel> result = null;
            yield return _questModel.GetQuests(
                r => result = r,
                GameManager.Instance.Domain,
                _questSetting.questNamespaceName,
                _questSetting.onListQuestModel,
                _questSetting.onError
            );

            if (result == null)
            {
                SetState(State.GetQuestFailed);
                yield break;
            }

            OnGetListQuestModel(_questModel.quests);

            // クエストメニューを開く
            SetState(State.OpenQuestMenu);
        }
        
#if GS2_ENABLE_UNITASK
        /// <summary>
        /// クエストの一覧を取得
        /// </summary>
        public async UniTask GetQuestsTaskAsync()
        {
            List<EzQuestModel> result = await _questModel.GetQuestsAsync(
                GameManager.Instance.Domain,
                _questSetting.questNamespaceName,
                _questSetting.onListQuestModel,
                _questSetting.onError
            );

            if (result == null)
            {
                SetState(State.GetQuestFailed);
                return;
            }

            OnGetListQuestModel(_questModel.quests);

            // クエストメニューを開く
            SetState(State.OpenQuestMenu);
        }
#endif
        
        /// <summary>
        /// クエストを選択する
        /// </summary>
        public void ClickToSelect(EzQuestModel quest)
        {
            if (_questSelectState == State.OpenQuestMenu)
            {
                _questModel.selectedQuest = quest;
                SetState(State.SelectQuest);
                
#if GS2_ENABLE_UNITASK
                StartTaskAsync().Forget();
#else
                StartCoroutine(
                    StartTask()
                );
#endif
            }
        }

        /// <summary>
        /// クエストを開始
        /// </summary>
        public IEnumerator StartTask()
        {
            EzProgress result = null;
            yield return _questModel.QuestStart(
                r => result = r,
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _questSetting.questNamespaceName,
                MoneyModel.Slot,
                _questSetting.onStart,
                _questSetting.onError
            );

            if (result == null)
            {
                SetState(State.StartQuestFailed);
                yield break;
            }

            _staminaPresenter.OnUpdateStamina();
            
            SetState(State.StartQuestSuccess);
            
            SetQuestState(QuestState.QuestStarted);
            
            UIManager.Instance.OpenDialog1("Notice", "QuestStart");
        }
        
#if GS2_ENABLE_UNITASK
        /// <summary>
        /// クエストを開始
        /// </summary>
        public async UniTask StartTaskAsync()
        {
            EzProgress result = await _questModel.QuestStartAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _questSetting.questNamespaceName,
                MoneyModel.Slot,
                _questSetting.onStart,
                _questSetting.onError
            );

            if (result == null)
            {
                SetState(State.StartQuestFailed);
                return;
            }

            _staminaPresenter.OnUpdateStamina();
            
            SetState(State.StartQuestSuccess);
            
            SetQuestState(QuestState.QuestStarted);
            
            UIManager.Instance.OpenDialog1("Notice", "QuestStart");
        }
#endif
        
        /// <summary>
        /// クエスト完了ダイアログを開く
        /// </summary>
        public void OnOpenQuestClear()
        {
            gameObject.SetActive(true);
            SetState(State.GetQuestClearProcessing);
#if GS2_ENABLE_UNITASK
            CheckCurrentProgressTaskAsync().Forget();
#else
            StartCoroutine(
                CheckCurrentProgressTask()
            );
#endif
        }
        
        /// <summary>
        /// クエスト完了ダイアログを閉じる
        /// </summary>
        public void ClickToCloseQuestClear()
        {
            SetState(State.MainMenu);
        }

        /// <summary>
        /// クエストリストの表示
        /// </summary>
        private void OnGetListQuestModel(List<EzQuestModel> quests)
        {
            _selectQuestView.questInfo.SetActive(false);
            
            if (_selectQuestView.questsContent != null)
            {
                foreach (Transform child in _selectQuestView.questsContent.transform)
                {
                    if (child != null && child.gameObject != _selectQuestView.questInfo)
                    {
                        Destroy(child.gameObject);
                    }
                }

                foreach (var quest in quests)
                {
                    var questInfo = Instantiate<GameObject>(_selectQuestView.questInfo,
                        _selectQuestView.questsContent.transform);
                    var questView = questInfo.GetComponent<QuestView>();
                    questView.Initialize(new QuestInformation(quest, _questModel.currentCompletedQuestList),
                        () =>
                        {
                            ClickToSelect(quest);
                        }
                    );
                    questInfo.gameObject.SetActive(true);
                }
            }
        }
        
        /// <summary>
        /// クエスト完了をリクエスト
        /// </summary>
        public void OnSendCompleteResult()
        {
            SetState(State.SendCompleteResult);

#if GS2_ENABLE_UNITASK
            EndTaskAsync(
                _questModel.progress.Rewards,
                true
            ).Forget();
#else
            StartCoroutine(
                EndTask(
                    _questModel.progress.Rewards,
                    true
                )
            );
#endif
        }
        
        /// <summary>
        /// クエスト失敗（破棄）をリクエスト
        /// </summary>
        public void OnSendFailedResult()
        {
            SetState(State.SendFailedResult);
            
#if GS2_ENABLE_UNITASK
            EndTaskAsync(
                _questModel.progress.FailedRewards,
                false
            ).Forget();
#else
            StartCoroutine(
                EndTask(
                    _questModel.progress.FailedRewards,
                    false
                )
            );
#endif
        }

        /// <summary>
        /// クエスト完了リクエスト
        /// </summary>
        /// <returns></returns>
        public IEnumerator EndTask(
            List<EzReward> rewards,
            bool isComplete
        )
        {
            EzProgress result = null;
            yield return _questModel.QuestEnd(
                r => result = r,
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _questSetting.questNamespaceName,
                isComplete,
                rewards,
                MoneyModel.Slot,
                _questSetting.onEnd,
                _questSetting.onError
            );

            if (result == null)
            {
                SetState(State.Error);

                yield break;
            }

            SetState(State.MainMenu);
            
            SetQuestState(QuestState.None);

            _staminaPresenter.OnUpdateStamina();
            _moneyPresenter.OnUpdateWallet();
            
            if (isComplete)
            {
                UIManager.Instance.OpenDialog1("Notice", "QuestComp");
            }
            else
            {
                UIManager.Instance.OpenDialog1("Notice", "QuestFailed");
            }
        }
#if GS2_ENABLE_UNITASK
        /// <summary>
        /// クエスト完了リクエスト
        /// </summary>
        /// <returns></returns>
        public async UniTask EndTaskAsync(
            List<EzReward> rewards,
            bool isComplete
        )
        {
            Gs2Exception error = await _questModel.QuestEndAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _questSetting.questNamespaceName,
                isComplete,
                rewards,
                MoneyModel.Slot,
                _questSetting.onEnd,
                _questSetting.onError
            );

            if (error != null)
            {
                SetState(State.Error);
                return;
            }

            SetState(State.MainMenu);
            
            SetQuestState(QuestState.None);

            _staminaPresenter.OnUpdateStamina();
            _moneyPresenter.OnUpdateWallet();
            
            if (isComplete)
            {
                UIManager.Instance.OpenDialog1("Notice", "QuestComp");
            }
            else
            {
                UIManager.Instance.OpenDialog1("Notice", "QuestFailed");
            }
        }
#endif
    }
}