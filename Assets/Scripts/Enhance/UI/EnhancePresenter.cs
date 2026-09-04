using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gs2.Unity.Gs2Inventory.Model;
using UnityEngine;
using UnityEngine.Assertions;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
#endif

namespace Gs2.Sample.Enhance
{
    public class EnhancePresenter : MonoBehaviour
    {
        [SerializeField]
        private EnhanceSetting _enhanceSetting;

        [SerializeField]
        private EnhanceModel _enhanceModel;

        [SerializeField]
        private EnhanceView _enhanceView;

        [SerializeField]
        private EnhanceDialog _enhanceDialog;

        public enum State
        {
            MainMenu,

            /// <summary>
            /// 強化に必要な情報を取得中
            /// The information required for the enhancement is being retrieved
            /// </summary>
            GetEnhanceProcessing,

            /// <summary>
            /// 強化メニュー
            /// Enhancement menu
            /// </summary>
            EnhanceMenu,

            /// <summary>
            /// 強化を実行中
            /// The enhancement is being executed
            /// </summary>
            EnhanceProcessing,
        }

        /// <summary>
        /// 現在のステート
        /// Current State
        /// </summary>
        private State _enhanceState = State.MainMenu;

        public void Start()
        {
            Assert.IsNotNull(_enhanceSetting);
            Assert.IsNotNull(_enhanceModel);
            Assert.IsNotNull(_enhanceView);
            Assert.IsNotNull(_enhanceDialog);

            _enhanceDialog.onExecuteEnhance.AddListener(OnExecuteEnhance);

            _enhanceDialog.Close();
            _enhanceView.OnCloseEvent();
        }

        private void SetState(State state)
        {
            if (_enhanceState != state)
            {
                switch (state)
                {
                    default:
                        UIManager.Instance.CloseProcessing();
                        _enhanceView.OnCloseEvent();
                        break;

                    case State.GetEnhanceProcessing:
                    case State.EnhanceProcessing:
                        UIManager.Instance.OpenProcessing();
                        break;

                    case State.EnhanceMenu:
                        UIManager.Instance.CloseProcessing();
                        _enhanceView.OnOpenEvent();
                        break;
                }
            }

            _enhanceState = state;
        }

