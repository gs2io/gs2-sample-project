using System;
using Gs2.Unity.Gs2JobQueue.Model;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.JobQueue
{
    [Serializable]
    public class RunJobEvent : UnityEvent<EzJob, EzJobResultBody, bool>
    {
    }
    
    [Serializable]
    public class JobQueueSetting : MonoBehaviour
    {
        /// <summary>
        /// GS2-JobQueue のネームスペース名
        /// </summary>
        [SerializeField]
        public string jobQueueNamespaceName;

        /// <summary>
        /// ジョブキュー実行時に発行されるイベント
        /// </summary>
        [SerializeField]
        public RunJobEvent onRunJob = new RunJobEvent();
        
        /// <summary>
        /// エラー発生時に発行されるイベント
        /// </summary>
        [SerializeField]
        public ErrorEvent onError = new ErrorEvent();
    }
}