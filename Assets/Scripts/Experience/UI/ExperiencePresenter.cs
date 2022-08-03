using System.Collections;
using System.Collections.Generic;
using Gs2.Sample.Unit;
using Gs2.Unity.Gs2Experience.Model;
using Gs2.Unity.Gs2Inventory.Model;
using UnityEngine;
using UnityEngine.Assertions;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
#endif

namespace Gs2.Sample.Experience
{
    public class ExperiencePresenter : MonoBehaviour
    {
        [SerializeField]
        private ExperienceSetting _experienceSetting;
        
        [SerializeField]
        private ExperienceModel _experienceModel;
        [SerializeField]
        private ExperienceView _experienceView;

        [SerializeField]
        private UnitPresenter _unitPresenter;
        
        [SerializeField]
        private ExpStatusDescriptor _expStatusDescriptor;
        
        public int increaseValue = 10;
                
        // Start is called before the first frame update
        void Start()
        {
            Assert.IsNotNull(_experienceSetting);
            Assert.IsNotNull(_experienceModel);
        }

        /// <summary>
        /// 経験値の初期化
        /// </summary>
        public IEnumerator Initialize()
        {
            UIManager.Instance.AddLog("ExperiencePresenter::Initialize");
            
            yield return _experienceModel.GetPlayerExperienceModel(
                GameManager.Instance.Domain,
                _experienceSetting.experienceNamespaceName,
                _experienceSetting.playerExperienceModelName,
                _experienceSetting.onGetExperienceModel,
                _experienceSetting.onError
            );

            yield return _experienceModel.GetItemExperienceModel(
                GameManager.Instance.Domain,
                _experienceSetting.experienceNamespaceName,
                _experienceSetting.itemExperienceModelName,
                _experienceSetting.onGetExperienceModel,
                _experienceSetting.onError
            );
            
            yield return Refresh();
        }
#if GS2_ENABLE_UNITASK
        public async UniTask InitializeAsync()
        {
            UIManager.Instance.AddLog("ExperiencePresenter::InitializeAsync");
            
            await _experienceModel.GetPlayerExperienceModelAsync(
                GameManager.Instance.Domain,
                _experienceSetting.experienceNamespaceName,
                _experienceSetting.playerExperienceModelName,
                _experienceSetting.onGetExperienceModel,
                _experienceSetting.onError
            );

            await _experienceModel.GetItemExperienceModelAsync(
                GameManager.Instance.Domain,
                _experienceSetting.experienceNamespaceName,
                _experienceSetting.itemExperienceModelName,
                _experienceSetting.onGetExperienceModel,
                _experienceSetting.onError
            );
            
            await RefreshAsync();
        }
#endif
        
        /// <summary>
        /// プレイヤーの経験値を増加
        /// </summary>
        public void OnClickIncreasePlayerExperience()
        {
#if GS2_ENABLE_UNITASK
            IncreasePlayerExperienceAsync(increaseValue).Forget();
#else
            StartCoroutine(
                IncreasePlayerExperience(increaseValue)
            );
#endif
        }
        
        /// <summary>
        /// プレイヤーの経験値を増加
        /// </summary>
        private IEnumerator IncreasePlayerExperience(int _increaseValue)
        {
            yield return _experienceModel.IncreaseExperience(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _experienceSetting.exchangeNamespaceName,
                _experienceSetting.playerEexchangeRateName,
                GameManager.Instance.Session.AccessToken.UserId, // プレイヤーのUserId
                _increaseValue,
                _experienceSetting.onError
            );
            
            yield return Refresh();
        }
#if GS2_ENABLE_UNITASK
        private async UniTask IncreasePlayerExperienceAsync(int _increaseValue)
        {
            await _experienceModel.IncreaseExperienceAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _experienceSetting.exchangeNamespaceName,
                _experienceSetting.playerEexchangeRateName,
                GameManager.Instance.Session.AccessToken.UserId, // プレイヤーのUserId
                _increaseValue,
                _experienceSetting.onError
            );
            
            await RefreshAsync();
        }
#endif
        
