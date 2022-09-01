using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gs2.Core.Exception;
using Gs2.Sample.Money;
using Gs2.Unity.Core;
using Gs2.Unity.Gs2Quest.Model;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Events;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
#endif

namespace Gs2.Sample.Quest
{
    public class QuestModel : MonoBehaviour
    {
        public List<EzQuestGroupModel> questGroups = new List<EzQuestGroupModel>();

        public List<EzCompletedQuestList> completedQuests = new List<EzCompletedQuestList>();

        public EzQuestGroupModel selectedQuestGroup;
        
        public List<EzQuestModel> quests = new List<EzQuestModel>();

        public EzQuestModel selectedQuest;
        
        public EzProgress progress;
        
        public EzCompletedQuestList currentCompletedQuestList;
        
        /// <summary>
        /// クエストグループの一覧を取得
        /// Get a list of quest groups
        /// </summary>
        public IEnumerator GetQuestGroups(
            UnityAction<List<EzQuestGroupModel>> callback,
            Gs2Domain gs2,
            string questNamespaceName,
            ListQuestGroupModelEvent onListGroupQuestModel,
            ErrorEvent onError
        )
        {
            questGroups.Clear();
            var domain = gs2.Quest.Namespace(
                namespaceName: questNamespaceName
            );
            var it = domain.QuestGroupModels();
            while (it.HasNext())
            {
                yield return it.Next();
                if (it.Error != null)
                {
                    onError.Invoke(it.Error, null);
                    callback.Invoke(null);
                    yield break;
                }

                if (it.Current != null)
                {
                    questGroups.Add(it.Current);
                }
            }
            
            onListGroupQuestModel.Invoke(questGroups);
            callback.Invoke(questGroups);
        }
#if GS2_ENABLE_UNITASK
        public async UniTask<List<EzQuestGroupModel>> GetQuestGroupsAsync(
            Gs2Domain gs2,
            string questNamespaceName,
            ListQuestGroupModelEvent onListGroupQuestModel,
            ErrorEvent onError
        )
        {
            questGroups.Clear();
            var domain = gs2.Quest.Namespace(
                namespaceName: questNamespaceName
            );
            try
            {
                questGroups = await domain.QuestGroupModelsAsync().ToListAsync();

                onListGroupQuestModel.Invoke(questGroups);
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e, null);
                return null;
            }

            return questGroups;
        }
#endif

        /// <summary>
        /// クエストの一覧を取得
        /// Get a list of quests
        /// </summary>
        public IEnumerator GetQuests(
            UnityAction<List<EzQuestModel>> callback,
            Gs2Domain gs2,
            string questNamespaceName,
            ListQuestModelEvent onListQuestModel,
            ErrorEvent onError
        )
        {
            quests.Clear();
            var domain = gs2.Quest.Namespace(
                namespaceName: questNamespaceName
            ).QuestGroupModel(
                questGroupName: selectedQuestGroup.Name
            );
            var it = domain.QuestModels();
            while (it.HasNext())
            {
                yield return it.Next();
                if (it.Error != null)
                {
                    onError.Invoke(it.Error, null);
                    callback.Invoke(null);
                    yield break;
                }

                if (it.Current != null)
                {
                    quests.Add(it.Current);
                }
            }
            
            onListQuestModel.Invoke(quests);
            callback.Invoke(quests);
        }
#if GS2_ENABLE_UNITASK
        public async UniTask<List<EzQuestModel>> GetQuestsAsync(
            Gs2Domain gs2,
            string questNamespaceName,
            ListQuestModelEvent onListQuestModel,
            ErrorEvent onError
        )
        {
            var domain = gs2.Quest.Namespace(
                namespaceName: questNamespaceName
            ).QuestGroupModel(
                questGroupName: selectedQuestGroup.Name
            );
            try
            {
                quests = await domain.QuestModelsAsync().ToListAsync();

                onListQuestModel.Invoke(quests);
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e, null);
            }

            return quests;
        }
