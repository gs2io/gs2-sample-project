using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gs2.Unity.Gs2Dictionary.Model;
using UnityEngine;
using UnityEngine.Assertions;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
#endif

namespace Gs2.Sample.Dictionary
{
    public class DictionaryPresenter : MonoBehaviour
    {
        [SerializeField]
        private DictionarySetting _dictionarySetting;

        [SerializeField]
        private DictionaryModel _dictionaryModel;

        [SerializeField]
        private DictionaryView _dictionaryView;

        public enum State
        {
            MainMenu,

            /// <summary>
            /// 図鑑情報を取得中
            /// Dictionary information is being retrieved
            /// </summary>
            GetEntriesProcessing,

            /// <summary>
            /// 図鑑メニュー
            /// Dictionary menu
            /// </summary>
            DictionaryMenu,

            /// <summary>
            /// 図鑑エントリーを更新中
            /// Dictionary entries are being updated
            /// </summary>
            UpdateEntriesProcessing,
        }

        /// <summary>
        /// 現在のステート
        /// Current State
        /// </summary>
        private State _dictionaryState = State.MainMenu;

        public void Start()
        {
            Assert.IsNotNull(_dictionarySetting);
            Assert.IsNotNull(_dictionaryModel);
            Assert.IsNotNull(_dictionaryView);

            _dictionaryView.OnCloseEvent();
        }

        private void SetState(State state)
        {
            if (_dictionaryState != state)
            {
                switch (state)
                {
                    default:
                        UIManager.Instance.CloseProcessing();
                        _dictionaryView.OnCloseEvent();
                        break;

                    case State.GetEntriesProcessing:
                        UIManager.Instance.OpenProcessing();
                        break;

                    case State.UpdateEntriesProcessing:
                        UIManager.Instance.OpenProcessing();
                        break;

                    case State.DictionaryMenu:
                        UIManager.Instance.CloseProcessing();
                        _dictionaryView.OnOpenEvent();
                        break;
                }
            }

            _dictionaryState = state;
        }

