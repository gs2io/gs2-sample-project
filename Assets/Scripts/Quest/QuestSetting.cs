using System;
using System.Collections.Generic;
using Gs2.Unity.Gs2Quest.Model;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Quest
{
    [Serializable]
    public class ListCompletedQuestsEvent : UnityEvent<List<EzCompletedQuestList>>
    {
    }
    [Serializable]
    public class ListQuestGroupModelEvent : UnityEvent<List<EzQuestGroupModel>>
    {
    }

    [Serializable]
    public class ListQuestModelEvent : UnityEvent<List<EzQuestModel>>
    {
    }

    [Serializable]
    public class StartEvent : UnityEvent<EzProgress>
    {
    }

    [Serializable]
    public class GetProgressEvent : UnityEvent<EzProgress>
    {
    }

    [Serializable]
    public class EndEvent : UnityEvent<EzProgress, List<EzReward>, bool>
    {
    }

    [Serializable]
    public class QuestSetting : MonoBehaviour
    {
        [SerializeField]
        public string questNamespaceName;

        /// <summary>
        /// クエストグループを取得したとき
        /// </summary>
        [SerializeField]
        public ListCompletedQuestsEvent onListCompletedQuestsModel = new ListCompletedQuestsEvent();

        /// <summary>
        /// クエストグループを取得したとき
        /// </summary>
        [SerializeField]
        public ListQuestGroupModelEvent onListGroupQuestModel = new ListQuestGroupModelEvent();

        /// <summary>
        /// クエストを取得したとき
        /// </summary>
        [SerializeField]
        public ListQuestModelEvent onListQuestModel = new ListQuestModelEvent();

        /// <summary>
        /// クエストを開始したとき
        /// </summary>
        [SerializeField]
        public StartEvent onStart = new StartEvent();

        /// <summary>
        /// 進行中のクエストを取得したとき
        /// </summary>
        [SerializeField]
        public GetProgressEvent onGetProgress = new GetProgressEvent();

        /// <summary>
        /// クエストを完了したとき
        /// </summary>
        [SerializeField]
        public EndEvent onEnd = new EndEvent();

        /// <summary>
        /// エラー発生時に発行されるイベント
        /// </summary>
        [SerializeField]
        public ErrorEvent onError = new ErrorEvent();
    }
}