#endif
        
        /// <summary>
        /// 完了済みのクエストを取得
        /// Retrieve completed quests
        /// </summary>
        public IEnumerator GetCompleteQuests(
            UnityAction<List<EzCompletedQuestList>> callback,
            Gs2Domain gs2,
            GameSession gameSession,
            string questNamespaceName,
            ListCompletedQuestsEvent onListCompletedQuestsModel,
            ErrorEvent onError
        )
        {
            completedQuests.Clear();
            var domain = gs2.Quest.Namespace(
                namespaceName: questNamespaceName
            ).Me(
                gameSession: gameSession
            );
            var it = domain.CompletedQuestLists();
            while (it.HasNext())
            {
                yield return it.Next();
                if (it.Error != null)
                {
                    onError.Invoke(it.Error, null);
                    callback.Invoke(null);
                    yield break;
                }

                if (it.Current != null)
                {
                    completedQuests.Add(it.Current);
                }
            }
            
            onListCompletedQuestsModel.Invoke(completedQuests);
            callback.Invoke(completedQuests);
        }
#if GS2_ENABLE_UNITASK
        public async UniTask<List<EzCompletedQuestList>> GetCompleteQuestsAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string questNamespaceName,
            ListCompletedQuestsEvent onListCompletedQuestsModel,
            ErrorEvent onError
        )
        {
            var domain = gs2.Quest.Namespace(
                namespaceName: questNamespaceName
            ).Me(
                gameSession: gameSession
            );
            try
            {
                completedQuests = await domain.CompletedQuestListsAsync().ToListAsync();

                onListCompletedQuestsModel.Invoke(completedQuests);
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e, null);
            }

            return completedQuests;
        }
#endif

        /// <summary>
        /// クエストを開始する
        /// Start a quest
        /// </summary>
        public IEnumerator QuestStart(
            UnityAction<EzProgress> callback,
            Gs2Domain gs2,
            GameSession gameSession,
            string questNamespaceName,
            int slot,
            StartEvent onStart,
            ErrorEvent onError
        )
        {
            var domain = gs2.Quest.Namespace(
                namespaceName: questNamespaceName
            ).Me(
                gameSession: gameSession
            );
            var future = domain.Start(
                questGroupName: selectedQuestGroup.Name,
                questName: selectedQuest.Name,
                force: null,
                config: new[]
                {
                    new EzConfig
                    {
                        Key = "slot",
                        Value = MoneyModel.Slot.ToString()
                    }
                }
            );
            yield return future;
            if (future.Error != null)
            {
                onError.Invoke(future.Error, null);
                callback.Invoke(null);
                yield break;
            }

            var domain2 = future.Result.Progress();
            var futurew2 = domain2.Model();
            yield return futurew2;
            if (futurew2.Error != null)
            {
                onError.Invoke(futurew2.Error, null);
                callback.Invoke(null);
                yield break;
            }

            onStart.Invoke(futurew2.Result);
            callback.Invoke(futurew2.Result);
        }
#if GS2_ENABLE_UNITASK
        public async UniTask<EzProgress> QuestStartAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string questNamespaceName,
            int slot,
            StartEvent onStart,
            ErrorEvent onError
        )
        {
            {
                var domain = gs2.Quest.Namespace(
                    namespaceName: questNamespaceName
                ).Me(
                    gameSession: gameSession
                );
                try
                {
                    var result = await domain.StartAsync(
                        questGroupName: selectedQuestGroup.Name,
                        questName: selectedQuest.Name,
                        force: null,
                        config: new[]
                        {
                            new EzConfig
                            {
                                Key = "slot",
                                Value = slot.ToString()
                            }
                        }
                    );
                }
                catch (Gs2Exception e)
                {
                    onError.Invoke(e, null);
                    return null;
                }
            }
            {
                var domain = gs2.Quest.Namespace(
                    namespaceName: questNamespaceName
                ).Me(
                    gameSession: gameSession
                ).Progress();
                try
                {
                    progress = await domain.ModelAsync();
                }
                catch (Gs2Exception e)
                {
                    onError.Invoke(e, null);
                }

                onStart.Invoke(progress);
                
                return progress;
            }
        }
