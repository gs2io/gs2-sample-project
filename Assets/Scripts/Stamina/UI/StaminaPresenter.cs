using System;
using System.Collections;
using Gs2.Core;
using Gs2.Core.Util;
using Gs2.Unity.Gs2Distributor.Result;
using Gs2.Unity.Gs2Stamina.Model;
using Gs2.Unity.Gs2Stamina.Result;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Gs2.Sample.Stamina
{
    public class StaminaPresenter : MonoBehaviour
    {
        [SerializeField]
        private StaminaSetting _staminaSetting;
        
        [SerializeField]
        private StaminaModel _staminaModel;
        [SerializeField]
        private StaminaView _staminaView;

        // Start is called before the first frame update
        void Start()
        {
            Assert.IsNotNull(_staminaSetting);
            Assert.IsNotNull(_staminaModel);
        }

        /// <summary>
        /// スタミナの初期化
        /// </summary>
        /// <returns></returns>
        public IEnumerator Initialize()
        {
            UIManager.Instance.AddLog("Stamina::Initialize");
            
            void OnGetStaminaModel(
                string staminaModelNameTemp,
                EzStaminaModel staminaModel
            )
            {
                _staminaSetting.onGetStaminaModel.RemoveListener(OnGetStaminaModel);

                _staminaModel.staminaModel = staminaModel;
            }

            _staminaSetting.onGetStaminaModel.AddListener(OnGetStaminaModel);
        
            yield return GetStaminaModel(
                GameManager.Instance.Client,
                _staminaSetting.staminaNamespaceName,
                _staminaSetting.staminaModelName,
                _staminaSetting.onGetStaminaModel,
                _staminaSetting.onError
            );

            yield return Refresh();
        }
        
        /// <summary>
        /// スタミナモデルの取得
        /// </summary>
        /// <param name="client"></param>
        /// <param name="staminaNamespaceName"></param>
        /// <param name="staminaModelName"></param>
        /// <param name="onGetStaminaModel"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public static IEnumerator GetStaminaModel(
            Gs2.Unity.Client client,
            string staminaNamespaceName,
            string staminaModelName,
            GetStaminaModelEvent onGetStaminaModel,
            Gs2.Unity.Util.ErrorEvent onError
        )
        {
            AsyncResult<EzGetStaminaModelResult> result = null;
            yield return client.Stamina.GetStaminaModel(
                r =>
                {
                    result = r;
                },
                staminaNamespaceName,
                staminaModelName
            );
            
            if (result.Error != null)
            {
                onError.Invoke(
                    result.Error
                );
                yield break;
            }

            var staminaModel = result.Result.Item;

            onGetStaminaModel.Invoke(staminaModelName, staminaModel);
        }
        
        public void OnClickStamina_ConsumeButton(int consumeValue)
        {
            UIManager.Instance.AddLog("OnClickStamina_ConsumeButton");
            UIManager.Instance.CloseDialog();
            
            StartCoroutine(
                ConsumeStamina(
                    consumeValue
                )
            );
        }

        public IEnumerator ConsumeStamina(int consumeValue)
        {
            void RefreshStaminaAction(
                EzStaminaModel staminaModelTemp,
                EzStamina stamina
            )
            {
                if (staminaModelTemp.Name != _staminaModel.staminaModel.Name)
                {
                    return;
                }

                _staminaSetting.onGetStamina.RemoveListener(RefreshStaminaAction);

                UIManager.Instance.AddLog("stamina.Value : " + _staminaModel.stamina.Value);
            }

            _staminaSetting.onGetStamina.AddListener(RefreshStaminaAction);

            yield return _staminaModel.ConsumeStamina(
                GameManager.Instance.Client,
                GameManager.Instance.Session,
                _staminaSetting.staminaNamespaceName,
                consumeValue,
                _staminaSetting.onConsumeStamina,
                _staminaSetting.onGetStamina,
                _staminaSetting.onError
            );
            
            yield return Refresh();
        }

        private void Update()
        {
            if (_staminaModel.stamina != null)
            {

                if (_staminaModel.stamina.Value >= _staminaModel.stamina.MaxValue)
                {
                    _staminaModel.stamina.Value = _staminaModel.stamina.MaxValue;
                    _staminaModel.stamina.NextRecoverAt = 0;
                    _staminaView.SetRecoveryTime("--:--");
                    _staminaView.SetStamina(_staminaModel.stamina);
                    return;
                }

                if (_staminaModel.stamina.NextRecoverAt == 0)
                {
                    _staminaView.SetRecoveryTime("--:--");
                }
                else
                {
                    var timeSpan = UnixTime.FromUnixTime(_staminaModel.stamina.NextRecoverAt) - DateTime.UtcNow;
                    if (timeSpan.Ticks < 0)
                    {
                        if (_staminaModel.stamina.Value >= _staminaModel.stamina.MaxValue)
                        {
                            _staminaModel.stamina.Value = _staminaModel.stamina.MaxValue;
                            _staminaModel.stamina.NextRecoverAt = 0;
                        }
                        else
                        {
                            _staminaModel.stamina.Value += _staminaModel.stamina.RecoverValue;
                            _staminaModel.stamina.NextRecoverAt +=
                                _staminaModel.stamina.RecoverIntervalMinutes * 60 * 1000;

                            timeSpan = UnixTime.FromUnixTime(_staminaModel.stamina.NextRecoverAt) - DateTime.UtcNow;
                        }
                    }

                    _staminaView.SetRecoveryTime($"{timeSpan.Minutes:00}:{timeSpan.Seconds:00}");
                    _staminaView.SetStamina(_staminaModel.stamina);
                }
            }
        }

        public void OnUpdateStamina()
        {
            StartCoroutine(
                Refresh()
            );
        }
        
        /// <summary>
        /// スタミナの更新
        /// </summary>
        /// <returns></returns>
        public IEnumerator Refresh()
        {
            AsyncResult<EzGetStaminaResult> result = null;
            yield return _staminaModel.GetStamina(
                r => result = r,
                GameManager.Instance.Client,
                GameManager.Instance.Session,
                _staminaSetting.staminaNamespaceName,
                _staminaSetting.staminaName,
                _staminaSetting.onGetStamina,
                _staminaSetting.onError
            );

            if (result.Error == null)
            {
                _staminaView.SetStamina(_staminaModel.stamina);
            }
        }
        
        public UnityAction<EzStampTask, EzRunStampTaskResult> GetTaskCompleteAction()
        {
            return (task, taskResult) =>
            {
                UIManager.Instance.AddLog("StaminaPresenter::GetTaskCompleteAction");

                if (task.Action == "Gs2Stamina:ConsumeStaminaByUserId")
                {
                    UIManager.Instance.AddLog("Gs2Stamina:ConsumeStaminaByUserId");
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
                UIManager.Instance.AddLog("StaminaPresenter::GetSheetCompleteAction");
                
                if (sheet.Action == "Gs2Stamina:RecoverStaminaByUserId")
                {
                    UIManager.Instance.AddLog("Gs2Stamina:RecoverStaminaByUserId");
                    StartCoroutine(
                        Refresh()
                    );
                }
            };
        }
    }
}