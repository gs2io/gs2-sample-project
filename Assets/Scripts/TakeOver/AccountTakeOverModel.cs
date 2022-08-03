using System.Collections;
using System.Collections.Generic;
using Gs2.Core.Exception;
using Gs2.Unity.Core;
using Gs2.Unity.Gs2Account.Model;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Events;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
#endif

namespace Gs2.Sample.AccountTakeOver
{
    public enum TakeOverType
    {
        Email = 0,
        Platform = 1,
    }
    
    public class AccountTakeOverModel : MonoBehaviour
    {
        /// <summary>
        /// 現在有効な引継ぎ設定一覧
        /// List of currently active transfer settings
        /// </summary>
        public List<EzTakeOver> takeOverSettings = new List<EzTakeOver>();

        /// <summary>
        /// 引継ぎの種類
        /// Type of takeover
        /// </summary>
        public int type;
        
        /// <summary>
        /// ユーザ識別子
        /// user identifier
        /// </summary>
        public string userIdentifier;
        
        /// <summary>
        /// パスワード
        /// password
        /// </summary>
        public string password;
        
        /// <summary>
        /// Email設定の取得
        /// Retrieving Email Settings
        /// </summary>
        public EzTakeOver GetEmailSetting()
        {
            return takeOverSettings.Find(takeOver => takeOver.Type == (int) TakeOverType.Email);
        }
        
        /// <summary>
        /// プラットフォーム設定の取得
        /// Retrieving Platform Settings
        /// </summary>
        public EzTakeOver GetPlatformSetting()
        {
            return takeOverSettings.Find(takeOver => takeOver.Type == (int) TakeOverType.Platform);
        }

        /// <summary>
        /// 設定済みの引継ぎ設定一覧を取得
        /// Retrieve list of transfer settings that have been set up
        /// </summary>
#if GS2_ENABLE_UNITASK
        public async UniTask<Gs2Exception> ListAccountTakeOverSettingsAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string accountNamespaceName,
            ErrorEvent onError
        )
        {
            var domain = gs2.Account.Namespace(
                namespaceName: accountNamespaceName
            ).Me(
                gameSession: gameSession
            );
            try
            {
                takeOverSettings = await domain.TakeOversAsync().ToListAsync();
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e);
                return e;
            }
            return null;
        }
#endif
        public IEnumerator ListAccountTakeOverSettings(
            UnityAction<Gs2Exception> callback,
            Gs2Domain gs2,
            GameSession gameSession,
            string accountNamespaceName,
            ErrorEvent onError
        )
        {
            var _takeOver = new List<EzTakeOver>();
            var it = gs2.Account.Namespace(
                namespaceName: accountNamespaceName
            ).Me(
                gameSession: gameSession
            ).TakeOvers();
            while (it.HasNext())
            {
                yield return it.Next();
                if (it.Error != null)
                {
                    onError.Invoke(it.Error);
                    callback.Invoke(it.Error);
                    break;
                }

                if (it.Current != null)
                {
                    _takeOver.Add(it.Current);
                }
            }
            
            takeOverSettings = _takeOver;
            
            callback.Invoke(null);
        }

        /// <summary>
        /// 引継ぎ設定を追加
        /// Added takeover settings
        /// </summary>
#if GS2_ENABLE_UNITASK
        public async UniTask<Gs2Exception> AddAccountTakeOverSettingAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string accountNamespaceName,
            SetTakeOverEvent onSetTakeOver,
            ErrorEvent onError
        )
        {
            var domain = gs2.Account.Namespace(
                namespaceName: accountNamespaceName
            ).Me(
                gameSession: gameSession
            ).TakeOver(
                type: type
            );
            try
            {
                var result = await domain.AddTakeOverSettingAsync(
                    userIdentifier: userIdentifier,
                    password: password
                );
                var item = await result.ModelAsync();
                
                onSetTakeOver.Invoke(item);
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e);
                return e;
            }

            return null;
        }