#endif

        /// <summary>
        /// 進行中のクエストを取得
        /// Get quests in progress
        /// </summary>
        public IEnumerator GetProgress(
            UnityAction<EzProgress> callback,
            Gs2Domain gs2,
            GameSession gameSession,
            string questNamespaceName,
            GetProgressEvent onGetProgress,
            ErrorEvent onError
        )
        {
            {
                var domain = gs2.Quest.Namespace(
                    namespaceName: questNamespaceName
                ).Me(
                    gameSession: gameSession
                ).Progress();
                var future = domain.Model();
                yield return future;
                if (future.Error != null)
                {
                    onError.Invoke(future.Error, null);
                    yield break;
                }

                progress = future.Result;
            }
            if (progress != null) {
                foreach (var questGroup in questGroups)
                {
                    quests.Clear();
                    var domain = gs2.Quest.Namespace(
                        namespaceName: questNamespaceName
                    ).QuestGroupModel(
                        questGroupName: questGroup.Name
                    );
                    var it = domain.QuestModels();
                    while (it.HasNext())
                    {
                        yield return it.Next();
                        if (it.Error != null)
                        {
                            onError.Invoke(it.Error, null);
                            callback.Invoke(null);
                            yield break;
                        }

                        if (it.Current != null)
                        {
                            quests.Add(it.Current);
                        }
                    }
                    var quest = quests.Find(q => q.QuestModelId == progress.QuestModelId);
                    if (quest != null)
                    {
                        selectedQuest = quest;
                        selectedQuestGroup = questGroup;
                    }
                }
            }

            onGetProgress.Invoke(progress);
            callback.Invoke(progress);
        }
#if GS2_ENABLE_UNITASK
        public async UniTask<EzProgress> GetProgressAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string questNamespaceName,
            GetProgressEvent onGetProgress,
            ErrorEvent onError
        )
        {
            var domain = gs2.Quest.Namespace(
                namespaceName: questNamespaceName
            ).Me(
                gameSession: gameSession
            ).Progress();
            try
            {
                progress = await domain.ModelAsync();

                onGetProgress.Invoke(progress);
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e, null);
                return null;
            }
            if (progress != null) {
                foreach (var questGroup in questGroups)
                {
                    quests.Clear();
                    var domain2 = gs2.Quest.Namespace(
                        namespaceName: questNamespaceName
                    ).QuestGroupModel(
                        questGroupName: questGroup.Name
                    );
                    quests = await domain2.QuestModelsAsync().ToListAsync();
                    var quest = quests.Find(q => q.QuestModelId == progress.QuestModelId);
                    if (quest != null)
                    {
                        selectedQuest = quest;
                        selectedQuestGroup = questGroup;
                    }
                }
            }
            
            onGetProgress.Invoke(progress);
            return progress;
        }
#endif
        
        /// <summary>
        /// クエストを完了する
        /// Complete the quest
        /// </summary>
        public IEnumerator QuestEnd(
            UnityAction<EzProgress> callback,
            Gs2Domain gs2,
            GameSession gameSession,
            string questNamespaceName,
            bool isComplete,
            List<EzReward> rewards,
            int slot,
            EndEvent onEnd,
            ErrorEvent onError
        )
        {
            var domain = gs2.Quest.Namespace(
                namespaceName: questNamespaceName
            ).Me(
                gameSession: gameSession
            ).Progress();
            var future = domain.End(
                isComplete: isComplete,
                rewards: rewards.ToArray(),
                config: new []
                {
                    new EzConfig
                    {
                        Key = "slot",
                        Value = slot.ToString(),
                    }
                }
            );
            yield return future;
            if (future.Error != null)
            {
                onError.Invoke(future.Error, null);
                callback.Invoke(null);
                yield break;
            }

            onEnd.Invoke(progress, rewards, isComplete);
            callback.Invoke(progress);
        }
#if GS2_ENABLE_UNITASK
        public async UniTask<Gs2Exception> QuestEndAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string questNamespaceName,
            bool isComplete,
            List<EzReward> rewards,
            int slot,
            EndEvent onEnd,
            ErrorEvent onError
        )
        {
            var domain = gs2.Quest.Namespace(
                namespaceName: questNamespaceName
            ).Me(
                gameSession: gameSession
            ).Progress();
            try
            {
                var domain2 = await domain.EndAsync(
                    isComplete: isComplete,
                    rewards: rewards.ToArray(),
                    config: new []
                    {
                        new EzConfig
                        {
                            Key = "slot",
                            Value = slot.ToString(),
                        }
                    }
                    );
                progress = await domain.ModelAsync();
                onEnd.Invoke(progress, rewards, isComplete);
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e, null);
                return e;
            }

            return null;
        }
#endif
    }
}