        /// <summary>
        /// プレイヤー経験値の更新
        /// </summary>
        private IEnumerator Refresh()
        {
            void RefreshStatuesAction(
                EzExperienceModel _experienceModel,
                List<EzStatus> _statuses
            )
            {
                _experienceSetting.onGetStatuses.RemoveListener(RefreshStatuesAction);

                if (_statuses.Count == 0)
                {
                    _experienceView.SetRank(1);
                    var nextRankExperience = _experienceModel.RankThreshold.Values[0];
                    _experienceView.SetExperience(0, nextRankExperience);
                }
                else
                {
                    foreach (var status in _statuses)
                    {
                        if (status.PropertyId == GameManager.Instance.Session.AccessToken.UserId)
                        {
                            _experienceView.SetRank(status);
                            long nextRankExperience;
                            if (status.RankCapValue <= status.RankValue)
                            {
                                nextRankExperience = _experienceModel.RankThreshold.Values[(int)status.RankValue - 2];
                            }
                            else
                            {
                                nextRankExperience = _experienceModel.RankThreshold.Values[(int)status.RankValue - 1];
                            }
                            _experienceView.SetExperience(status, nextRankExperience);
                        }
                    }
                }
            }

            _experienceSetting.onGetStatuses.AddListener(RefreshStatuesAction);
            
            yield return _experienceModel.GetPlayerStatuses(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _experienceSetting.experienceNamespaceName,
                _experienceSetting.onGetStatuses,
                _experienceSetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        private async UniTask RefreshAsync()
        {
            void RefreshStatuesAction(
                EzExperienceModel _experienceModel,
                List<EzStatus> _statuses
            )
            {
                _experienceSetting.onGetStatuses.RemoveListener(RefreshStatuesAction);

                if (_statuses.Count == 0)
                {
                    _experienceView.SetRank(1);
                    var nextRankExperience = _experienceModel.RankThreshold.Values[0];
                    _experienceView.SetExperience(0, nextRankExperience);
                }
                else
                {
                    foreach (var status in _statuses)
                    {
                        if (status.PropertyId == GameManager.Instance.Session.AccessToken.UserId)
                        {
                            _experienceView.SetRank(status);
                            long nextRankExperience;
                            if (status.RankCapValue <= status.RankValue)
                            {
                                nextRankExperience = _experienceModel.RankThreshold.Values[(int)status.RankValue - 2];
                            }
                            else
                            {
                                nextRankExperience = _experienceModel.RankThreshold.Values[(int)status.RankValue - 1];
                            }
                            _experienceView.SetExperience(status, nextRankExperience);
                        }
                    }
                }
            }

            _experienceSetting.onGetStatuses.AddListener(RefreshStatuesAction);
            
            await _experienceModel.GetPlayerStatusesAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _experienceSetting.experienceNamespaceName,
                _experienceSetting.onGetStatuses,
                _experienceSetting.onError
            );
        }
#endif

        /// <summary>
        /// アイテムをクリックした
        /// </summary>
        public void OnClickItem(EzItemSet item)
        {
            _experienceModel.selectedItem = item;

            OpenItemStatus(item);
        }
        
        /// <summary>
        /// アイテムステータスの表示
        /// </summary>
        private void OpenItemStatus(EzItemSet item)
        {
            if (!_experienceModel.itemStatuses.ContainsKey(item.ItemSetId))
            {
                var experienceModel = _experienceModel.itemExperienceModel;
                var nextRankExperience = experienceModel.RankThreshold.Values[0];
                
                _expStatusDescriptor.Initialize(
                    1,
                    0,
                    nextRankExperience,
                    item
                );
            }
            else
            {
                var status = _experienceModel.itemStatuses[item.ItemSetId];
                var experienceModel = _experienceModel.itemExperienceModel;
                long nextRankExperience;
                if (status.RankCapValue <= status.RankValue)
                {
                    nextRankExperience = experienceModel.RankThreshold.Values[(int)status.RankValue - 2];
                }
                else
                {
                    nextRankExperience = experienceModel.RankThreshold.Values[(int)status.RankValue - 1];
                }
                
                _expStatusDescriptor.Initialize(
                    status.RankValue,
                    status.ExperienceValue,
                    nextRankExperience,
                    item
                );
            }
            _expStatusDescriptor.onClickIncreaseExperience.RemoveAllListeners();
            _expStatusDescriptor.onClickIncreaseExperience.AddListener(OnClickIncreaseExperience);
            _expStatusDescriptor.onClickDecreaseItem.RemoveAllListeners();
            _expStatusDescriptor.onClickDecreaseItem.AddListener(_unitPresenter.OnClickDecreaseItem);
        }
        
        /// <summary>
        /// アイテムの経験値を増加
        /// </summary>
        public void OnClickIncreaseExperience(
            string propertyId,
            int value
        )
        {
#if GS2_ENABLE_UNITASK
            IncreaseItemExperienceAsync(
                propertyId,
                value
            ).Forget();
#else
            StartCoroutine(
                IncreaseItemExperience(
                    propertyId,
                    value
                )
            );
#endif
        }

        /// <summary>
        /// アイテムの経験値を増加
        /// </summary>
        private IEnumerator IncreaseItemExperience(
            string propertyId,
            int value
        )
        {
            yield return _experienceModel.IncreaseExperience(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _experienceSetting.exchangeNamespaceName,
                _experienceSetting.itemExchangeRateName,
                propertyId,
                value,
                _experienceSetting.onError
            );
            
            yield return RefreshItemExperience();
            
            OpenItemStatus(_experienceModel.selectedItem);
        }
#if GS2_ENABLE_UNITASK
        private async UniTask IncreaseItemExperienceAsync(
            string propertyId,
            int value
        )
        {
            await _experienceModel.IncreaseExperienceAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _experienceSetting.exchangeNamespaceName,
                _experienceSetting.itemExchangeRateName,
                propertyId,
                value,
                _experienceSetting.onError
            );
            
            await RefreshItemExperienceAsync();
            
            OpenItemStatus(_experienceModel.selectedItem);
        }
#endif
        
        /// <summary>
        /// アイテム経験値の更新
        /// </summary>
        public IEnumerator RefreshItemExperience()
        {
            yield return _experienceModel.GetItemStatuses(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _experienceSetting.experienceNamespaceName,
                _experienceSetting.onGetStatuses,
                _experienceSetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        public async UniTask RefreshItemExperienceAsync()
        {
           await _experienceModel.GetItemStatusesAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _experienceSetting.experienceNamespaceName,
                _experienceSetting.onGetStatuses,
                _experienceSetting.onError
           );
        }
#endif
    }
}