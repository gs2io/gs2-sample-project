using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gs2.Unity.Gs2Distributor.Result;
using Gs2.Unity.Gs2Inventory.Model;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
#endif

namespace Gs2.Sample.Gold
{
    public class GoldPresenter : MonoBehaviour
    {
        [SerializeField]
        private GoldSetting _goldSetting;
        
        [SerializeField]
        private GoldModel _goldModel;
        [SerializeField]
        private GoldView _goldView;

        // Start is called before the first frame update
        void Start()
        {
            Assert.IsNotNull(_goldSetting);
            Assert.IsNotNull(_goldModel);
        }
        
        /// <summary>
        /// ゴールドの取得
        /// </summary>
        public IEnumerator Initialize()
        {
            UIManager.Instance.AddLog("GoldPresenter::Initialize");
        
            yield return _goldModel.GetInventoryModel(
                GameManager.Instance.Domain,
                _goldSetting.inventoryNamespaceName,
                _goldSetting.inventoryModelName,
                _goldSetting.onGetInventoryModel,
                _goldSetting.onError
            );
            
            yield return Refresh();
        }

#if GS2_ENABLE_UNITASK
        public async UniTask InitializeAsync()
        {
            UIManager.Instance.AddLog("GoldPresenter::InitializeAsync");
        
            await _goldModel.GetInventoryModelAsync(
                GameManager.Instance.Domain,
                _goldSetting.inventoryNamespaceName,
                _goldSetting.inventoryModelName,
                _goldSetting.onGetInventoryModel,
                _goldSetting.onError
            );
            
            await RefreshAsync();
        }
#endif

        private IEnumerator Refresh()
        {
            void RefreshInventoryAction(
                EzInventory inventory, 
                List<EzItemSet> itemSets
            )
            {
                if (inventory.InventoryName != _goldModel.InventoryModel.Name)
                {
                    return;
                }
                
                _goldModel.Inventory = inventory;
                _goldModel.ItemSets = itemSets;
                _goldModel.ItemSets.Sort((o1, o2) => o1.SortValue != o2.SortValue ? o1.SortValue - o2.SortValue : (int)(o2.Count - o1.Count));
                
                _goldSetting.onGetInventory.RemoveListener(RefreshInventoryAction);

                OnUpdateGold();
            }

            _goldSetting.onGetInventory.AddListener(RefreshInventoryAction);
            
            yield return _goldModel.GetInventory(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _goldSetting.inventoryNamespaceName,
                _goldModel.InventoryModel.Name,
                _goldSetting.onGetInventory,
                _goldSetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        private async UniTask RefreshAsync()
        {
            void RefreshInventoryAction(
                EzInventory inventory, 
                List<EzItemSet> itemSets
            )
            {
                if (inventory.InventoryName != _goldModel.InventoryModel.Name)
                {
                    return;
                }
                
                _goldModel.Inventory = inventory;
                _goldModel.ItemSets = itemSets;
                _goldModel.ItemSets.Sort((o1, o2) => o1.SortValue != o2.SortValue ? o1.SortValue - o2.SortValue : (int)(o2.Count - o1.Count));
                
                _goldSetting.onGetInventory.RemoveListener(RefreshInventoryAction);

                OnUpdateGold();
            }

            _goldSetting.onGetInventory.AddListener(RefreshInventoryAction);
            
            await _goldModel.GetInventoryAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _goldSetting.inventoryNamespaceName,
                _goldModel.InventoryModel.Name,
                _goldSetting.onGetInventory,
                _goldSetting.onError
            );
        }
#endif
        
        public void OnUpdateGold()
        {
            if (_goldModel.ItemSets == null || _goldModel.ItemSets.Count == 0)
            {
                _goldView.SetGold(0);
            }
            else
            {
                _goldView.SetGold(_goldModel.ItemSets[0].Count);
            }
        } 
        
        /// <summary>
        /// ゴールドを増やす リクエスト
        /// </summary>
        /// <param name="acquireValue"></param>
        public void OnClickGold_AcquireButton(int acquireValue)
        {
#if GS2_ENABLE_UNITASK
            _goldModel.AcquireAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _goldSetting.exchangeNamespaceName,
                _goldSetting.exchangeRateName,
                acquireValue,
                _goldSetting.inventoryNamespaceName,
                _goldSetting.inventoryModelName,
                _goldSetting.onAcquire,
                _goldSetting.onError
            ).Forget();
#else
            StartCoroutine(
                _goldModel.Acquire(
                    GameManager.Instance.Domain,
                    GameManager.Instance.Session,
                    _goldSetting.exchangeNamespaceName,
                    _goldSetting.exchangeRateName,
                    acquireValue,
                    _goldSetting.inventoryNamespaceName,
                    _goldSetting.inventoryModelName,
                    _goldSetting.onAcquire,
                    _goldSetting.onError
                )
            );
#endif
        } 
        
