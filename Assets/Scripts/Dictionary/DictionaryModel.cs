using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gs2.Core.Exception;
using Gs2.Unity.Core;
using Gs2.Unity.Gs2Dictionary.Model;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Events;
using EzConfig = Gs2.Unity.Gs2Exchange.Model.EzConfig;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
#endif

namespace Gs2.Sample.Dictionary
{
    public class DictionaryModel : MonoBehaviour
    {
        /// <summary>
        /// 図鑑に登録されうるエントリーの一覧（マスターデータ）
        /// List of entries that can be registered in the dictionary (master data)
        /// </summary>
        public List<EzEntryModel> EntryModels = new List<EzEntryModel>();

        /// <summary>
        /// 取得済みの図鑑エントリー
        /// Dictionary entries already registered
        /// </summary>
        public List<EzEntry> Entries = new List<EzEntry>();

        /// <summary>
        /// 未登録のエントリーモデル名を1件返す（すべて登録済みの場合は null）
        /// Returns the name of one unregistered entry model (null if everything is registered)
        /// </summary>
        public string FindUnregisteredEntryModelName()
        {
            var entryModel = EntryModels.FirstOrDefault(
                model => Entries.All(entry => entry.Name != model.Name)
            );
            return entryModel?.Name;
        }

        /// <summary>
        /// 図鑑のエントリーモデル（定義）を取得
        /// Get the entry models (definitions) of the dictionary
        /// </summary>
        public IEnumerator GetEntryModels(
            Gs2Domain gs2,
            string dictionaryNamespaceName,
            GetEntryModelsEvent onGetEntryModels,
            ErrorEvent onError
        )
        {
            EntryModels.Clear();

            var it = gs2.Dictionary.Namespace(
                namespaceName: dictionaryNamespaceName
            ).EntryModels();
            while (it.HasNext())
            {
                yield return it.Next();
                if (it.Error != null)
                {
                    onError.Invoke(it.Error, null);
                    yield break;
                }

                if (it.Current != null)
                {
                    EntryModels.Add(it.Current);
                }
            }

            onGetEntryModels.Invoke(EntryModels);
        }
#if GS2_ENABLE_UNITASK
        public async UniTask GetEntryModelsAsync(
            Gs2Domain gs2,
            string dictionaryNamespaceName,
            GetEntryModelsEvent onGetEntryModels,
            ErrorEvent onError
        )
        {
            EntryModels.Clear();

            try
            {
                EntryModels = await gs2.Dictionary.Namespace(
                    namespaceName: dictionaryNamespaceName
                ).EntryModelsAsync().ToListAsync();
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e, null);
                return;
            }

            onGetEntryModels.Invoke(EntryModels);
        }
#endif

        /// <summary>
        /// 取得済みの図鑑エントリーを取得
        /// Get the dictionary entries already registered
        /// </summary>
        public IEnumerator GetEntries(
            Gs2Domain gs2,
            GameSession gameSession,
            string dictionaryNamespaceName,
            GetEntriesEvent onGetEntries,
            ErrorEvent onError
        )
        {
            Entries.Clear();

            var it = gs2.Dictionary.Namespace(
                namespaceName: dictionaryNamespaceName
            ).Me(
                gameSession: gameSession
            ).Entries();
            while (it.HasNext())
            {
                yield return it.Next();
                if (it.Error != null)
                {
                    onError.Invoke(it.Error, null);
                    yield break;
                }

                if (it.Current != null)
                {
                    Entries.Add(it.Current);
                }
            }

            onGetEntries.Invoke(Entries);
        }
#if GS2_ENABLE_UNITASK
        public async UniTask GetEntriesAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string dictionaryNamespaceName,
            GetEntriesEvent onGetEntries,
            ErrorEvent onError
        )
        {
            Entries.Clear();

            try
            {
                Entries = await gs2.Dictionary.Namespace(
                    namespaceName: dictionaryNamespaceName
                ).Me(
                    gameSession: gameSession
                ).EntriesAsync().ToListAsync();
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e, null);
                return;
            }

            onGetEntries.Invoke(Entries);
        }
#endif

        /// <summary>
        /// 図鑑エントリーを1件登録する
        /// Register one dictionary entry
        ///
        /// ※図鑑エントリーの登録はサーバー専用 API のため、
        /// 　このサンプルでは GS2-Exchange の交換レートとして実行しています。
        /// *Registering a dictionary entry is a server-side only API,
        /// 　so this sample executes it as a GS2-Exchange rate.
        /// </summary>
        public IEnumerator RegisterEntry(
            UnityAction<Gs2Exception> callback,
            Gs2Domain gs2,
            GameSession gameSession,
            string exchangeNamespaceName,
            string exchangeRateName,
            string entryModelName,
            RegisterEntryEvent onRegisterEntry,
            ErrorEvent onError
        )
        {
            var domain = gs2.Exchange.Namespace(
                namespaceName: exchangeNamespaceName
            ).Me(
                gameSession: gameSession
            ).Exchange();
            var future = domain.ExchangeFuture(
                rateName: exchangeRateName,
                count: 1,
                config: new[]
                {
                    new EzConfig
                    {
                        Key = "entryModelName",
                        Value = entryModelName,
                    },
                }
            );
            yield return future;
            if (future.Error != null)
            {
                onError.Invoke(future.Error, null);
                callback.Invoke(future.Error);
                yield break;
            }

            // トランザクションの自動実行の完了を待機（連鎖するトランザクションも含めて全て待つ）
            // Wait for automatic transaction execution to complete (including all chained transactions)
            var waitFuture = future.Result.WaitFuture(true);
            yield return waitFuture;
            if (waitFuture.Error != null)
            {
                onError.Invoke(waitFuture.Error, null);
                callback.Invoke(waitFuture.Error);
                yield break;
            }

            onRegisterEntry.Invoke(entryModelName);

            callback.Invoke(null);
        }
