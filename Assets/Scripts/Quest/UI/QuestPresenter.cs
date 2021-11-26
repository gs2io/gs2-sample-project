using System.Collections;
using System.Collections.Generic;
using Gs2.Core;
using Gs2.Core.Exception;
using Gs2.Sample.Core.Runtime;
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
        [SerializeField] private QuestSetting _questSetting;
        
        [SerializeField] private QuestModel _questModel;
        
        [SerializeField] private SelectQuestView _selectQuestView;
        [SerializeField] private QuestClearView _questClearView;

        [SerializeField] private StaminaPresenter _staminaPresenter;
        
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
            
            /// <summary>
            /// クエストグループ情報を取得中
            /// </summary>
            GetQuestGroupProcessing,
            /// <summary>
            /// クエストグループ情報の取得に失敗
            /// </summary>
            GetQuestGroupFailed,
            
            /// <summary>
            /// クエストグループ メニュー
            /// </summary>
            CreateQuestGroupMenu,

            /// <summary>
            /// クエストグループ選択メニューを開く
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
            CreateQuestMenu,
            /// <summary>
            /// クエストを開始
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
            /// クエストを進行中
            /// </summary>
            PlayGame,
            
            /// <summary>
            /// クエスト完了ダイアログを開く
            /// </summary>
            CreateQuestClear,

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
        /// 現在のステータス
        /// </summary>
        private State _questSelectState = State.MainMenu;
        
        // Start is called before the first frame update
        void Start()
        {
            Assert.IsNotNull(_questSetting);
            Assert.IsNotNull(_questModel);

            _selectQuestView.OnCloseEvent();
        }
        
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
        
        private void SetState(State _state)
        {
            if (_questSelectState != _state)
            {
                switch (_state)
                {
                    default:
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
                    case State.CreateQuestGroupMenu:
                        UIManager.Instance.CloseProcessing();
                        _selectQuestView.OnOpenQuestGroupMenuEvent();
                        break;
                    
                    // クエストグループを選択
                    case State.SelectQuestGroup:
                        _selectQuestView.OnCloseQuestGroupMenuEvent();
                        UIManager.Instance.OpenProcessing();
                        break;
                    
                    // クエスト情報を取得中
                    case State.GetQuestProcessing:
                        _selectQuestView.OnOpenQuestMenuEvent();
                        UIManager.Instance.CloseProcessing();
                        break;
                    
                    // クエスト メニューを開く
                    case State.CreateQuestMenu:
                        UIManager.Instance.CloseProcessing();
                        _selectQuestView.OnOpenQuestMenuEvent();
                        break;
                    
                    // クエストを選択
                    case State.SelectQuest:
                        _selectQuestView.OnCloseQuestMenuEvent();
                        break;
                    
                    // クエスト 完了を開く
                    case State.CreateQuestClear:
                        UIManager.Instance.CloseProcessing();
                        _selectQuestView.OnOpenQuestClearEvent();
                        break;
                    case State.QuestProgressError:
                        break;
                    
                    // クエストを完了をリクエスト
                    case State.SendCompleteResult:
                        _selectQuestView.OnCloseQuestClearEvent();
                        break;
                    // クエストを失敗をリクエスト
                    case State.SendFailedResult:
                        _selectQuestView.OnCloseQuestClearEvent();
                        break;
                }
                UIManager.Instance.SetQuestStateText(_state.ToString());
            }
            
            _questSelectState = _state;
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
            Validate();
            
            {
                AsyncResult<EzListQuestGroupsResult> result = null;
                yield return _questModel.GetQuestGroups(
                    r => result = r,
                    _gs2Client.Client,
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
                SetState(State.CreateQuestGroupMenu);
            }

            {
                AsyncResult<EzDescribeCompletedQuestListsResult> result = null;
                yield return _questModel.GetCompleteQuests(
                    r => result = r,
                    _gs2Client.Client,
                    _session.Session,
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

            SetState(State.CreateQuestGroupMenu);

        }
        
        /// <summary>
        /// クエストグループの項目UI操作
        /// </summary>
        /// <param name="questGroups"></param>
        private void OnGetListGroupQuestModelFunc(List<EzQuestGroupModel> questGroups)
        {
            _questModel.QuestGroups = questGroups;
        
            _selectQuestView.QuestGroupInfo.SetActive(false);

            if (_selectQuestView.questGroupsContent != null)
            {
                foreach (Transform child in _selectQuestView.questGroupsContent.transform)
                {
                    if (child != null && child.gameObject != _selectQuestView.QuestGroupInfo)
                    {
                        Destroy(child.gameObject);
                    }
                }

                foreach (var questGroup in questGroups)
                {
                    var questGroupInfo = Instantiate<GameObject>(_selectQuestView.QuestGroupInfo,
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
            if (_questSelectState == State.CreateQuestGroupMenu)
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
            Validate();
            
            {
                AsyncResult<EzListQuestsResult> result = null;
                yield return _questModel.GetQuests(
                    r => result = r,
                    _gs2Client.Client,
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
                SetState(State.CreateQuestMenu);
            }

            // クエストメニューを開く
            SetState(State.CreateQuestMenu);
        }

        /// <summary>
        /// クエストを選択する
        /// </summary>
        public void ClickToSelect(EzQuestModel quest)
        {
            if (_questSelectState == State.CreateQuestMenu)
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
                _gs2Client.Client,
                _session.Session,
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

            _questModel.Progress = result.Result;
            
            _staminaPresenter.OnUpdateStamina();
            
            SetState(State.StartQuestSuccess);
        }
        
        /// <summary>
        /// クエスト完了ダイアログを開く
        /// </summary>
        public void OnOpenQuestClear()
        {
            gameObject.SetActive(true);
            SetState(State.CreateQuestClear);

            StartCoroutine(
                CheckCurrentProgressTask()
            );
        }
        
        /// <summary>
        /// クエスト完了ダイアログを閉じる
        /// </summary>
        public void ClickToQuestClearClose()
        {
            SetState(State.PlayGame);
        }

        /// <summary>
        /// 現在進行しているクエストのチェック
        /// </summary>
        /// <returns></returns>
        public IEnumerator CheckCurrentProgressTask()
        {
            Validate();

            AsyncResult<EzGetProgressResult> result = null;
            yield return _questModel.GetProgress(
                r => result = r,
                _gs2Client.Client,
                _session.Session,
                _questSetting.questNamespaceName,
                _questSetting.onGetProgress,
                _questSetting.onError
            );

            if (result.Error != null)
            {
                if (result.Error is NotFoundException)
                {
                    SetState(State.QuestProgressError);
                    
                    _questClearView.SetProgress("進行しているクエストはありません。", "");
                    _questClearView.SetInteractable(false);
                    
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

            _questClearView.SetProgress(_questModel.SelectedQuest.Metadata, _questModel.Progress.RandomSeed.ToString());
            _questClearView.SetInteractable(true);
        }
        
        /// <summary>
        /// クエストリストの表示
        /// </summary>
        /// <param name="quests"></param>
        private void OnGetListQuestModel(List<EzQuestModel> quests)
        {
            _selectQuestView.QuestInfo.SetActive(false);
            
            if (_selectQuestView.questsContent != null)
            {
                foreach (Transform child in _selectQuestView.questsContent.transform)
                {
                    if (child != null && child.gameObject != _selectQuestView.QuestInfo)
                    {
                        Destroy(child.gameObject);
                    }
                }

                foreach (var quest in quests)
                {
                    var questInfo = Instantiate<GameObject>(_selectQuestView.QuestInfo,
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
            Validate();
            
            {
                AsyncResult<object> result = null;
                yield return _questModel.QuestEnd(
                    r => result = r,
                    _gs2Client.Client,
                    _session.Session,
                    _questSetting.questNamespaceName,
                    rewards,
                    isComplete,
                    MoneyModel.Slot,
                    _questSetting.distributorNamespaceName,
                    _questSetting.questKeyId,
                    _questSetting.onEnd,
                    _questSetting.onError
                );

                if (result.Error != null)
                {
                    SetState(State.Error);
                    
                    yield break;
                }
            }

            SetState(State.MainMenu);

            if (isComplete)
            {
                UIManager.Instance.OpenDialog1("Notice", "クエスト完了");
            }
            else
            {
                UIManager.Instance.OpenDialog1("Notice","クエスト失敗");
            }
            
            _staminaPresenter.OnUpdateStamina();
        }
        

    }
}