using System;
using System.Collections;
using Gs2.Core;
using Gs2.Core.Exception;
using Gs2.Unity;
using Gs2.Unity.Gs2Distributor.Result;
using Gs2.Unity.Gs2JobQueue.Model;
using Gs2.Unity.Gs2JobQueue.Result;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.JobQueue
{
    public class JobQueueModel : MonoBehaviour
    {
        private string _jobQueueNamespaceName;
        
        [Serializable]
        public class PushJobEvent : UnityEvent
        {
        }
    
        [Serializable]
        public class ExecJobEvent : UnityEvent<EzJob, EzJobResultBody>
        {
        }
        
        [Serializable]
        public class RunJobEvent : UnityEvent<EzJob, EzJobResultBody, bool>
        {
        }
        
        /// <summary>
        /// ジョブ実行 リクエスト
        /// </summary>
        private PushJobEvent _onPushJob = new PushJobEvent();
        /// <summary>
        /// ジョブ実行後に実行
        /// </summary>
        public ExecJobEvent onExecJob = new ExecJobEvent();
        
        private ErrorEvent _onError;

        public void Initialize(
            string jobQueueNamespaceName,
            ErrorEvent onError
        )
        {
            _jobQueueNamespaceName = jobQueueNamespaceName;

            _onError = onError;
            
            _onPushJob.AddListener(OnPushJob);
        }
        
        public void Finish()
        {
            _onPushJob.RemoveListener(OnPushJob);
        }

        /// <summary>
        /// ジョブキューの実行
        /// </summary>
        void OnPushJob()
        {
            StartCoroutine(
                Run()
            );
        }
        
        /// <summary>
        /// ジョブキューの実行 リクエスト
        /// </summary>
        /// <returns></returns>
        public IEnumerator Run()
        {
            Client _client = GameManager.Instance.Cllient.Client;
            GameSession _gameSession = GameManager.Instance.Session.Session;
            
            AsyncResult<EzRunResult> result = null;
            yield return _client.JobQueue.Run(
                r => { result = r; },
                _gameSession,
                _jobQueueNamespaceName
            );

            if (result.Error != null)
            {
                Debug.LogError(result.Error);
                _onError.Invoke(
                    result.Error
                );
                yield break;
            }

            var job = result.Result.Item;
            var body = result.Result.Result;
            var isLastJob = result.Result.IsLastJob;

            if (job != null)
            {
                // ジョブキュー実行後に実行するコールバックの呼び出し
                onExecJob.Invoke(job, body);
            }
            
            // JobQueueの実行間隔は1秒以上で
            yield return new WaitForSeconds(1);
            
            if (!isLastJob)
            {
                // 次のジョブ実行をリクエスト
                _onPushJob.Invoke();
            }
        }

        public UnityAction<EzJob, EzJobResultBody> GetJobQueueAction()
        {
            return (job, jobResult) =>
            {
                Debug.Log("JobQueueModel::GetJobQueueAction");
            };
        }

        public UnityAction<EzStampTask, EzRunStampTaskResult> GetTaskCompleteAction()
        {
            return (task, taskResult) =>
            {
                Debug.Log("JobQueueModel::StateMachineOnDoneStampTask");
            };
        }

        public UnityAction<EzStampSheet, EzRunStampSheetResult> GetSheetCompleteAction()
        {
            return (sheet, sheetResult) =>
            {
                Debug.Log("JobQueueModel::StateMachineOnCompleteStampSheet");

                //スタンプシートによるジョブ登録
                if (sheet.Action == "Gs2JobQueue:PushByUserId")
                {
                    OnPushJob();
                }
            };
        }
    }
}