        /// <summary>
        /// ゴールドを増やす リザルト
        /// </summary>
        /// <param name="inventory"></param>
        /// <param name="itemSets"></param>
        /// <param name="acquireValue"></param>
        public void OnGold_AcquireResult(
            EzInventory inventory,
            List<EzItemSet> itemSets,
            int acquireValue
        )
        {
            if (inventory.InventoryName != _goldModel.InventoryModel.Name)
            {
                return;
            }
            
            UIManager.Instance.AddLog("acquireValue:"+acquireValue);
            
            _goldModel.Inventory = inventory;
            foreach (var itemSet in itemSets)
            {
                _goldModel.ItemSets = _goldModel.ItemSets.Where(item => item.Name != itemSet.Name).ToList();
                if (itemSet.Count != 0)
                {
                    _goldModel.ItemSets.Add(itemSet);
                }
                
                UIManager.Instance.AddLog("itemSet["+itemSet.ItemName + "]:" + itemSet.Count);
            }
            _goldModel.ItemSets.Sort((o1, o2) => o1.SortValue != o2.SortValue ? o1.SortValue - o2.SortValue : (int)(o2.Count - o1.Count));
            
            OnUpdateGold();
        }
        
        /// <summary>
        /// ゴールドを減らす リクエスト
        /// </summary>
        /// <param name="consumeValue"></param>
        public void OnClickGold_ConsumeButton(int consumeValue)
        {
#if GS2_ENABLE_UNITASK
            _goldModel.ConsumeAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _goldSetting.inventoryNamespaceName,
                _goldSetting.inventoryModelName,
                _goldSetting.itemModelName,
                consumeValue,
                _goldSetting.onConsume,
                _goldSetting.onError
            ).Forget();
#else
            StartCoroutine(
                _goldModel.Consume(
                    GameManager.Instance.Domain,
                    GameManager.Instance.Session,
                    _goldSetting.inventoryNamespaceName,
                    _goldSetting.inventoryModelName,
                    _goldSetting.itemModelName,
                    consumeValue,
                    _goldSetting.onConsume,
                    _goldSetting.onError
                )
            );
#endif
        }

        /// <summary>
        /// ゴールドを減らす リザルト
        /// </summary>
        /// <param name="inventory"></param>
        /// <param name="itemSets"></param>
        /// <param name="consumeValue"></param>
        public void OnGold_ConsumeResult(
            EzInventory inventory,
            List<EzItemSet> itemSets,
            int consumeValue
        )
        {
            _goldSetting.onAcquire.RemoveListener(OnGold_ConsumeResult);
                
            if (inventory.InventoryName != _goldModel.InventoryModel.Name)
            {
                return;
            }
                        
            UIManager.Instance.AddLog("consumeValue : "+consumeValue);
            
            _goldModel.Inventory = inventory;
            foreach (var itemSet in itemSets)
            {
                _goldModel.ItemSets = _goldModel.ItemSets.Where(item => item.Name != itemSet.Name).ToList();
                if (itemSet.Count != 0)
                {
                    _goldModel.ItemSets.Add(itemSet);
                }
                
                UIManager.Instance.AddLog("itemSet["+itemSet.ItemName + "]:" + itemSet.Count);
            }
            _goldModel.ItemSets.Sort((o1, o2) => o1.SortValue != o2.SortValue ? o1.SortValue - o2.SortValue : (int)(o2.Count - o1.Count));
            
            OnUpdateGold();
        }
    }
}