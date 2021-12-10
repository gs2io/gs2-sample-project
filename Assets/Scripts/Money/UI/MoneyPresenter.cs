using System.Collections;
using Gs2.Sample.Core.Runtime;
using Gs2.Unity.Gs2Distributor.Result;
using Gs2.Unity.Gs2JobQueue.Model;
using Gs2.Unity.Gs2Money.Model;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Gs2.Sample.Money
{
    public class MoneyPresenter : MonoBehaviour
    {
        [SerializeField] public MoneySetting _moneySetting;

        [SerializeField] private MoneyModel _moneyModel;
        [SerializeField] private MoneyView _moneyView;

        /// <summary>
        /// Gs2Client
        /// </summary>
        private Gs2Client _gs2Client;
        /// <summary>
        /// Gs2GameSession
        /// </summary>
        private Gs2GameSession _session;
        
        // Start is called before the first frame update
        void Start()
        {
            Assert.IsNotNull(_moneySetting);
            Assert.IsNotNull(_moneyModel);
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
        
        /// <summary>
        /// 有償財貨の初期化
        /// </summary>
        public IEnumerator Initialize()
        {
            UIManager.Instance.AddLog("MoneyPresenter::Initialize");
            
            Validate();
            
            yield return Refresh();
        }
        
        public void OnUpdateWallet()
        {
            StartCoroutine(
                Refresh()
            );
        }
        
        public IEnumerator Refresh()
        {
            void RefreshMoneyAction(
                EzWallet wallet
            )
            {
                _moneySetting.onGetWallet.RemoveListener(RefreshMoneyAction);
                
                _moneyView.SetMoney(wallet.Free + wallet.Paid);
            }

            _moneySetting.onGetWallet.AddListener(RefreshMoneyAction);
            
            yield return _moneyModel.GetWallet(
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                _moneySetting.moneyNamespaceName,
                _moneySetting.onGetWallet,
                _moneySetting.onError
            );
        }
        
        public UnityAction<EzJob, EzJobResultBody> GetJobQueueAction()
        {
            return (job, jobResult) =>
            {
                Debug.Log("MoneyPresenter::GetJobQueueAction");
            };
        }

        public UnityAction<EzStampTask, EzRunStampTaskResult> GetTaskCompleteAction()
        {
            return (task, taskResult) =>
            {
                Debug.Log("MoneyPresenter::StateMachineOnDoneStampTask");

                if (task.Action == "Gs2Money:WithdrawByUserId")
                {
                    StartCoroutine(
                        Refresh()
                    );
                }
            };
        }

        public UnityAction<EzStampSheet, EzRunStampSheetResult> GetSheetCompleteAction()
        {
            return (sheet, sheetResult) =>
            {
                Debug.Log("MoneyPresenter::StateMachineOnCompleteStampSheet");

                if (sheet.Action == "Gs2Money:DepositByUserId")
                {
                    StartCoroutine(
                        Refresh()
                    );
                }
            };
        }
    }
}