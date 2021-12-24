using System.Collections;
using System.Collections.Generic;
using Gs2.Core;
using Gs2.Core.Exception;
using Gs2.Gs2Quest.Result;
using Gs2.Sample.Money;
using Gs2.Unity;
using Gs2.Unity.Gs2Quest.Model;
using Gs2.Unity.Gs2Quest.Result;
using Gs2.Unity.Util;
using Gs2.Util.LitJson;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Quest
{
    public class QuestModel : MonoBehaviour
    {
        public List<EzQuestGroupModel> QuestGroups;

        public List<EzCompletedQuestList> CompletedQuests;

        public EzQuestGroupModel SelectedQuestGroup;
        
        public List<EzQuestModel> Quests;

        public EzQuestModel SelectedQuest;
        
        public EzProgress Progress;
        
        public EzCompletedQuestList CurrentCompletedQuestList;
        
        /// <summary>
        /// クエストグループの一覧を取得
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="client"></param>
        /// <param name="questNamespaceName"></param>
        /// <param name="onListGroupQuestModel"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator GetQuestGroups(
            UnityAction<AsyncResult<EzListQuestGroupsResult>> callback,
            Client client,
            string questNamespaceName,
            ListQuestGroupModelEvent onListGroupQuestModel,
            ErrorEvent onError
        )
        {
            AsyncResult<EzListQuestGroupsResult> result = null;
            yield return client.Quest.ListQuestGroups(
                r => { result = r; },
                questNamespaceName
            );
            
            if (result.Error != null)
            {
                onError.Invoke(
                    result.Error
                );
                callback.Invoke(result);
                yield break;
            }
            
            QuestGroups = result.Result.Items;
            
            onListGroupQuestModel.Invoke(result.Result.Items);
            
            callback.Invoke(result);
        }
        
        /// <summary>
        /// クエストの一覧を取得
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="client"></param>
        /// <param name="questNamespaceName"></param>
        /// <param name="onListQuestModel"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator GetQuests(
            UnityAction<AsyncResult<EzListQuestsResult>> callback,
            Client client,
            string questNamespaceName,
            ListQuestModelEvent onListQuestModel,
            ErrorEvent onError
        )
        {
            AsyncResult<EzListQuestsResult> result = null;
            yield return client.Quest.ListQuests(
                r => { result = r; },
                questNamespaceName,
                SelectedQuestGroup.Name
            );
            
            if (result.Error != null)
            {
                onError.Invoke(
                    result.Error
                );
                callback.Invoke(result);
                yield break;
            }
            
            Quests = result.Result.Items;
            
            onListQuestModel.Invoke(result.Result.Items);
            
            callback.Invoke(result);
        }
        
        /// <summary>
        /// 完了済みのクエストを取得
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="client"></param>
        /// <param name="session"></param>
        /// <param name="questNamespaceName"></param>
        /// <param name="onListCompletedQuestsModel"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator GetCompleteQuests(
            UnityAction<AsyncResult<EzDescribeCompletedQuestListsResult>> callback,
            Client client,
            GameSession session,
            string questNamespaceName,
            ListCompletedQuestsEvent onListCompletedQuestsModel,
            ErrorEvent onError
        )
        {
            AsyncResult<EzDescribeCompletedQuestListsResult> result = null;
            yield return client.Quest.DescribeCompletedQuestLists(
                r => { result = r; },
                session,
                questNamespaceName,
                null,
                30
            );
            
            if (result.Error != null)
            {
                onError.Invoke(
                    result.Error
                );
                callback.Invoke(result);
                yield break;
            }
            
            CompletedQuests = result.Result.Items;
            
            onListCompletedQuestsModel.Invoke(result.Result.Items);
            
            callback.Invoke(result);
        }
        
        /// <summary>
        /// クエストを開始する
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="client"></param>
        /// <param name="session"></param>
        /// <param name="questNamespaceName"></param>
        /// <param name="slot"></param>
        /// <param name="distributorNamespaceName"></param>
        /// <param name="questKeyId"></param>
        /// <param name="onStart"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator QuestStart(
            UnityAction<AsyncResult<EzProgress>> callback,
            Client client,
            GameSession session,
            string questNamespaceName,
            string distributorNamespaceName,
            string questKeyId,
            StartEvent onStart,
            ErrorEvent onError
        )
        {
            string stampSheet;
            {
                AsyncResult<EzStartResult> result = null;
                yield return client.Quest.Start(
                    r => { result = r; },
                    session,
                    questNamespaceName,
                    SelectedQuestGroup.Name,
                    SelectedQuest.Name,
                    false,
                    config: new List<EzConfig>
                    {
                        new EzConfig
                        {
                            Key = "slot",
                            Value = MoneyModel.Slot.ToString(),
                        }
                    }
                );

                if (result.Error != null)
                {
                    onError.Invoke(
                        result.Error
                    );
                    callback.Invoke(new AsyncResult<EzProgress>(null, result.Error));
                    yield break;
                }

                stampSheet = result.Result.StampSheet;
            }
            EzProgress progress = null;
            {
                var machine = new StampSheetStateMachine(
                    stampSheet,
                    client,
                    distributorNamespaceName,
                    questKeyId
                );

                Gs2Exception exception = null;
                void OnError(Gs2Exception e)
                {
                    exception = e;
                };
                
                void OnComplete(EzStampSheet sheet, Gs2.Unity.Gs2Distributor.Result.EzRunStampSheetResult stampResult)
                {
                    var json = JsonMapper.ToObject(stampResult.Result);
                    var result = CreateProgressByStampSheetResult.FromJson(json);
                    progress = EzProgress.FromModel(result.Item);
                };
                
                onError.AddListener(OnError);
                machine.OnCompleteStampSheet.AddListener(OnComplete);

                // スタンプシートを実行
                yield return machine.Execute(onError);
                
                onError.RemoveListener(OnError);
                
                if (exception != null)
                {
                    onError.Invoke(
                        exception
                    );
                    callback.Invoke(new AsyncResult<EzProgress>(null, exception));
                    yield break;
                }
            }

            Progress = progress;
            
            onStart.Invoke(Progress);
            
            callback.Invoke(new AsyncResult<EzProgress>(Progress, null));
        }
                
        /// <summary>
        /// 進行中のクエストを取得
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="client"></param>
        /// <param name="session"></param>
        /// <param name="questNamespaceName"></param>
        /// <param name="onGetProgress"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator GetProgress(
            UnityAction<AsyncResult<EzGetProgressResult>> callback,
            Client client,
            GameSession session,
            string questNamespaceName,
            GetProgressEvent onGetProgress,
            ErrorEvent onError
        )
        {
            AsyncResult<EzGetProgressResult> result = null;
            yield return client.Quest.GetProgress(
                r => { result = r; },
                session,
                questNamespaceName
            );
            
            if (result.Error != null)
            {
                if (!(result.Error is NotFoundException))
                {
                    onError.Invoke(
                        result.Error
                    );
                }

                callback.Invoke(result);
                yield break;
            }
            
            SelectedQuestGroup = result.Result.QuestGroup;
            SelectedQuest = result.Result.Quest;
            Progress = result.Result.Item;
            
            onGetProgress.Invoke(result.Result.Item);
            
            callback.Invoke(result);
        }
        
        /// <summary>
        /// クエストを完了する
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="client"></param>
        /// <param name="session"></param>
        /// <param name="questNamespaceName"></param>
        /// <param name="rewards"></param>
        /// <param name="isComplete"></param>
        /// <param name="slot"></param>
        /// <param name="distributorNamespaceName"></param>
        /// <param name="questKeyId"></param>
        /// <param name="onEnd"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator QuestEnd(
            UnityAction<AsyncResult<object>> callback,
            Client client,
            GameSession session,
            string questNamespaceName,
            bool isComplete,
            List<EzReward> rewards,
            int slot,
            string distributorNamespaceName,
            string questKeyId,
            UnityAction<EzStampSheet, Gs2.Unity.Gs2Distributor.Result.EzRunStampSheetResult> OnComplete,
            EndEvent onEnd,
            ErrorEvent onError
        )
        {
            string stampSheet;
            {
                AsyncResult<EzEndResult> result = null;
                yield return client.Quest.End(
                    r => { result = r; },
                    session,
                    questNamespaceName,
                    isComplete,
                    rewards,
                    Progress.TransactionId,
                    new List<EzConfig>
                    {
                        new EzConfig
                        {
                            Key = "slot",
                            Value = slot.ToString(),
                        }
                    }
                );

                if (result.Error != null)
                {
                    onError.Invoke(
                        result.Error
                    );
                    callback.Invoke(new AsyncResult<object>(null, result.Error));
                    yield break;
                }

                // スタンプシートを取得
                stampSheet = result.Result.StampSheet;
            }
            {
                var machine = new StampSheetStateMachine(
                    stampSheet,
                    client,
                    distributorNamespaceName,
                    questKeyId
                );

                Gs2Exception exception = null;
                void OnError(Gs2Exception e)
                {
                    exception = e;
                }

                machine.OnCompleteStampSheet.AddListener(OnComplete);
                onError.AddListener(OnError);
                
                // スタンプシートの実行
                yield return machine.Execute(onError);
                
                onError.RemoveListener(OnError);
                
                if (exception != null)
                {
                    // スタンプシート実行エラー
                    callback.Invoke(new AsyncResult<object>(null, exception));
                    yield break;
                }
            }
            // クエストを完了
            
            onEnd.Invoke(Progress, rewards, isComplete);
            
            callback.Invoke(new AsyncResult<object>(Progress, null));
        }
    }
}