using System.Collections;
using Gs2.Core.Exception;
using Gs2.Unity.Core;
using Gs2.Unity.Gs2Stamina.Model;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Events;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
#endif

namespace Gs2.Sample.Stamina
{
    public class StaminaModel : MonoBehaviour
    {
        /// <summary>
        /// スタミナモデル
        /// stamina model
        /// </summary>
        public EzStaminaModel model;
        
        /// <summary>
        /// 現在のスタミナ値
        /// Current Stamina Value
        /// </summary>
        public EzStamina stamina;

        /// <summary>
        /// スタミナモデルを取得
        /// Get Stamina Model
        /// </summary>
        public IEnumerator GetStaminaModel(
            Gs2Domain gs2,
            string staminaNamespaceName,
            string staminaModelName,
            GetStaminaModelEvent onGetStaminaModel,
            ErrorEvent onError
        )
        {
            var domain = gs2.Stamina.Namespace(
                namespaceName: staminaNamespaceName
            ).StaminaModel(
                staminaName: staminaModelName
            );
            var future = domain.Model();
            yield return future;
            if (future.Error != null)
            {
                onError.Invoke(future.Error, null);
                yield break;
            }

            model = future.Result;

            onGetStaminaModel.Invoke(staminaModelName, model);
        }
#if GS2_ENABLE_UNITASK
        /// <summary>
        /// スタミナモデルを取得
        /// Get Stamina Model
        /// </summary>
        public async UniTask GetStaminaModelAsync(
            Gs2Domain gs2,
            string staminaNamespaceName,
            string staminaModelName,
            GetStaminaModelEvent onGetStaminaModel,
            ErrorEvent onError
        )
        {
            var domain = gs2.Stamina.Namespace(
                namespaceName: staminaNamespaceName
            ).StaminaModel(
                staminaName: staminaModelName
            );
            try
            {
                model = await domain.ModelAsync();
                onGetStaminaModel.Invoke(staminaModelName, model);
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e, null);
            }
        }
#endif

        /// <summary>
        /// スタミナ値を取得
        /// Get Stamina Value
        /// </summary>
        public IEnumerator GetStamina(
            UnityAction<EzStamina> callback,
            Gs2Domain gs2,
            GameSession gameSession,
            string staminaNamespaceName,
            string staminaName,
            GetStaminaEvent onGetStamina,
            ErrorEvent onError
        )
        {
            var future = gs2.Stamina.Namespace(
                namespaceName: staminaNamespaceName
            ).Me(
                gameSession: gameSession
            ).Stamina(
                staminaName: staminaName
            ).Model();
            yield return future;
            if (future.Error != null)
            {
                onError.Invoke(future.Error, null);
                yield break;
            }
            stamina = future.Result;
            
            onGetStamina.Invoke(stamina);
            
            callback.Invoke(stamina);
        }
#if GS2_ENABLE_UNITASK
        /// <summary>
        /// スタミナ値を取得
        /// Get Stamina Value
        /// </summary>
        public async UniTask<EzStamina> GetStaminaAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string staminaNamespaceName,
            string staminaName,
            GetStaminaEvent onGetStamina,
            ErrorEvent onError
        )
        {
            var domain = gs2.Stamina.Namespace(
                namespaceName: staminaNamespaceName
            ).Me(
                gameSession: gameSession
            ).Stamina(
                staminaName: staminaName
            );
            try
            {
                stamina = await domain.ModelAsync();
                
                onGetStamina.Invoke(stamina);
                
                return stamina;
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e, null);
            }

            return null;
        }
#endif

        /// <summary>
        /// スタミナを消費する
        /// Consume Stamina
        /// </summary>
        public IEnumerator ConsumeStamina(
            Gs2Domain gs2,
            GameSession gameSession,
            string staminaNamespaceName,
            string staminaName,
            int consumeValue,
            ConsumeStaminaEvent onConsumeStamina,
            GetStaminaEvent onGetStamina,
            ErrorEvent onError
        )
        {
            var domain = gs2.Stamina.Namespace(
                namespaceName: staminaNamespaceName
            ).Me(
                gameSession: gameSession
            ).Stamina(
                staminaName: staminaName
            );
            var future = domain.Consume(
                consumeValue: consumeValue
            );
            yield return future;
            if (future.Error != null)
            {
                onError.Invoke(future.Error, null);
                yield break;
            }

            var future2 = future.Result.Model();
            yield return future2;
            if (future2.Error != null)
            {
                onError.Invoke(future2.Error, null);
                yield break;
            }

            stamina = future2.Result;

            onConsumeStamina.Invoke(model, stamina, consumeValue);
            onGetStamina.Invoke(stamina);
        }
#if GS2_ENABLE_UNITASK
        public async UniTask ConsumeStaminaAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string staminaNamespaceName,
            string staminaName,
            int consumeValue,
            ConsumeStaminaEvent onConsumeStamina,
            GetStaminaEvent onGetStamina,
            ErrorEvent onError
        )
        {
            var domain = gs2.Stamina.Namespace(
                namespaceName: staminaNamespaceName
            ).Me(
                gameSession: gameSession
            ).Stamina(
                staminaName: staminaName
            );
            try
            {
                var result = await domain.ConsumeAsync(
                    consumeValue
                );

                stamina = await result.ModelAsync();
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e, null);
                return;
            }
            
            onConsumeStamina.Invoke(model, stamina, consumeValue);
            onGetStamina.Invoke(stamina);
        }
#endif

        /// <summary>
        /// スタミナを購入する
        /// Buy Stamina
        /// </summary>
        public IEnumerator Buy(
            UnityAction<Gs2Exception> callback,
            Gs2Domain gs2,
            GameSession gameSession,
            string exchangeNamespaceName,
            string exchangeRateName,
            int slot,
            StaminaBuyEvent onBuy,
            ErrorEvent onError
        )
        {
            var domain = gs2.Exchange.Namespace(
                namespaceName: exchangeNamespaceName
            ).Me(
                gameSession: gameSession
            ).Exchange();
            var future = domain.Exchange(
                rateName: exchangeRateName,
                count: 1,
                config: new[]
                {
                    new Gs2.Unity.Gs2Exchange.Model.EzConfig
                    {
                        Key = "slot",
                        Value = slot.ToString(),
                    }
                }
            );
            yield return future;
            if (future.Error != null)
            {
                onError.Invoke(
                    future.Error,
                    null
                );
                callback.Invoke(future.Error);
                yield break;
            }

            // スタミナ購入に成功
            // Successfully purchased stamina

            onBuy.Invoke();

            callback.Invoke(null);
        }

#if GS2_ENABLE_UNITASK
        /// <summary>
        /// スタミナを購入する
        /// Buy Stamina
        /// </summary>
        public async UniTask<Gs2Exception> BuyAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string exchangeNamespaceName,
            string exchangeRateName,
            int slot,
            StaminaBuyEvent onBuy,
            ErrorEvent onError
        )
        {
            var domain = gs2.Exchange.Namespace(
                namespaceName: exchangeNamespaceName
            ).Me(
                gameSession: gameSession
            ).Exchange();
            Gs2.Unity.Gs2Exchange.Model.EzConfig[] config =
            {
                new Gs2.Unity.Gs2Exchange.Model.EzConfig
                {
                    Key = "slot",
                    Value = slot.ToString(),
                }
            };
            try
            {
                await domain.ExchangeAsync(
                    exchangeRateName,
                    1,
                    config
                );
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e, null);
                return e;
            }

            // スタミナ購入に成功
            // Successfully purchased stamina

            onBuy.Invoke();
            return null;
        }
#endif
    }
}