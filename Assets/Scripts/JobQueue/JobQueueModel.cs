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
using UnityEngine.PlayerLoop;

namespace Gs2.Sample.JobQueue
{
    [Serializable]
    public class RegistJobEvent : UnityEvent
    {
    }
    
    [Serializable]
    public class ExecJobEvent : UnityEvent<EzJob, EzJobResultBody>
    {
    }
    
    public class JobQueueModel : MonoBehaviour
    {
        public RegistJobEvent onRegistJob = new RegistJobEvent();
        public ExecJobEvent onExecJob = new ExecJobEvent();
        
        private string _jobQueueNamespaceName;
        
        private RunJobEvent _onRunJob;
        private ErrorEvent _onError;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="body"></param>
        /// <param name="isLastJob"></param>
        void OnRunJob(EzJob job, EzJobResultBody body, bool isLastJob)
        {
            if (!isLastJob)
            {
                onRegistJob.Invoke();
            }

            if (job != null)
            {
                onExecJob.Invoke(job, body);
            }
        }

        void OnError(Gs2Exception e)
        {
            onRegistJob.Invoke();
        }

        public void Initialize(
            string jobQueueNamespaceName,
            RunJobEvent onRunJob,
            ErrorEvent onError
        )
        {
            _jobQueueNamespaceName = jobQueueNamespaceName;
            
            _onRunJob = onRunJob;
            _onError = onError;

            _onRunJob.AddListener(OnRunJob);
            _onError.AddListener(OnError);
            
            onRegistJob.AddListener(OnPushJob);
        }

        public void Finish()
        {
            onRegistJob.RemoveListener(OnPushJob);
        }

        void OnPushJob()
        {
            StartCoroutine(
                Run()
            );
        }
        
        public IEnumerator Run()
        {
            yield return Run(
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                _jobQueueNamespaceName,
                _onRunJob,
                _onError
            );
        }
        
        /// <summary>
        /// ジョブキューの実行
        /// </summary>
        /// <param name="client"></param>
        /// <param name="gameSession"></param>
        /// <param name="jobQueueNamespaceName"></param>
        /// <param name="onRunJob"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public static IEnumerator Run(
            Client client,
            GameSession gameSession,
            string jobQueueNamespaceName,
            RunJobEvent onRunJob,
            ErrorEvent onError
        )
        {
            AsyncResult<EzRunResult> result = null;
            yield return client.JobQueue.Run(
                r => { result = r; },
                gameSession,
                jobQueueNamespaceName
            );

            if (result.Error != null)
            {
                Debug.LogError(result.Error);
                onError.Invoke(
                    result.Error
                );
                yield break;
            }

            var job = result.Result.Item;
            var jobResult = result.Result.Result;
            var isLastJob = result.Result.IsLastJob;

            onRunJob.Invoke(job, jobResult, isLastJob);
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