        /// <summary>
        /// 図鑑の初期化（エントリーモデルの取得）
        /// Dictionary initialization (obtains the entry models)
        /// </summary>
        public IEnumerator Initialize()
        {
            UIManager.Instance.AddLog("DictionaryPresenter::Initialize");

            yield return _dictionaryModel.GetEntryModels(
                GameManager.Instance.Domain,
                _dictionarySetting.dictionaryNamespaceName,
                _dictionarySetting.onGetEntryModels,
                _dictionarySetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        public async UniTask InitializeAsync()
        {
            UIManager.Instance.AddLog("DictionaryPresenter::InitializeAsync");

            await _dictionaryModel.GetEntryModelsAsync(
                GameManager.Instance.Domain,
                _dictionarySetting.dictionaryNamespaceName,
                _dictionarySetting.onGetEntryModels,
                _dictionarySetting.onError
            );
        }
#endif

        /// <summary>
        /// 図鑑を開く
        /// Open the dictionary
        /// </summary>
        public void ClickToOpenDictionary()
        {
            SetState(State.GetEntriesProcessing);

#if GS2_ENABLE_UNITASK
            OpenDictionaryAsync().Forget();
#else
            StartCoroutine(
                OpenDictionary()
            );
#endif
        }

        private IEnumerator OpenDictionary()
        {
            yield return Refresh();

            SetState(State.DictionaryMenu);
        }
#if GS2_ENABLE_UNITASK
        private async UniTask OpenDictionaryAsync()
        {
            await RefreshAsync();

            SetState(State.DictionaryMenu);
        }
#endif

        public IEnumerator Refresh()
        {
            void RefreshEntriesAction(
                List<EzEntry> entries
            )
            {
                _dictionarySetting.onGetEntries.RemoveListener(RefreshEntriesAction);

                OnChangeEntries();
            }

            _dictionarySetting.onGetEntries.AddListener(RefreshEntriesAction);

            yield return _dictionaryModel.GetEntries(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _dictionarySetting.dictionaryNamespaceName,
                _dictionarySetting.onGetEntries,
                _dictionarySetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        public async UniTask RefreshAsync()
        {
            void RefreshEntriesAction(
                List<EzEntry> entries
            )
            {
                _dictionarySetting.onGetEntries.RemoveListener(RefreshEntriesAction);

                OnChangeEntries();
            }

            _dictionarySetting.onGetEntries.AddListener(RefreshEntriesAction);

            await _dictionaryModel.GetEntriesAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _dictionarySetting.dictionaryNamespaceName,
                _dictionarySetting.onGetEntries,
                _dictionarySetting.onError
            );
        }
#endif

        /// <summary>
        /// 図鑑の一覧表示を更新
        /// Update the list of dictionary entries
        /// </summary>
        private void OnChangeEntries()
        {
            for (var i = 0; i < _dictionaryView.contentTransform.childCount; i++)
            {
                var child = _dictionaryView.contentTransform.GetChild(i).gameObject;
                if (child == _dictionaryView.entryItemPrefab.gameObject)
                    continue;
                Destroy(child);
            }

            foreach (var entryModel in _dictionaryModel.EntryModels)
            {
                var registered = _dictionaryModel.Entries.Any(entry => entry.Name == entryModel.Name);

                var item = Instantiate(_dictionaryView.entryItemPrefab, _dictionaryView.contentTransform);
                item.Initialize(
                    entryModel,
                    registered
                );

                item.gameObject.SetActive(true);
            }

            _dictionaryView.SetProgress(
                _dictionaryModel.Entries.Count,
                _dictionaryModel.EntryModels.Count
            );
            _dictionaryView.SetInteractable(true);
        }

        /// <summary>
        /// 未登録の図鑑エントリーを1件登録する
        /// Register one unregistered dictionary entry
        /// </summary>
        public void ClickToRegisterEntry()
        {
            var entryModelName = _dictionaryModel.FindUnregisteredEntryModelName();
            if (entryModelName == null)
            {
                UIManager.Instance.OpenDialog1("Notice", "EntryAllRegistered");
                return;
            }

            _dictionaryView.SetInteractable(false);
            SetState(State.UpdateEntriesProcessing);

#if GS2_ENABLE_UNITASK
            RegisterEntryTaskAsync(entryModelName).Forget();
#else
            StartCoroutine(
                RegisterEntryTask(entryModelName)
            );
#endif
        }

        private IEnumerator RegisterEntryTask(string entryModelName)
        {
            var failed = false;
            yield return _dictionaryModel.RegisterEntry(
                err => { failed = err != null; },
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _dictionarySetting.exchangeNamespaceName,
                _dictionarySetting.exchangeRateNameRegister,
                entryModelName,
                _dictionarySetting.onRegisterEntry,
                _dictionarySetting.onError
            );

            yield return Refresh();

            SetState(State.DictionaryMenu);

            if (!failed)
            {
                UIManager.Instance.OpenDialog1("Notice", "EntryRegistered");
            }
        }
#if GS2_ENABLE_UNITASK
        private async UniTask RegisterEntryTaskAsync(string entryModelName)
        {
            var error = await _dictionaryModel.RegisterEntryAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _dictionarySetting.exchangeNamespaceName,
                _dictionarySetting.exchangeRateNameRegister,
                entryModelName,
                _dictionarySetting.onRegisterEntry,
                _dictionarySetting.onError
            );

            await RefreshAsync();

            SetState(State.DictionaryMenu);

            if (error == null)
            {
                UIManager.Instance.OpenDialog1("Notice", "EntryRegistered");
            }
        }
#endif

        /// <summary>
        /// 取得済みの図鑑エントリーをすべて削除する
        /// Delete all the registered dictionary entries
        /// </summary>
        public void ClickToResetEntries()
        {
            _dictionaryView.SetInteractable(false);
            SetState(State.UpdateEntriesProcessing);

#if GS2_ENABLE_UNITASK
            ResetEntriesTaskAsync().Forget();
#else
            StartCoroutine(
                ResetEntriesTask()
            );
#endif
        }

        private IEnumerator ResetEntriesTask()
        {
            var failed = false;
            yield return _dictionaryModel.ResetEntries(
                err => { failed = err != null; },
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _dictionarySetting.exchangeNamespaceName,
                _dictionarySetting.exchangeRateNameReset,
                _dictionarySetting.onResetEntries,
                _dictionarySetting.onError
            );

            yield return Refresh();

            SetState(State.DictionaryMenu);

            if (!failed)
            {
                UIManager.Instance.OpenDialog1("Notice", "EntryReset");
            }
        }
#if GS2_ENABLE_UNITASK
        private async UniTask ResetEntriesTaskAsync()
        {
            var error = await _dictionaryModel.ResetEntriesAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _dictionarySetting.exchangeNamespaceName,
                _dictionarySetting.exchangeRateNameReset,
                _dictionarySetting.onResetEntries,
                _dictionarySetting.onError
            );

            await RefreshAsync();

            SetState(State.DictionaryMenu);

            if (error == null)
            {
                UIManager.Instance.OpenDialog1("Notice", "EntryReset");
            }
        }
#endif

        /// <summary>
        /// 図鑑を閉じる
        /// Close the dictionary
        /// </summary>
        public void ClickToClose()
        {
            SetState(State.MainMenu);
        }
    }
}
