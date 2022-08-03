using System.Collections;
using Gs2.Sample.Money;
using Gs2.Unity.Gs2Stamina.Model;
using UnityEngine;
using UnityEngine.Assertions;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
#endif

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
        private void OnGetStamina(EzStamina stamina)
        {
            _staminaStoreView.Initialize(
                stamina,
                10,
                5,
                _moneyPresenter.GetWalletBalance()
            );
        }

        public void ClickToOpenStaminaStore()
        {
            SetState(State.GetStaminaProcessing);
            
#if GS2_ENABLE_UNITASK
            GetStaminaTaskAsync().Forget();
#else
            StartCoroutine(
                GetStaminaTask()
            );
#endif
        }

        /// <summary>
        /// スタミナを取得
        /// Get Stamina
        /// </summary>
        private IEnumerator GetStaminaTask()
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
                
            if (result == null)
            {
                SetState(State.GetStaminaFailed);
                yield break;
            }
            
            OnGetStamina(_staminaModel.stamina);

            SetState(State.OpenStaminaStore);
        }
#if GS2_ENABLE_UNITASK
        /// <summary>
        /// スタミナを取得
        /// Get Stamina
        /// </summary>
        private async UniTask GetStaminaTaskAsync()
        {
            var result = await _staminaModel.GetStaminaAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _staminaSetting.staminaNamespaceName,
                _staminaSetting.staminaName,
                _staminaSetting.onGetStamina,
                _staminaSetting.onError
            );

            if (result == null)
            {
                SetState(State.GetStaminaFailed);
            }
            else
            {
                OnGetStamina(result);
                SetState(State.OpenStaminaStore);
            }
        }
#endif

        /// <summary>
        /// スタミナ回復商品を購入
        /// </summary>
        public void ClickToBuy()
        {
            if (_staminaStoreState == State.OpenStaminaStore)
            {
                SetState(State.BuyProcessing);
                
#if GS2_ENABLE_UNITASK
                BuyTaskAsync().Forget();
#else
                StartCoroutine(
                    BuyTask()
                );
#endif
            }
        }
        
        /// <summary>
        /// スタミナ回復商品を購入
        /// </summary>
        public IEnumerator BuyTask()
        {
            yield return _staminaModel.Buy(
                e =>
                {
                    SetState(e == null
                        ? State.BuySucceed
                        : State.BuyFailed);
                },
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _staminaSetting.exchangeNamespaceName,
                _staminaSetting.exchangeRateName,
                MoneyModel.Slot,
                _staminaSetting.onBuy,
                _staminaSetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        public async UniTask BuyTaskAsync()
        {
            var err = await _staminaModel.BuyAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _staminaSetting.exchangeNamespaceName,
                _staminaSetting.exchangeRateName,
                MoneyModel.Slot,
                _staminaSetting.onBuy,
                _staminaSetting.onError
            );
            
            if (err != null)
                SetState(State.BuyFailed);
            else
                SetState(State.BuySucceed);
        }
#endif

        public void ClickToClose()
        {
            SetState(State.MainMenu);
        }
    }
}