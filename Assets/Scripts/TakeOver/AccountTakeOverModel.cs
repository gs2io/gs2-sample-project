using System.Collections;
using System.Collections.Generic;
using Gs2.Core;
using Gs2.Unity;
using Gs2.Unity.Gs2Account.Model;
using Gs2.Unity.Gs2Account.Result;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Events;

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
        /// </summary>
        public List<EzTakeOver> takeOverSettings = new List<EzTakeOver>();

        /// <summary>
        /// 引継ぎの種類
        /// </summary>
        public int type;
        
        /// <summary>
        /// ユーザ識別子
        /// </summary>
        public string userIdentifier;
        
        /// <summary>
        /// パスワード
        /// </summary>
        public string password;
        
        /// <summary>
        /// Email設定の取得
        /// </summary>
        /// <returns></returns>
        public EzTakeOver GetEmailSetting()
        {
            return takeOverSettings.Find(takeOver => takeOver.Type == (int) TakeOverType.Email);
        }
        
        /// <summary>
        /// プラットフォーム設定の取得
        /// </summary>
        /// <returns></returns>
        public EzTakeOver GetPlatformSetting()
        {
            return takeOverSettings.Find(takeOver => takeOver.Type == (int) TakeOverType.Platform);
        }

        
        /// <summary>
        /// 設定済みの引継ぎ設定一覧を取得
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="client"></param>
        /// <param name="session"></param>
        /// <param name="callback"></param>
        /// <param name="accountNamespaceName"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator ListAccountTakeOverSettings(
            UnityAction<AsyncResult<EzListTakeOverSettingsResult>> callback,
            Client client,
            GameSession session,
            string accountNamespaceName,
            ErrorEvent onError
        )
        {
            AsyncResult<EzListTakeOverSettingsResult> result = null;
            yield return client.Account.ListTakeOverSettings(
                r => { result = r; },
                session,
                accountNamespaceName,
                null,
                30
            );
            
            if (result.Error != null)
            {
                onError.Invoke(
                    result.Error
                );
                callback.Invoke(result);
                yield break;
            }
            
            takeOverSettings = result.Result.Items;
            
            callback.Invoke(result);
        }

        /// <summary>
        /// 引継ぎ設定を追加
        /// </summary>
        /// <param name="client"></param>
        /// <param name="session"></param>
        /// <param name="callback"></param>
        /// <param name="accountNamespaceName"></param>
        /// <param name="onSetTakeOver"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator AddAccountTakeOverSetting(
            Client client,
            GameSession session,
            UnityAction<AsyncResult<EzAddTakeOverSettingResult>> callback,
            string accountNamespaceName,
            SetTakeOverEvent onSetTakeOver,
            ErrorEvent onError
        )
        {
            AsyncResult<EzAddTakeOverSettingResult> result = null;
            yield return client.Account.AddTakeOverSetting(
                r => { result = r; },
                session,
                accountNamespaceName,
                type,
                userIdentifier,
                password
            );
            
            if (result.Error != null)
            {
                onError.Invoke(
                    result.Error
                );
                callback.Invoke(result);
                yield break;
            }
            
            onSetTakeOver.Invoke(
                result.Result.Item
            );
            callback.Invoke(result);
        }

        /// <summary>
        /// 引継ぎ実行
        /// </summary>
        /// <param name="client"></param>
        /// <param name="callback"></param>
        /// <param name="accountNamespaceName"></param>
        /// <param name="onDoTakeOver"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator DoAccountTakeOver(
            Client client,
            UnityAction<AsyncResult<EzDoTakeOverResult>> callback,
            string accountNamespaceName,
            DoTakeOverEvent onDoTakeOver,
            ErrorEvent onError
        )
        {
            AsyncResult<EzDoTakeOverResult> result = null;
            yield return client.Account.DoTakeOver(
                r => { result = r; },
                accountNamespaceName,
                type,
                userIdentifier,
                password
            );
            
            if (result.Error != null)
            {
                onError.Invoke(
                    result.Error
                );
                callback.Invoke(result);
                yield break;
            }
            
            onDoTakeOver.Invoke(
                result.Result.Item
            );
            callback.Invoke(result);
        }
        
        /// <summary>
        /// 引継ぎ設定を削除
        /// </summary>
        /// <param name="client"></param>
        /// <param name="session"></param>
        /// <param name="callback"></param>
        /// <param name="accountNamespaceName"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator DeleteAccountTakeOverSetting(
            Client client,
            GameSession session,
            UnityAction<AsyncResult<EzDeleteTakeOverSettingResult>> callback,
            string accountNamespaceName,
            ErrorEvent onError
        )
        {
            AsyncResult<EzDeleteTakeOverSettingResult> result = null;
            yield return client.Account.DeleteTakeOverSetting(
                r => { result = r; },
                session,
                accountNamespaceName,
                type
            );
            
            if (result.Error != null)
            {
                onError.Invoke(
                    result.Error
                );
                callback.Invoke(result);
                yield break;
            }

            callback.Invoke(result);
        }
    }
}