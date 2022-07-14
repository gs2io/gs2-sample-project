using System.Collections;
using System.Collections.Generic;
using Gs2.Sample.Unit;
using Gs2.Unity.Gs2Experience.Model;
using Gs2.Unity.Gs2Inventory.Model;
using UnityEngine;
using UnityEngine.Assertions;

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
        /// <returns></returns>
        public IEnumerator Initialize()
        {
            UIManager.Instance.AddLog("ExperiencePresenter::Initialize");
            
            yield return _experienceModel.GetPlayerExperienceModel(
                GameManager.Instance.Client,
                _experienceSetting.experienceNamespaceName,
                _experienceSetting.playerExperienceModelName,
                _experienceSetting.onGetExperienceModel,
                _experienceSetting.onError
            );

            yield return _experienceModel.GetItemExperienceModel(
                GameManager.Instance.Client,
                _experienceSetting.experienceNamespaceName,
                _experienceSetting.itemExperienceModelName,
                _experienceSetting.onGetExperienceModel,
                _experienceSetting.onError
            );
            
            yield return Refresh();
        }
        
        /// <summary>
        /// プレイヤーの経験値を増加
        /// </summary>
        public void OnClickIncreasePlayerExperience()
        {
            StartCoroutine(
                IncreaseExperience(increaseValue)
            );
        }
        
        /// <summary>
        /// プレイヤーの経験値を増加
        /// </summary>
        /// <param name="_increaseValue"></param>
        public IEnumerator IncreaseExperience(int _increaseValue)
        {
            void RefreshStaminaAction(
                EzExperienceModel experienceModelTemp,
                EzStatus status,
                int value
            )
            {
                if (experienceModelTemp.Name != _experienceModel.playerExperienceModel.Name)
                {
                    return;
                }

                _experienceSetting.onIncreaseExperience.RemoveListener(RefreshStaminaAction);

                UIManager.Instance.AddLog("ExperienceValue : " + status.ExperienceValue);
            }

            _experienceSetting.onIncreaseExperience.AddListener(RefreshStaminaAction);
            
            yield return _experienceModel.IncreaseExperience(
                GameManager.Instance.Session,
                _experienceSetting.identifierIncreaseExperienceClientId,
                _experienceSetting.identifierIncreaseExperienceClientSecret,
                _experienceSetting.experienceNamespaceName,
                _experienceModel.playerExperienceModel,
                GameManager.Instance.Session.AccessToken.UserId, // プレイヤーのUserId
                _increaseValue,
                _experienceSetting.onIncreaseExperience, 
                _experienceSetting.onError
            );
            
            yield return Refresh();
        }
        
        /// <summary>
        /// プレイヤー経験値の更新
        /// </summary>
        /// <returns></returns>
        public IEnumerator Refresh()
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
                GameManager.Instance.Client,
                GameManager.Instance.Session,
                _experienceSetting.experienceNamespaceName,
                _experienceSetting.onGetStatuses,
                _experienceSetting.onError
            );
        }
                
        /// <summary>
        /// アイテムをクリックした
        /// </summary>
        /// <param name="item"></param>
        public void OnClickItem(EzItemSet item)
        {
            _experienceModel.selectedItem = item;

            OpenItemStatus(item);
        }
        
        /// <summary>
        /// アイテムステータスの表示
        /// </summary>
        /// <param name="item"></param>
        public void OpenItemStatus(EzItemSet item)
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
        /// <param name="propertyId"></param>
        /// <param name="value"></param>
        public void OnClickIncreaseExperience(
            string propertyId,
            int value
        )
        {
            StartCoroutine(
                IncreaseExperience(
                    propertyId,
                    value
                )
            );
        }

        /// <summary>
        /// アイテムの経験値を増加
        /// </summary>
        /// <param name="propertyId"></param>
        /// <param name="value"></param>
        public IEnumerator IncreaseExperience(
            string propertyId,
            int value
        )
        {
            yield return _experienceModel.IncreaseExperience(
                GameManager.Instance.Session,
                _experienceSetting.identifierIncreaseExperienceClientId,
                _experienceSetting.identifierIncreaseExperienceClientSecret,
                _experienceSetting.experienceNamespaceName,
                _experienceModel.itemExperienceModel,
                propertyId,
                value,
                _experienceSetting.onIncreaseExperience,
                _experienceSetting.onError
            );
            
            yield return RefreshItemExperience();
            
            OpenItemStatus(_experienceModel.selectedItem);
        }
        
        /// <summary>
        /// アイテム経験値の更新
        /// </summary>
        /// <returns></returns>
        public IEnumerator RefreshItemExperience()
        {
            yield return _experienceModel.GetItemStatuses(
                GameManager.Instance.Client,
                GameManager.Instance.Session,
                _experienceSetting.experienceNamespaceName,
                _experienceSetting.onGetStatuses,
                _experienceSetting.onError
            );
        }
    }
}