        /// <summary>
        /// 強化の初期化（強化素材のアイテムモデルを取得）
        /// Enhancement initialization (obtains the item models of the enhancement materials)
        /// </summary>
        public IEnumerator Initialize()
        {
            UIManager.Instance.AddLog("EnhancePresenter::Initialize");

            yield return _enhanceModel.GetMaterialItemModels(
                GameManager.Instance.Domain,
                _enhanceSetting.inventoryNamespaceName,
                _enhanceSetting.materialInventoryModelName,
                _enhanceSetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        public async UniTask InitializeAsync()
        {
            UIManager.Instance.AddLog("EnhancePresenter::InitializeAsync");

            await _enhanceModel.GetMaterialItemModelsAsync(
                GameManager.Instance.Domain,
                _enhanceSetting.inventoryNamespaceName,
                _enhanceSetting.materialInventoryModelName,
                _enhanceSetting.onError
            );
        }
#endif

        /// <summary>
        /// 強化メニューを開く
        /// Open the enhancement menu
        /// </summary>
        public void ClickToOpenEnhance()
        {
            SetState(State.GetEnhanceProcessing);

#if GS2_ENABLE_UNITASK
            OpenEnhanceAsync().Forget();
#else
            StartCoroutine(
                OpenEnhance()
            );
#endif
        }

        private IEnumerator OpenEnhance()
        {
            yield return Refresh();

            SetState(State.EnhanceMenu);
        }
#if GS2_ENABLE_UNITASK
        private async UniTask OpenEnhanceAsync()
        {
            await RefreshAsync();

            SetState(State.EnhanceMenu);
        }
#endif

        /// <summary>
        /// 強化対象・強化素材・ステータスを取得し直す
        /// Retrieve the targets, materials and statuses again
        /// </summary>
        public IEnumerator Refresh()
        {
            yield return _enhanceModel.GetCharacters(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _enhanceSetting.inventoryNamespaceName,
                _enhanceSetting.characterInventoryModelName,
                _enhanceSetting.onGetItemSets,
                _enhanceSetting.onError
            );

            yield return _enhanceModel.GetMaterials(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _enhanceSetting.inventoryNamespaceName,
                _enhanceSetting.materialInventoryModelName,
                _enhanceSetting.onGetItemSets,
                _enhanceSetting.onError
            );

            yield return _enhanceModel.GetStatuses(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _enhanceSetting.experienceNamespaceName,
                _enhanceSetting.experienceModelName,
                _enhanceSetting.onGetStatuses,
                _enhanceSetting.onError
            );

            // 未強化の強化対象にはステータスが存在しないため、
            // 次のランクまでの必要経験値を経験値モデルから取得する
            // A target that has never been enhanced has no status, so read the experience
            // required for the next rank from the experience model
            yield return _enhanceModel.GetExperienceModel(
                GameManager.Instance.Domain,
                _enhanceSetting.experienceNamespaceName,
                _enhanceSetting.experienceModelName,
                _enhanceSetting.onError
            );

            OnChangeCharacters();
        }
#if GS2_ENABLE_UNITASK
        public async UniTask RefreshAsync()
        {
            await _enhanceModel.GetCharactersAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _enhanceSetting.inventoryNamespaceName,
                _enhanceSetting.characterInventoryModelName,
                _enhanceSetting.onGetItemSets,
                _enhanceSetting.onError
            );

            await _enhanceModel.GetMaterialsAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _enhanceSetting.inventoryNamespaceName,
                _enhanceSetting.materialInventoryModelName,
                _enhanceSetting.onGetItemSets,
                _enhanceSetting.onError
            );

            await _enhanceModel.GetStatusesAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _enhanceSetting.experienceNamespaceName,
                _enhanceSetting.experienceModelName,
                _enhanceSetting.onGetStatuses,
                _enhanceSetting.onError
            );

            // 未強化の強化対象にはステータスが存在しないため、
            // 次のランクまでの必要経験値を経験値モデルから取得する
            // A target that has never been enhanced has no status, so read the experience
            // required for the next rank from the experience model
            await _enhanceModel.GetExperienceModelAsync(
                GameManager.Instance.Domain,
                _enhanceSetting.experienceNamespaceName,
                _enhanceSetting.experienceModelName,
                _enhanceSetting.onError
            );

            OnChangeCharacters();
        }
#endif

        /// <summary>
        /// 強化対象の一覧表示を更新
        /// Update the list of enhancement targets
        /// </summary>
        private void OnChangeCharacters()
        {
            for (var i = 0; i < _enhanceView.contentTransform.childCount; i++)
            {
                var child = _enhanceView.contentTransform.GetChild(i).gameObject;
                if (child == _enhanceView.characterItemPrefab.gameObject)
                    continue;
                Destroy(child);
            }

            foreach (var character in _enhanceModel.Characters)
            {
                var item = Instantiate(_enhanceView.characterItemPrefab, _enhanceView.contentTransform);
                item.Initialize(
                    character,
                    _enhanceModel.FindStatus(character),
                    _enhanceModel.FirstRankUpExperience
                );

                item.onClickCharacter.AddListener(OnClickCharacter);

                item.gameObject.SetActive(true);
            }

            _enhanceView.SetMaterialCount(GetOwnedMaterialCount());
            _enhanceView.SetInteractable(true);
        }

        private long GetOwnedMaterialCount()
        {
            return _enhanceModel.Materials.Sum(material => material.Count);
        }

        /// <summary>
        /// 強化対象を選択したらランクを上げるダイアログを開く
        /// Open the dialog used to raise the rank when an enhancement target is selected
        /// </summary>
        private void OnClickCharacter(EzItemSet character)
        {
            var material = _enhanceModel.Materials.FirstOrDefault(itemSet => itemSet.Count > 0);
            if (material == null)
            {
                UIManager.Instance.OpenDialog1("Notice", "EnhanceMaterialEmpty");
                return;
            }

            _enhanceDialog.Open(
                character,
                _enhanceModel.FindStatus(character),
                (int)material.Count,
                _enhanceModel.GetMaterialExperience(material),
                _enhanceModel.FirstRankUpExperience
            );
        }

