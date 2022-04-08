using System.Collections;
using Gs2.Core;
using Gs2.Sample.Core;
using Gs2.Sample.Money;
using Gs2.Unity.Gs2Stamina.Model;
using Gs2.Unity.Gs2Stamina.Result;
using UnityEngine;
using UnityEngine.Assertions;

namespace Gs2.Sample.Stamina
{
    public class StaminaStorePresenter : MonoBehaviour
    {
        [SerializeField]
        private StaminaSetting _staminaSetting;

        [SerializeField]
        private StaminaModel _staminaModel;

        [SerializeField]
        private StaminaStoreView _staminaStoreView;

        [SerializeField]
        private StaminaPresenter _staminaPresenter;
        [SerializeField]
        private MoneyPresenter _moneyPresenter;

        public enum State
        {
            MainMenu,
            
            /// <summary>
            /// スタミナ情報を取得中
            /// </summary>
            GetStaminaProcessing,
            /// <summary>
            /// スタミナ情報の取得に失敗
            /// </summary>
            GetStaminaFailed,
            
            /// <summary>
            /// スタミナ回復ストアを開く
            /// </summary>
            OpenStaminaStore,

            /// <summary>
            /// スタミナ回復商品を購入
            /// </summary>
            BuyProcessing,
            /// <summary>
            /// スタミナ回復商品購入に成功
            /// </summary>
            BuySucceed,
            /// <summary>
            /// スタミナ回復商品購入に失敗
            /// </summary>
            BuyFailed,
        }
        
        /// <summary>
        /// 現在のステート
        /// </summary>
        private State _staminaStoreState = State.MainMenu;

        private void Start()
        {
            Assert.IsNotNull(_staminaSetting);
            Assert.IsNotNull(_staminaModel);
            Assert.IsNotNull(_staminaStoreView);
            Assert.IsNotNull(_staminaPresenter);
            Assert.IsNotNull(_moneyPresenter);
            
            _staminaStoreView.OnCloseEvent();
        }

        private void SetState(State _state)
        {
            if (_staminaStoreState != _state)
            {
                switch (_state)
                {
                    default:
                        _staminaStoreView.OnCloseEvent();
                        break;
                    case State.GetStaminaProcessing:
                        _staminaStoreView.OnCloseEvent();
                        UIManager.Instance.OpenProcessing();
                        break;
                    case State.OpenStaminaStore:
                        UIManager.Instance.CloseProcessing();
                        _staminaStoreView.OnOpenEvent();
                        break;

                    case State.BuyProcessing:
                        UIManager.Instance.OpenProcessing();
                        break;
                    
                    case State.BuySucceed:
                        UIManager.Instance.CloseProcessing();
                        _staminaStoreView.OnCloseEvent();
                        
                        _moneyPresenter.OnUpdateWallet();
                        
                        UIManager.Instance.OpenDialog1("Notice","StaminaPurchase");
                        _staminaPresenter.OnUpdateStamina();
                        break;

                    case State.BuyFailed:
                        UIManager.Instance.CloseProcessing();
                        _staminaStoreView.OnCloseEvent();
                        break;
                }
            }
            _staminaStoreState = _state;
        }

        /// <summary>
        /// 商品情報の初期化
        /// </summary>
        /// <param name="stamina"></param>
        private void OnGetStamina(EzStamina stamina)
        {
            _staminaModel.stamina = stamina;

            _staminaStoreView.Initialize(
                stamina,
                10,
                5,
                () => { ClickToBuy(); }
            );
        }

        public void ClickToOpenStaminaStore()
        {
            SetState(State.GetStaminaProcessing);
            
            StartCoroutine(
                GetStaminaTask()
            );
        }

        /// <summary>
        /// スタミナを取得
        /// </summary>
        /// <returns></returns>
        private IEnumerator GetStaminaTask()
        {
            AsyncResult<EzGetStaminaResult> result = null;
            yield return _staminaModel.GetStamina(
                r => result = r,
                GameManager.Instance.Cllient.Client,
                GameManager.Instance.Session.Session,
                _staminaSetting.staminaNamespaceName,
                _staminaSetting.staminaName,
                _staminaSetting.onGetStamina,
                _staminaSetting.onError
            );
                
            if (result.Error != null)
            {
                SetState(State.GetStaminaFailed);
                yield break;
            }
            
            OnGetStamina(_staminaModel.stamina);
            
            SetState(State.OpenStaminaStore);
        }
        
        /// <summary>
        /// スタミナ回復商品を購入
        /// </summary>
        public void ClickToBuy()
        {
            if (_staminaStoreState == State.OpenStaminaStore)
            {
                SetState(State.BuyProcessing);
                
                StartCoroutine(
                    _staminaModel.Buy(
                        r =>
                        {
                            SetState(r.Error == null
                                ? State.BuySucceed
                                : State.BuyFailed);
                        },
                        GameManager.Instance.Cllient.Client,
                        GameManager.Instance.Session.Session,
                        _staminaSetting.exchangeNamespaceName,
                        _staminaSetting.exchangeRateName,
                        MoneyModel.Slot,
                        _staminaSetting.distributorNamespaceName,
                        _staminaSetting.exchangeKeyId,
                        _staminaSetting.onBuy,
                        _staminaSetting.onError
                    )
                );
            }
        }

        public void ClickToClose()
        {
            SetState(State.MainMenu);
        }
    }
}