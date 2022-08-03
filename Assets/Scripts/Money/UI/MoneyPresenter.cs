using System.Collections;
using Gs2.Unity.Gs2Distributor.Result;
using Gs2.Unity.Gs2JobQueue.Model;
using Gs2.Unity.Gs2Money.Model;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
#endif

namespace Gs2.Sample.Money
{
    public class MoneyPresenter : MonoBehaviour
    {
        [SerializeField] public MoneySetting _moneySetting;

        [SerializeField] private MoneyModel _moneyModel;
        [SerializeField] private MoneyView _moneyView;

        // Start is called before the first frame update
        void Start()
        {
            Assert.IsNotNull(_moneySetting);
            Assert.IsNotNull(_moneyModel);
        }
        
        /// <summary>
        /// 課金通貨の初期化
        /// </summary>
        public IEnumerator Initialize()
        {
            UIManager.Instance.AddLog("MoneyPresenter::Initialize");
            
            yield return Refresh();
        }
        
#if GS2_ENABLE_UNITASK
        public async UniTask InitializeAsync()
        {
            UIManager.Instance.AddLog("MoneyPresenter::InitializeAsync");
            
            await RefreshAsync();
        }
#endif
        
        public void OnUpdateWallet()
        {
#if GS2_ENABLE_UNITASK
            RefreshAsync().Forget();
#else
            StartCoroutine(
                Refresh()
            );
#endif
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
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _moneySetting.moneyNamespaceName,
                _moneySetting.onGetWallet,
                _moneySetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        public async UniTask RefreshAsync()
        {
            void RefreshMoneyAction(
                EzWallet wallet
            )
            {
                _moneySetting.onGetWallet.RemoveListener(RefreshMoneyAction);
                
                _moneyView.SetMoney(wallet.Free + wallet.Paid);
            }

            _moneySetting.onGetWallet.AddListener(RefreshMoneyAction);
            
            await _moneyModel.GetWalletAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _moneySetting.moneyNamespaceName,
                _moneySetting.onGetWallet,
                _moneySetting.onError
            );
        }
#endif
        
        public int GetWalletBalance()
        {
            return _moneyModel.Wallet.Free + _moneyModel.Wallet.Paid;
        }
    }
}