#if GS2_ENABLE_UNITASK
        public async UniTask<Gs2Exception> RegisterEntryAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string exchangeNamespaceName,
            string exchangeRateName,
            string entryModelName,
            RegisterEntryEvent onRegisterEntry,
            ErrorEvent onError
        )
        {
            var domain = gs2.Exchange.Namespace(
                namespaceName: exchangeNamespaceName
            ).Me(
                gameSession: gameSession
            ).Exchange();
            try
            {
                var result = await domain.ExchangeAsync(
                    rateName: exchangeRateName,
                    count: 1,
                    config: new[]
                    {
                        new EzConfig
                        {
                            Key = "entryModelName",
                            Value = entryModelName,
                        },
                    }
                );
                // トランザクションの自動実行の完了を待機（連鎖するトランザクションも含めて全て待つ）
                // Wait for automatic transaction execution to complete (including all chained transactions)
                await result.WaitAsync(true);
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e, null);
                return e;
            }
            catch (System.Exception e)
            {
                // トランザクション結果の取得失敗（TimeoutException など）も失敗として扱う
                // Treat a failure to obtain the transaction result (TimeoutException, etc.) as a failure
                var error = new UnknownException(e.Message);
                onError.Invoke(error, null);
                return error;
            }

            onRegisterEntry.Invoke(entryModelName);

            return null;
        }
#endif

        /// <summary>
        /// 取得済みの図鑑エントリーをすべて削除する
        /// Delete all the registered dictionary entries
        ///
        /// ※交換レートは1件ずつ削除するため、取得済みのエントリー分だけ実行します。
        /// *The exchange rate deletes one entry at a time, so it is executed for each registered entry.
        /// </summary>
        public IEnumerator ResetEntries(
            UnityAction<Gs2Exception> callback,
            Gs2Domain gs2,
            GameSession gameSession,
            string exchangeNamespaceName,
            string exchangeRateName,
            ResetEntriesEvent onResetEntries,
            ErrorEvent onError
        )
        {
            foreach (var entryName in Entries.Select(entry => entry.Name).ToList())
            {
                var domain = gs2.Exchange.Namespace(
                    namespaceName: exchangeNamespaceName
                ).Me(
                    gameSession: gameSession
                ).Exchange();
                var future = domain.ExchangeFuture(
                    rateName: exchangeRateName,
                    count: 1,
                    config: new[]
                    {
                        new EzConfig
                        {
                            Key = "entryModelName",
                            Value = entryName,
                        },
                    }
                );
                yield return future;
                if (future.Error != null)
                {
                    onError.Invoke(future.Error, null);
                    callback.Invoke(future.Error);
                    yield break;
                }

                // トランザクションの自動実行の完了を待機（連鎖するトランザクションも含めて全て待つ）
                // Wait for automatic transaction execution to complete (including all chained transactions)
                var waitFuture = future.Result.WaitFuture(true);
                yield return waitFuture;
                if (waitFuture.Error != null)
                {
                    onError.Invoke(waitFuture.Error, null);
                    callback.Invoke(waitFuture.Error);
                    yield break;
                }
            }

            onResetEntries.Invoke();

            callback.Invoke(null);
        }
#if GS2_ENABLE_UNITASK
        public async UniTask<Gs2Exception> ResetEntriesAsync(
            Gs2Domain gs2,
            GameSession gameSession,
            string exchangeNamespaceName,
            string exchangeRateName,
            ResetEntriesEvent onResetEntries,
            ErrorEvent onError
        )
        {
            foreach (var entryName in Entries.Select(entry => entry.Name).ToList())
            {
                var domain = gs2.Exchange.Namespace(
                    namespaceName: exchangeNamespaceName
                ).Me(
                    gameSession: gameSession
                ).Exchange();
                try
                {
                    var result = await domain.ExchangeAsync(
                        rateName: exchangeRateName,
                        count: 1,
                        config: new[]
                        {
                            new EzConfig
                            {
                                Key = "entryModelName",
                                Value = entryName,
                            },
                        }
                    );
                    // トランザクションの自動実行の完了を待機（連鎖するトランザクションも含めて全て待つ）
                    // Wait for automatic transaction execution to complete (including all chained transactions)
                    await result.WaitAsync(true);
                }
                catch (Gs2Exception e)
                {
                    onError.Invoke(e, null);
                    return e;
                }
                catch (System.Exception e)
                {
                    // トランザクション結果の取得失敗（TimeoutException など）も失敗として扱う
                    // Treat a failure to obtain the transaction result (TimeoutException, etc.) as a failure
                    var error = new UnknownException(e.Message);
                    onError.Invoke(error, null);
                    return error;
                }
            }

            onResetEntries.Invoke();

            return null;
        }
#endif
    }
}