#endif
        public IEnumerator AddAccountTakeOverSetting(
            UnityAction<Gs2Exception> callback,
            Gs2Domain gs2,
            GameSession gameSession,
            string accountNamespaceName,
            SetTakeOverEvent onSetTakeOver,
            ErrorEvent onError
        )
        {
            var domain = gs2.Account.Namespace(
                namespaceName: accountNamespaceName
            ).Me(
                gameSession: gameSession
            ).TakeOver(
                type: type
            );
            var future = domain.AddTakeOverSetting(
                userIdentifier: userIdentifier,
                password: password
            );
            yield return future;
            if (future.Error != null)
            {
                onError.Invoke(future.Error);
                callback.Invoke(future.Error);
                yield break;
            }

            var future2 = future.Result.Model();
            yield return future2;
            if (future2.Error != null)
            {
                onError.Invoke(future2.Error);
                callback.Invoke(future2.Error);
                yield break;
            }
            
            onSetTakeOver.Invoke(future2.Result);
            callback.Invoke(null);
        }

        /// <summary>
        /// 引継ぎ実行
        /// taking over
        /// </summary>
#if GS2_ENABLE_UNITASK
        public async UniTask<Gs2Exception> DoAccountTakeOverAsync(
            Gs2Domain gs2,
            string accountNamespaceName,
            DoTakeOverEvent onDoTakeOver,
            ErrorEvent onError
        )
        {
            var domain = gs2.Account.Namespace(
                namespaceName: accountNamespaceName
            );
            try
            {
                var result = await domain.DoTakeOverAsync(
                    type: type,
                    userIdentifier: userIdentifier,
                    password: password
                );
                var item = await result.ModelAsync();

                onDoTakeOver.Invoke(item);
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e);
                return e;
            }

            return null;
        }
#endif
        public IEnumerator DoAccountTakeOver(
            UnityAction<Gs2Exception> callback,
            Gs2Domain gs2,
            string accountNamespaceName,
            DoTakeOverEvent onDoTakeOver,
            ErrorEvent onError
        )
        {
            var domain = gs2.Account.Namespace(
                namespaceName: accountNamespaceName
            );
            var future = domain.DoTakeOver(
                type: type,
                userIdentifier: userIdentifier,
                password: password
            );
            yield return future;
            if (future.Error != null)
            {
                onError.Invoke(future.Error);
                callback.Invoke(future.Error);
                yield break;
            }

            var future2 = future.Result.Model();
            yield return future2;
            if (future2.Error != null)
            {
                onError.Invoke(future2.Error);
                callback.Invoke(future2.Error);
                yield break;
            }
            
            onDoTakeOver.Invoke(
                future2.Result
            );
            callback.Invoke(null);
        }
        
        /// <summary>
        /// 引継ぎ設定を削除
        /// Delete takeover settings
        /// </summary>
#if GS2_ENABLE_UNITASK
        public async UniTask<Gs2Exception> DeleteAccountTakeOverSettingAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string accountNamespaceName,
            ErrorEvent onError
        )
        {
            var domain = gs2.Account.Namespace(
                namespaceName: accountNamespaceName
            ).Me(
                gameSession: gameSession
            ).TakeOver(
                type: type
            );
            try
            {
                var result = await domain.DeleteTakeOverSettingAsync();
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e);
                return e;
            }

            return null;
        }
#endif
        public IEnumerator DeleteAccountTakeOverSetting(
            UnityAction<Gs2Exception> callback,
            Gs2Domain gs2,
            GameSession gameSession,
            string accountNamespaceName,
            ErrorEvent onError
        )
        {
            var domain = gs2.Account.Namespace(
                namespaceName: accountNamespaceName
            ).Me(
                gameSession: gameSession
            ).TakeOver(
                type: type
            );
            var future = domain.DeleteTakeOverSetting();
            yield return future;
            if (future.Error != null)
            {
                onError.Invoke(future.Error);
                callback.Invoke(future.Error);
                yield break;
            }
            
            callback.Invoke(null);
        }
    }
}