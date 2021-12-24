using System.Collections;
using System.Collections.Generic;
using Gs2.Core;
using Gs2.Core.Exception;
using Gs2.Sample.Core;
using Gs2.Sample.JobQueue;
using Gs2.Sample.Money;
using Gs2.Sample.Stamina;
using Gs2.Unity.Gs2Quest.Model;
using Gs2.Unity.Gs2Quest.Result;
using UnityEngine;
using UnityEngine.Assertions;

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
        
        [SerializeField]
        private JobQueueModel _jobQueueModel;
        
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
                    UIManager.Instance.SetQuestStateText("未受注");
                    break;
                case QuestState.QuestStarted:
                    _selectQuestView.questStart.interactable = false;
                    _selectQuestView.questEnd.interactable = true;
                    UIManager.Instance.SetQuestStateText("クエスト進行中");
                    break;
            }

            _questState = _state;
        }

        /// <summary>
        /// クエストの初期化　現在進行しているクエストのチェック
        /// </summary>
        /// <returns></returns>
        public IEnumerator Initialize()
        {
            UIManager.Instance.AddLog("QuestPresenter::Initialize");
            
            // 報酬受け取りのJobQueue
            _jobQueueModel.Initialize(
                _questSetting.jobQueueNamespaceName,
                _questSetting.onError
            );
            _jobQueueModel.onExecJob.AddListener(
                (job, body) =>
                {
                    _staminaPresenter.OnUpdateStamina();
                    _moneyPresenter.OnUpdateWallet();
                }
            );
            
            AsyncResult<EzGetProgressResult> result = null;
            yield return _questModel.GetProgress(
                r => result = r,
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                _questSetting.questNamespaceName,
                _questSetting.onGetProgress,
                _questSetting.onError
            );

            if (result.Error != null)
	        {
                if (result.Error is NotFoundException)
                {
                    SetQuestState(QuestState.None);

                    yield break;
                }
                else
                {
                    SetState(State.Error);
                    
                    _questSetting.onError.Invoke(
                        result.Error
                    );

                    yield break;
                }
            }

            SetQuestState(QuestState.QuestStarted);
        }

        public void Finish()
        {
        }
        
        /// <summary>
        /// 現在進行しているクエストのチェック
        /// </summary>
        /// <returns></returns>
        public IEnumerator CheckCurrentProgressTask()
        {
            AsyncResult<EzGetProgressResult> result = null;
            yield return _questModel.GetProgress(
                r => result = r,
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                _questSetting.questNamespaceName,
                _questSetting.onGetProgress,
                _questSetting.onError
            );

            if (result.Error != null)
            {
                if (result.Error is NotFoundException)
                {
                    _questClearView.SetProgress("進行しているクエストはありません。");
                    _questClearView.SetInteractable(false);
                    
                    SetState(State.QuestProgressError);
                    
                    yield break;
                }
                else
                {
                    SetState(State.Error);
                    
                    _questSetting.onError.Invoke(
                        result.Error
                    );

                    yield break;
                }
            }

            UIManager.Instance.AddLog("QuestModel::Progress::RandomSeed:" + _questModel.Progress.RandomSeed);
            _questClearView.SetProgress(_questModel.SelectedQuest.Metadata);

            _questClearView.SetInteractable(true);
            
            SetState(State.OpenQuestClear);
        }
        
        /// <summary>
        /// クエストグループ選択メニューを開く
        /// </summary>
        public void OnOpenQuestGroupMenu()
        {
            gameObject.SetActive(true);
            SetState(State.GetQuestGroupProcessing);

            StartCoroutine(
                GetQuestGroupsTask()
            );
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
        /// <returns></returns>
        public IEnumerator GetQuestGroupsTask()
        {
            {
                AsyncResult<EzListQuestGroupsResult> result = null;
                yield return _questModel.GetQuestGroups(
                    r => result = r,
                    GameManager.Instance.Cllient.Client,
                    _questSetting.questNamespaceName,
                    _questSetting.onListGroupQuestModel,
                    _questSetting.onError
                );

                if (result.Error != null)
                {
                    SetState(State.GetQuestGroupFailed);
                    yield break;
                }

                OnGetListGroupQuestModelFunc(_questModel.QuestGroups);
                SetState(State.OpenQuestGroupMenu);
            }

            {
                AsyncResult<EzDescribeCompletedQuestListsResult> result = null;
                yield return _questModel.GetCompleteQuests(
                    r => result = r,
                    GameManager.Instance.Cllient.Client,
                    GameManager.Instance.Session.Session,
                    _questSetting.questNamespaceName,
                    _questSetting.onListCompletedQuestsModel,
                    _questSetting.onError
                );

                if (result.Error != null)
                {
                    SetState(State.GetQuestGroupFailed);
                    
                    _questSetting.onError.Invoke(
                        result.Error
                    );
                    
                    yield break;
                }
            }

            SetState(State.OpenQuestGroupMenu);

        }
        
        /// <summary>
        /// クエストグループの項目UI操作
        /// </summary>
        /// <param name="questGroups"></param>
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
                            _questModel.CurrentCompletedQuestList = _questModel.CompletedQuests.Find(completedQuestList =>
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
                _questModel.SelectedQuestGroup = questGroup;
                SetState(State.SelectQuestGroup);
                
                // クエストリストを取得
                StartCoroutine(
                    GetQuestsTask()
                );
            }
        }

        /// <summary>
        /// クエストの一覧を取得
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetQuestsTask()
        {
            AsyncResult<EzListQuestsResult> result = null;
            yield return _questModel.GetQuests(
                r => result = r,
                GameManager.Instance.Cllient.Client,
                _questSetting.questNamespaceName,
                _questSetting.onListQuestModel,
                _questSetting.onError
            );

            if (result.Error != null)
            {
                SetState(State.GetQuestFailed);
                yield break;
            }

            OnGetListQuestModel(_questModel.Quests);

            // クエストメニューを開く
            SetState(State.OpenQuestMenu);
        }

        /// <summary>
        /// クエストを選択する
        /// </summary>
        public void ClickToSelect(EzQuestModel quest)
        {
            if (_questSelectState == State.OpenQuestMenu)
            {
                _questModel.SelectedQuest = quest;
                SetState(State.SelectQuest);
                
                // クエストを開始
                StartCoroutine(
                    StartTask()
                );
            }
        }

        /// <summary>
        /// クエストを開始
        /// </summary>
        /// <returns></returns>
        public IEnumerator StartTask()
        {
            AsyncResult<EzProgress> result = null;
            yield return _questModel.QuestStart(
                r => result = r,
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                _questSetting.questNamespaceName,
                _questSetting.distributorNamespaceName,
                _questSetting.questKeyId,
                _questSetting.onStart,
                _questSetting.onError
            );

            if (result.Error != null)
            {
                SetState(State.StartQuestFailed);
                yield break;
            }

            _staminaPresenter.OnUpdateStamina();
            
            SetState(State.StartQuestSuccess);
            
            SetQuestState(QuestState.QuestStarted);
            
            UIManager.Instance.OpenDialog1("Notice", "クエストを開始");
        }
        
        /// <summary>
        /// クエスト完了ダイアログを開く
        /// </summary>
        public void OnOpenQuestClear()
        {
            gameObject.SetActive(true);
            SetState(State.GetQuestClearProcessing);

            StartCoroutine(
                CheckCurrentProgressTask()
            );
        }
        
        /// <summary>
        /// クエスト完了ダイアログを閉じる
        /// </summary>
        public void ClickToQuestClearClose()
        {
            SetState(State.MainMenu);
        }

        /// <summary>
        /// クエストリストの表示
        /// </summary>
        /// <param name="quests"></param>
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
                    questView.Initialize(new QuestInformation(quest, _questModel.CurrentCompletedQuestList),
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

            StartCoroutine(
                EndTask(
                    _questModel.Progress.Rewards,
                    true
                )
            );
        }
        
        /// <summary>
        /// クエスト失敗（破棄）をリクエスト
        /// </summary>
        public void OnSendFailedResult()
        {
            SetState(State.SendFailedResult);

            StartCoroutine(
                EndTask(
                    _questModel.Progress.Rewards,
                    false
                )
            );
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
            AsyncResult<object> result = null;
            yield return _questModel.QuestEnd(
                r => result = r,
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                _questSetting.questNamespaceName,
                isComplete,
                rewards,
                MoneyModel.Slot,
                _questSetting.distributorNamespaceName,
                _questSetting.questKeyId,
                _jobQueueModel.GetSheetCompleteAction(),
                _questSetting.onEnd,
                _questSetting.onError
            );

            if (result.Error != null)
            {
                SetState(State.Error);

                yield break;
            }

            SetState(State.MainMenu);
            
            SetQuestState(QuestState.None);

            if (isComplete)
            {
                UIManager.Instance.OpenDialog1("Notice", "クエスト完了");
            }
            else
            {
                UIManager.Instance.OpenDialog1("Notice", "クエスト失敗");
            }
        }
    }
}