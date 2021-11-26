﻿using System;
using System.Collections;
using Gs2.Core;
using Gs2.Core.Util;
using Gs2.Sample.Core.Runtime;
using Gs2.Unity;
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
            Assert.IsNotNull(_staminaSetting);
            Assert.IsNotNull(_staminaModel);
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
        /// スタミナの初期化
        /// </summary>
        /// <returns></returns>
        public IEnumerator Initialize()
        {
            UIManager.Instance.AddLog("Stamina::Initialize");
         
            Validate();
            
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
                _gs2Client.Client,
                _staminaSetting.staminaNamespaceName,
                _staminaSetting.staminaModelName,
                _staminaSetting.onGetStaminaModel,
                _staminaSetting.onError
            );

            yield return Refresh();
        }
        
        public void Finalize()
        {
            _gs2Client = null;
            _session = null;
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
            
            StartCoroutine(
                ConsumeStamina(
                    _gs2Client.Client,
                    _session.Session,
                    _staminaSetting.staminaNamespaceName,
                    _staminaModel.staminaModel,
                    consumeValue,
                    _staminaSetting.onConsumeStamina,
                    _staminaSetting.onGetStamina,
                    _staminaSetting.onError
                )
            );
        }
        
        public IEnumerator ConsumeStamina(
            Client client,
            GameSession session,
            string staminaNamespaceName,
            EzStaminaModel staminaModel,
            int consumeValue,
            ConsumeStaminaEvent onConsumeStamina,
            GetStaminaEvent onGetStamina,
            Gs2.Unity.Util.ErrorEvent onError
        )
        {
            AsyncResult<EzConsumeResult> result = null;
            yield return client.Stamina.Consume(
                r =>
                {
                    result = r;
                },
                session,
                staminaNamespaceName,
                staminaModel.Name,
                consumeValue
            );
            
            if (result.Error != null)
            {
                onError.Invoke(
                    result.Error
                );
                yield break;
            }

            var stamina = result.Result.Item;

            onConsumeStamina.Invoke(staminaModel, stamina, consumeValue);
            onGetStamina.Invoke(staminaModel, stamina);
            
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
            void RefreshStaminaAction(
                EzStaminaModel staminaModelTemp, 
                EzStamina stamina
            )
            {
                if (staminaModelTemp.Name != _staminaModel.staminaModel.Name)
                {
                    return;
                }

                _staminaModel.stamina = stamina;
                _staminaView.SetStamina(stamina);
                
                _staminaSetting.onGetStamina.RemoveListener(RefreshStaminaAction);
                
                UIManager.Instance.AddLog("stamina.Value : " + stamina.Value);
            }

            _staminaSetting.onGetStamina.AddListener(RefreshStaminaAction);
            
            yield return GetStamina(
                _staminaModel.staminaModel,
                _staminaSetting.onGetStamina,
                _staminaSetting.onError
            );
        }
        
        /// <summary>
        /// 現在のスタミナの取得
        /// </summary>
        /// <param name="client"></param>
        /// <param name="staminaNamespaceName"></param>
        /// <param name="staminaModel"></param>
        /// <param name="onGetStamina"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator GetStamina(
            EzStaminaModel staminaModel,
            GetStaminaEvent onGetStamina,
            Gs2.Unity.Util.ErrorEvent onError
        )
        {
            Validate();
            
            AsyncResult<EzGetStaminaResult> result = null;
            yield return _gs2Client.Client.Stamina.GetStamina(
                r =>
                {
                    result = r;
                },
                _session.Session,
                _staminaSetting.staminaNamespaceName,
                _staminaSetting.staminaName
            );
            
            if (result.Error != null)
            {
                onError.Invoke(
                    result.Error
                );
                yield break;
            }

            _staminaModel.stamina = result.Result.Item;
            
            _staminaView.SetStamina(_staminaModel.stamina);
            
            onGetStamina.Invoke(staminaModel, _staminaModel.stamina);
        }
        
        public UnityAction<EzStampTask, EzRunStampTaskResult> GetTaskCompleteAction()
        {
            return (task, taskResult) =>
            {
                UIManager.Instance.AddLog("GetTaskCompleteAction");

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
                UIManager.Instance.AddLog("GetSheetCompleteAction");
                
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