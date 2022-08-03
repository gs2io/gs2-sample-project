using System;
using System.Collections;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
#endif
using Gs2.Core;
using Gs2.Core.Exception;
using Gs2.Core.Util;
using Gs2.Unity.Core;
using Gs2.Unity.Core.Exception;
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
            
            yield return _staminaModel.GetStaminaModel(
                GameManager.Instance.Domain,
                _staminaSetting.staminaNamespaceName,
                _staminaSetting.staminaName,
                _staminaSetting.onGetStaminaModel,
                _staminaSetting.onError
            );

            yield return Refresh();
        }
        
#if GS2_ENABLE_UNITASK
        /// <summary>
        /// スタミナの初期化
        /// </summary>
        /// <returns></returns>
        public async UniTask InitializeAsync()
        {
            UIManager.Instance.AddLog("Stamina::InitializeAsync");
            
            await _staminaModel.GetStaminaModelAsync(
                GameManager.Instance.Domain,
                _staminaSetting.staminaNamespaceName,
                _staminaSetting.staminaName,
                _staminaSetting.onGetStaminaModel,
                _staminaSetting.onError
            );

            await RefreshAsync();
        }
#endif

        public void OnClickStamina_ConsumeButton(int consumeValue)
        {
            UIManager.Instance.AddLog("OnClickStamina_ConsumeButton");
            UIManager.Instance.CloseDialog();

#if GS2_ENABLE_UNITASK
            ConsumeStaminaAsync(consumeValue).Forget();
#else
            StartCoroutine(
                ConsumeStamina(
                    consumeValue
                )
            );
#endif
        }

        public IEnumerator ConsumeStamina(int consumeValue)
        {
            void RefreshStaminaAction(
                EzStamina stamina
            )
            {
                _staminaSetting.onGetStamina.RemoveListener(RefreshStaminaAction);

                UIManager.Instance.AddLog("stamina.Value : " + _staminaModel.stamina.Value);
            }

            _staminaSetting.onGetStamina.AddListener(RefreshStaminaAction);

            yield return _staminaModel.ConsumeStamina(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _staminaSetting.staminaNamespaceName,
                _staminaSetting.staminaName,
                consumeValue,
                _staminaSetting.onConsumeStamina,
                _staminaSetting.onGetStamina,
                _staminaSetting.onError
            );
            
            yield return Refresh();
        }

#if GS2_ENABLE_UNITASK
        async UniTask ConsumeStaminaAsync(int consumeValue)
        {
            void RefreshStaminaAction(
                EzStamina stamina
            )
            {
                _staminaSetting.onGetStamina.RemoveListener(RefreshStaminaAction);

                UIManager.Instance.AddLog("stamina.Value : " + _staminaModel.stamina.Value);
            }

            _staminaSetting.onGetStamina.AddListener(RefreshStaminaAction);

            await _staminaModel.ConsumeStaminaAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _staminaSetting.staminaNamespaceName,
                _staminaSetting.staminaName,
                consumeValue,
                _staminaSetting.onConsumeStamina,
                _staminaSetting.onGetStamina,
                _staminaSetting.onError
            );
            
            await RefreshAsync();
        }
#endif
        
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
#if GS2_ENABLE_UNITASK
            RefreshAsync().Forget();
#else
            StartCoroutine(
                Refresh()
            );
#endif
        }
        
        /// <summary>
        /// スタミナの更新
        /// </summary>
        private IEnumerator Refresh()
        {
            EzStamina result = null;
            yield return _staminaModel.GetStamina(
                r => result = r,
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _staminaSetting.staminaNamespaceName,
                _staminaSetting.staminaName,
                _staminaSetting.onGetStamina,
                _staminaSetting.onError
            );

            if (result != null)
            {
                _staminaView.SetStamina(result);
            }
        }
#if GS2_ENABLE_UNITASK
        /// <summary>
        /// スタミナの更新
        /// </summary>
        private async UniTask RefreshAsync()
        {
            var result = await _staminaModel.GetStaminaAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _staminaSetting.staminaNamespaceName,
                _staminaSetting.staminaName,
                _staminaSetting.onGetStamina,
                _staminaSetting.onError
            );
            
            if (result != null)
            {
                _staminaView.SetStamina(_staminaModel.stamina);
            }
        }
#endif
    }
}