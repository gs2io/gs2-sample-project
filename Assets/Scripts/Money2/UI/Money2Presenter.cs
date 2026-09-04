using System.Collections;
using Gs2.Unity.Gs2Distributor.Result;
using Gs2.Unity.Gs2JobQueue.Model;
using Gs2.Unity.Gs2Money2.Model;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
#endif

namespace Gs2.Sample.Money2
{
    public class Money2Presenter : MonoBehaviour
    {
        [SerializeField] public Money2Setting _money2Setting;

        [SerializeField] private Money2Model _money2Model;
        [SerializeField] private Money2View _money2View;

        // Start is called before the first frame update
        void Start()
        {
            Assert.IsNotNull(_money2Setting);
            Assert.IsNotNull(_money2Model);
        }
        
        /// <summary>
        /// 課金通貨の初期化
        /// </summary>
        public IEnumerator Initialize()
        {
            UIManager.Instance.AddLog("Money2Presenter::Initialize");
            
            yield return Refresh();
        }
        
#if GS2_ENABLE_UNITASK
        public async UniTask InitializeAsync()
        {
            UIManager.Instance.AddLog("Money2Presenter::InitializeAsync");
            
            await RefreshAsync();
        }
#endif
        
        public void OnUpdateWallet()
        {
            // 残高が変化した直後の更新なので、キャッシュを破棄して取得する
            // This refresh happens right after the balance changed, so discard the cache before fetching
#if GS2_ENABLE_UNITASK
            RefreshAsync(true).Forget();
#else
            StartCoroutine(
                Refresh(true)
            );
#endif
        }
        
        public IEnumerator Refresh(bool invalidateCache = false)
        {
            void RefreshMoneyAction(
                EzWallet wallet
            )
            {
                _money2Setting.onGetWallet.RemoveListener(RefreshMoneyAction);
                
                _money2View.SetMoney(wallet.Summary.Free + wallet.Summary.Paid);
            }

            _money2Setting.onGetWallet.AddListener(RefreshMoneyAction);
            
            yield return _money2Model.GetWallet(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _money2Setting.moneyNamespaceName,
                _money2Setting.onGetWallet,
                _money2Setting.onError,
                invalidateCache
            );
        }
#if GS2_ENABLE_UNITASK
        public async UniTask RefreshAsync(bool invalidateCache = false)
        {
            void RefreshMoneyAction(
                EzWallet wallet
            )
            {
                _money2Setting.onGetWallet.RemoveListener(RefreshMoneyAction);
                
                _money2View.SetMoney(wallet.Summary.Free + wallet.Summary.Paid);
            }

            _money2Setting.onGetWallet.AddListener(RefreshMoneyAction);
            
            await _money2Model.GetWalletAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _money2Setting.moneyNamespaceName,
                _money2Setting.onGetWallet,
                _money2Setting.onError,
                invalidateCache
            );
        }
#endif
        
        public int GetWalletBalance()
        {
            return _money2Model.Wallet.Summary.Free + _money2Model.Wallet.Summary.Paid;
        }
    }
}