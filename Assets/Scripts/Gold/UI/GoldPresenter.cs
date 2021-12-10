using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gs2.Sample.Core.Runtime;
using Gs2.Unity.Gs2Distributor.Result;
using Gs2.Unity.Gs2Inventory.Model;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Gs2.Sample.Gold
{
    public class GoldPresenter : MonoBehaviour
    {
        [SerializeField] private GoldSetting _goldSetting;
        
        [SerializeField] private GoldModel _goldModel;
        [SerializeField] private GoldView _goldView;

        /// <summary>
        /// Gs2Client
        /// </summary>
        private Gs2Client _gs2Client;
        /// <summary>
        /// Gs2GameSession
        /// </summary>
        private Gs2GameSession _session;
        
        // Start is called before the first frame update
        void Start()
        {
            Assert.IsNotNull(_goldSetting);
            Assert.IsNotNull(_goldModel);
        }
        
        private void Validate()
        {
            if (_gs2Client == null)
            {
                _gs2Client = GameManager.Instance.Cllient;
            }
            if (_session == null)
            {
                _session = GameManager.Instance.Session;
            }
        }

        /// <summary>
        /// ゴールドの取得
        /// </summary>
        public IEnumerator Initialize()
        {
            UIManager.Instance.AddLog("GoldPresenter::Initialize");

            Validate();
                
            void OnGetInventoryModel(
                string inventoryName, 
                EzInventoryModel inventoryModel, 
                List<EzItemModel> itemModels
            )
            {
                _goldSetting.onGetInventoryModel.RemoveListener(OnGetInventoryModel);
                
                _goldModel.InventoryModel = inventoryModel;
                _goldModel.ItemModels = itemModels;
            }
            
            _goldSetting.onGetInventoryModel.AddListener(OnGetInventoryModel);
        
            yield return _goldModel.GetInventoryModel(
                GameManager.Instance.Cllient.Client,
                _goldSetting.inventoryNamespaceName,
                _goldSetting.inventoryModelName,
                _goldSetting.onGetInventoryModel,
                _goldSetting.onError
            );
            
            yield return Refresh();
        }

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
                GameManager.Instance.Cllient.Client,
                _session.Session,
                _goldSetting.inventoryNamespaceName,
                _goldModel.InventoryModel.Name,
                _goldSetting.onGetInventory,
                _goldSetting.onError
            );
        }
        
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
            StartCoroutine(
                _goldModel.Acquire(
                    _session.Session,
                    _goldSetting.identifierAcquireGoldClientId,
                    _goldSetting.identifierAcquireGoldClientSecret,
                    _goldSetting.inventoryNamespaceName,
                    _goldSetting.inventoryModelName,
                    _goldSetting.itemModelName,
                    acquireValue,
                    _goldSetting.onAcquire,
                    _goldSetting.onError
                )
            );
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
            StartCoroutine(
                _goldModel.Consume(
                    GameManager.Instance.Cllient.Client,
                    GameManager.Instance.Session.Session,
                    _goldSetting.inventoryNamespaceName,
                    _goldSetting.inventoryModelName,
                    _goldSetting.itemModelName,
                    consumeValue,
                    _goldSetting.onConsume,
                    _goldSetting.onError
                )
            );
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
        
        public UnityAction<EzStampTask, EzRunStampTaskResult> Gold_GetTaskCompleteAction()
        {
            return (task, taskResult) =>
            {
                UIManager.Instance.AddLog("GoldPresenter::GetTaskCompleteAction");

                if (task.Action == "Gs2Inventory:ConsumeItemSetByUserId")
                {
                    StartCoroutine(
                        Refresh()
                    );
                }
            };
        }

        public UnityAction<EzStampSheet, EzRunStampSheetResult> Gold_GetSheetCompleteAction()
        {
            return (sheet, sheetResult) =>
            {
                UIManager.Instance.AddLog("GoldPresenter::GetSheetCompleteAction");

                if (sheet.Action == "Gs2Inventory:AcquireItemSetByUserId")
                {
                    StartCoroutine(
                        Refresh()
                    );
                }
            };
        }
    }
}