        /// <summary>
        /// ダイアログで強化を実行
        /// Execute the enhancement from the dialog
        /// </summary>
        private void OnExecuteEnhance(EzItemSet target, int count)
        {
            _enhanceDialog.Close();

            var material = _enhanceModel.Materials.FirstOrDefault(itemSet => itemSet.Count > 0);
            if (material == null)
            {
                UIManager.Instance.OpenDialog1("Notice", "EnhanceMaterialEmpty");
                return;
            }

            _enhanceView.SetInteractable(false);
            SetState(State.EnhanceProcessing);

#if GS2_ENABLE_UNITASK
            EnhanceTaskAsync(target, material, count).Forget();
#else
            StartCoroutine(
                EnhanceTask(target, material, count)
            );
#endif
        }

        private IEnumerator EnhanceTask(EzItemSet target, EzItemSet material, int count)
        {
            var failed = false;
            yield return _enhanceModel.Enhance(
                err => { failed = err != null; },
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _enhanceSetting.enhanceNamespaceName,
                _enhanceSetting.enhanceRateName,
                target,
                material,
                count,
                _enhanceSetting.onEnhance,
                _enhanceSetting.onError
            );

            yield return Refresh();

            SetState(State.EnhanceMenu);

            if (!failed)
            {
                UIManager.Instance.OpenDialog1("Notice", "EnhanceComplete");
            }
        }
#if GS2_ENABLE_UNITASK
        private async UniTask EnhanceTaskAsync(EzItemSet target, EzItemSet material, int count)
        {
            var error = await _enhanceModel.EnhanceAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _enhanceSetting.enhanceNamespaceName,
                _enhanceSetting.enhanceRateName,
                target,
                material,
                count,
                _enhanceSetting.onEnhance,
                _enhanceSetting.onError
            );

            await RefreshAsync();

            SetState(State.EnhanceMenu);

            if (error == null)
            {
                UIManager.Instance.OpenDialog1("Notice", "EnhanceComplete");
            }
        }
#endif

        /// <summary>
        /// 動作確認用に強化対象を入手する
        /// Obtain an enhancement target for verification purposes
        /// </summary>
        public void ClickToGetCharacter()
        {
            AcquireItem(_enhanceSetting.exchangeRateNameGetCharacter);
        }

        /// <summary>
        /// 動作確認用に強化素材を入手する
        /// Obtain enhancement materials for verification purposes
        /// </summary>
        public void ClickToGetMaterial()
        {
            AcquireItem(_enhanceSetting.exchangeRateNameGetMaterial);
        }

        private void AcquireItem(string exchangeRateName)
        {
            _enhanceView.SetInteractable(false);
            SetState(State.EnhanceProcessing);

#if GS2_ENABLE_UNITASK
            AcquireItemTaskAsync(exchangeRateName).Forget();
#else
            StartCoroutine(
                AcquireItemTask(exchangeRateName)
            );
#endif
        }

        private IEnumerator AcquireItemTask(string exchangeRateName)
        {
            yield return _enhanceModel.AcquireItem(
                err => { },
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _enhanceSetting.exchangeNamespaceName,
                exchangeRateName,
                _enhanceSetting.onAcquireItem,
                _enhanceSetting.onError
            );

            yield return Refresh();

            SetState(State.EnhanceMenu);
        }
#if GS2_ENABLE_UNITASK
        private async UniTask AcquireItemTaskAsync(string exchangeRateName)
        {
            await _enhanceModel.AcquireItemAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _enhanceSetting.exchangeNamespaceName,
                exchangeRateName,
                _enhanceSetting.onAcquireItem,
                _enhanceSetting.onError
            );

            await RefreshAsync();

            SetState(State.EnhanceMenu);
        }
#endif

        /// <summary>
        /// 強化メニューを閉じる
        /// Close the enhancement menu
        /// </summary>
        public void ClickToClose()
        {
            _enhanceDialog.Close();

            SetState(State.MainMenu);
        }
    }
}
