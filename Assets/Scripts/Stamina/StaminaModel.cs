using System.Collections;
using System.Collections.Generic;
using Gs2.Core;
using Gs2.Core.Exception;
using Gs2.Core.Util;
using Gs2.Unity;
using Gs2.Unity.Gs2Exchange.Result;
using Gs2.Unity.Gs2Stamina.Model;
using Gs2.Unity.Gs2Stamina.Result;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Stamina
{
    public class StaminaModel : MonoBehaviour
    {
        /// <summary>
        /// スタミナモデル
        /// stamina model
        /// </summary>
        public EzStaminaModel staminaModel;
        
        /// <summary>
        /// 現在のスタミナ値
        /// Current Stamina Value
        /// </summary>
        public EzStamina stamina;
        
        /// <summary>
        /// スタミナ値を取得
        /// Get Stamina Value
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="client"></param>
        /// <param name="session"></param>
        /// <param name="staminaNamespaceName"></param>
        /// <param name="staminaName"></param>
        /// <param name="onGetStamina"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator GetStamina(
            UnityAction<AsyncResult<EzGetStaminaResult>> callback,
            Client client,
            GameSession session,
            string staminaNamespaceName,
            string staminaName,
            GetStaminaEvent onGetStamina,
            ErrorEvent onError
        )
        {
            AsyncResult<EzGetStaminaResult> result = null;
            yield return client.Stamina.GetStamina(
                r =>
                {
                    result = r;
                },
                session,
                staminaNamespaceName,
                staminaName
            );
            
            if (result.Error != null)
            {
                onError.Invoke(
                    result.Error
                );
                callback.Invoke(result);
                yield break;
            }
            
            stamina = result.Result.Item;
            
            onGetStamina.Invoke(staminaModel, result.Result.Item);
            
            callback.Invoke(result);
        }

        /// <summary>
        /// スタミナを消費する
        /// Consume Stamina
        /// </summary>
        /// <param name="client"></param>
        /// <param name="session"></param>
        /// <param name="staminaNamespaceName"></param>
        /// <param name="consumeValue"></param>
        /// <param name="onConsumeStamina"></param>
        /// <param name="onGetStamina"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator ConsumeStamina(
            Client client,
            GameSession session,
            string staminaNamespaceName,
            int consumeValue,
            ConsumeStaminaEvent onConsumeStamina,
            GetStaminaEvent onGetStamina,
            ErrorEvent onError
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

            stamina = result.Result.Item;

            onConsumeStamina.Invoke(staminaModel, stamina, consumeValue);
            onGetStamina.Invoke(staminaModel, stamina);
        }
        
        /// <summary>
        /// スタミナを購入する
        /// Buy Stamina
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="client"></param>
        /// <param name="session"></param>
        /// <param name="exchangeNamespaceName"></param>
        /// <param name="exchangeRateName"></param>
        /// <param name="slot"></param>
        /// <param name="distributorNamespaceName"></param>
        /// <param name="exchangeKeyId"></param>
        /// <param name="onBuy"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator Buy(
            UnityAction<AsyncResult<object>> callback,
            Client client,
            GameSession session,
            string exchangeNamespaceName,
            string exchangeRateName,
            int slot,
            string distributorNamespaceName,
            string exchangeKeyId,
            StaminaBuyEvent onBuy,
            ErrorEvent onError
        )
        {
            string stampSheet = null;
            {
                AsyncResult<EzExchangeResult> result = null;
                yield return client.Exchange.Exchange(
                    r => { result = r; },
                    session,
                    exchangeNamespaceName,
                    exchangeRateName,
                    1,
                    new List<Gs2.Unity.Gs2Exchange.Model.EzConfig>
                    {
                        new Gs2.Unity.Gs2Exchange.Model.EzConfig
                        {
                            Key = "slot",
                            Value = slot.ToString(),
                        }
                    }
                );

                if (result.Error != null)
                {
                    onError.Invoke(
                        result.Error
                    );
                    callback.Invoke(new AsyncResult<object>(null, result.Error));
                    yield break;
                }

                // スタンプシートを取得
                // Get Stamp Sheet
                stampSheet = result.Result.StampSheet;
            }
            {
                var machine = new StampSheetStateMachine(
                    stampSheet,
                    client,
                    distributorNamespaceName,
                    exchangeKeyId
                );

                Gs2Exception exception = null;

                void OnError(Gs2Exception e)
                {
                    exception = e;
                }

                onError.AddListener(OnError);
                
                // スタンプシートの実行
                // Stamp sheet execution
                yield return machine.Execute(onError);
                
                onError.RemoveListener(OnError);

                if (exception != null)
                {
                    // スタンプシート実行エラー
                    // Stamp sheet execution error
                    onError.Invoke(
                        exception
                    );
                    callback.Invoke(new AsyncResult<object>(null, exception));
                    yield break;
                }
            }
            // スタミナ購入に成功
            // Successfully purchased stamina
            
            onBuy.Invoke();

            callback.Invoke(new AsyncResult<object>(null, null));
        }
    }
}