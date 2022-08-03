using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gs2.Unity.Gs2Inventory.Model;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
#endif

namespace Gs2.Sample.Inventory
{
    [Serializable]
    public class UpdateInventoryEvent : UnityEvent<EzInventory, List<EzItemSet>>
    {
    }

    [Serializable]
    public class ClickAcquireButtonEvent : UnityEvent<string, int>
    {
    }
    
    public class InventoryPresenter : MonoBehaviour
    {
        [SerializeField]
        private InventorySetting _inventorySetting;
        
        [SerializeField]
        public InventoryModel _inventoryModel;
        [SerializeField]
        public InventoryView _inventoryView;
        
        public ClickAcquireButtonEvent onClickAcquireButton = new ClickAcquireButtonEvent();

        public int acquireCount = 5;

        public enum State
        {
            MainMenu,

            /// <summary>
            /// インベントリ情報を取得中
            /// </summary>
            GetInventoryProcessing,
            /// <summary>
            /// インベントリ情報の取得に失敗
            /// </summary>
            GetInventoryFailed,
            
            /// <summary>
            /// インベントリ表示
            /// </summary>
            InventoryMenu,
        }
        
        /// <summary>
        /// 現在のステート
        /// </summary>
        private State _inventoryState = State.MainMenu;

        private void Start()
        {
            Assert.IsNotNull(_inventorySetting);
            Assert.IsNotNull(_inventoryModel);
            Assert.IsNotNull(_inventoryView);
            
            _inventoryView.onUseItem.AddListener(OnUseItem);
        }

        private void SetState(State _state)
        {
            if (_inventoryState != _state)
            {
                switch (_state)
                {
                    default:
                        UIManager.Instance.CloseProcessing();
                        _inventoryView.OnCloseEvent();
                        break;

                    case State.GetInventoryProcessing:
                        UIManager.Instance.OpenProcessing();
                        break;
                    
                    case State.InventoryMenu:
                        UIManager.Instance.CloseProcessing();
                        _inventoryView.OnOpenEvent();
                        break;
                }
            }
            
            _inventoryState = _state;
        }

        /// <summary>
        /// インベントリの初期化
        /// </summary>
        public IEnumerator Initialize()
        {
            UIManager.Instance.AddLog("InventoryPresenter::Initialize");

            yield return _inventoryModel.GetInventoryModel(
                GameManager.Instance.Domain,
                _inventorySetting.inventoryNamespaceName,
                _inventorySetting.inventoryModelName,
                _inventorySetting.onGetInventoryModel,
                _inventorySetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        public async UniTask InitializeAsync()
        {
            UIManager.Instance.AddLog("InventoryPresenter::InitializeAsync");

            await _inventoryModel.GetInventoryModelAsync(
                GameManager.Instance.Domain,
                _inventorySetting.inventoryNamespaceName,
                _inventorySetting.inventoryModelName,
                _inventorySetting.onGetInventoryModel,
                _inventorySetting.onError
            );
        }
#endif
        
        public void Open()
        {
#if GS2_ENABLE_UNITASK
            OpenInventoryAsync().Forget();
#else
            StartCoroutine(
                OpenInventory()
            );
#endif
        }

        private IEnumerator OpenInventory()
        {
            SetState(State.GetInventoryProcessing);
            
            _inventorySetting.onAcquire.AddListener(AcquireAction);
            _inventorySetting.onConsume.AddListener(ConsumeAction);
            
            yield return Refresh();

            SetState(State.InventoryMenu);
        }
        
        private IEnumerator Refresh()
        {
            void RefreshInventoryAction(
                EzInventory inventory, 
                List<EzItemSet> itemSets
            )
            {
                if (inventory.InventoryName != _inventoryModel.Model.Name)
                {
                    return;
                }
                
                _inventoryModel.ItemSets.Sort((o1, o2) => o1.SortValue != o2.SortValue ? o1.SortValue - o2.SortValue : (int)(o2.Count - o1.Count));
                
                _inventorySetting.onGetInventory.RemoveListener(RefreshInventoryAction);
                
                OnChangeInventory(inventory, itemSets);
            }

            _inventorySetting.onGetInventory.AddListener(RefreshInventoryAction);
            
            yield return _inventoryModel.GetInventory(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _inventorySetting.inventoryNamespaceName,
                _inventorySetting.inventoryModelName,
                _inventorySetting.onGetInventory,
                _inventorySetting.onError
            );
        }
        
#if GS2_ENABLE_UNITASK
        private async UniTask OpenInventoryAsync()
        {
            SetState(State.GetInventoryProcessing);
            
            _inventorySetting.onAcquire.AddListener(AcquireAction);
            _inventorySetting.onConsume.AddListener(ConsumeAction);
            
            await RefreshAsync();

            SetState(State.InventoryMenu);
        }
        
        private async UniTask RefreshAsync()
        {
            void RefreshInventoryAction(
                EzInventory inventory, 
                List<EzItemSet> itemSets
            )
            {
                _inventorySetting.onGetInventory.RemoveListener(RefreshInventoryAction);
                
                if (inventory.InventoryName != _inventoryModel.Model.Name)
                {
                    return;
                }
                
                _inventoryModel.ItemSets.Sort((o1, o2) => o1.SortValue != o2.SortValue ? o1.SortValue - o2.SortValue : (int)(o2.Count - o1.Count));
                
                OnChangeInventory(_inventoryModel.Inventory, _inventoryModel.ItemSets);
            }

            _inventorySetting.onGetInventory.AddListener(RefreshInventoryAction);
            
            await _inventoryModel.GetInventoryAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _inventorySetting.inventoryNamespaceName,
                _inventorySetting.inventoryModelName,
                _inventorySetting.onGetInventory,
                _inventorySetting.onError
            );
        }
#endif
        
        public void OnChangeInventory(
            EzInventory inventory,
            List<EzItemSet> itemSets
        )
        {
            for (var i = 0; i < _inventoryView.contentTransform.childCount; i++)
            {
                var _gameObject = _inventoryView.contentTransform.GetChild(i).gameObject;
                if (_gameObject == _inventoryView.inventoryItemPrefab.gameObject)
                    continue;
                if (_gameObject == _inventoryView.inventoryItemEmptyPrefab.gameObject)
                    continue;
                Destroy(_gameObject);
            }

            foreach (var itemSet in itemSets)
            {
                if (itemSet.Name == "null") continue;
                
                var item = Instantiate(_inventoryView.inventoryItemPrefab, _inventoryView.contentTransform);
                item.Initialize(
                    itemSet
                );
                item.onClickItem.AddListener(OnUseItem);
                item.gameObject.SetActive(true);
            }

            for (var i = itemSets.Count; i < inventory.CurrentInventoryMaxCapacity; i++)
            {
                Instantiate(_inventoryView.inventoryItemEmptyPrefab, _inventoryView.contentTransform);
            }

            _inventoryView.scrollRect.verticalNormalizedPosition = 1.0f;
            
            _inventoryView.capacity.SetText(
                $"{inventory.CurrentInventoryCapacityUsage} / {inventory.CurrentInventoryMaxCapacity}");
        }
        
        public void Close()
        {
            _inventorySetting.onAcquire.RemoveListener(AcquireAction);
            _inventorySetting.onConsume.RemoveListener(ConsumeAction);
            
            SetState(State.MainMenu);
        }
        
        public void OnClickFireElementButton()
        {
            UIManager.Instance.AddLog("InventoryPresenter::OnClickFireElementButton");
            
            OnClickAcquireButton(
                _inventorySetting.exchangeRateNameFire
            );
        }

        public void OnClickWaterElementButton()
        {
            UIManager.Instance.AddLog("InventoryPresenter::OnClickWaterElementButton");
            
            OnClickAcquireButton(
                _inventorySetting.exchangeRateNameWater
            );
        }

        public void OnClickAcquireButton(
            string itemModelName
        )
        {
            onClickAcquireButton.Invoke(
                itemModelName,
                acquireCount
            );
        }
        
        /// <summary>
        /// アイテムを増やす リクエスト
        /// </summary>
        /// <param name="itemModelName"></param>
        /// <param name="count">個数</param>
        public void OnClickAcquireItemButton(
            string itemModelName,
            int count
        )
        {
#if GS2_ENABLE_UNITASK
            _inventoryModel.AcquireAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _inventorySetting.exchangeNamespaceName,
                itemModelName,
                count,
                _inventorySetting.inventoryNamespaceName,
                _inventorySetting.inventoryModelName,
                _inventorySetting.onAcquire,
                _inventorySetting.onError
            ).Forget();
#else
            StartCoroutine(
                _inventoryModel.Acquire(
                    GameManager.Instance.Domain,
                    GameManager.Instance.Session,
                    _inventorySetting.exchangeNamespaceName,
                    itemModelName,
                    count,
                    _inventorySetting.inventoryNamespaceName,
                    _inventorySetting.inventoryModelName,
                    _inventorySetting.onAcquire,
                    _inventorySetting.onError
                )
            );
#endif
        }

        private void AcquireAction(
            EzInventory inventory, 
            List<EzItemSet> itemSets, 
            int acquireValue
        )
        {
            if (inventory.InventoryName != _inventoryModel.Model.Name)
            {
                return;
            }
            
            _inventoryModel.ItemSets.Sort((o1, o2) => o1.SortValue != o2.SortValue ? o1.SortValue - o2.SortValue : (int)(o2.Count - o1.Count));

            OnChangeInventory(inventory, _inventoryModel.ItemSets);
        }
        
        /// <summary>
        /// アイテムを消費する リクエスト
        /// </summary>
        /// <param name="itemSet"></param>
        public void OnUseItem(
            EzItemSet itemSet
        )
        {
#if GS2_ENABLE_UNITASK
            _inventoryModel.ConsumeAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _inventorySetting.inventoryNamespaceName,
                _inventorySetting.inventoryModelName,
                itemSet.ItemName,
                1,
                _inventorySetting.onConsume,
                _inventorySetting.onError
            ).Forget();
#else
            StartCoroutine(
                _inventoryModel.Consume(
                    GameManager.Instance.Domain,
                    GameManager.Instance.Session,
                    _inventorySetting.inventoryNamespaceName,
                    _inventorySetting.inventoryModelName,
                    itemSet.ItemName,
                    1,
                    _inventorySetting.onConsume,
                    _inventorySetting.onError
                )
            );
#endif
        }
        
        private void ConsumeAction(
            EzInventory inventory, 
            List<EzItemSet> itemSets, 
            int consumeValue
        )
        {
            if (inventory.InventoryName != _inventoryModel.Model.Name)
            {
                return;
            }

            foreach (var itemSet in itemSets)
            {
                _inventoryModel.ItemSets = _inventoryModel.ItemSets.Where(item => item.Name != itemSet.Name).ToList();
                if (itemSet.Count != 0 && itemSet.ItemName != String.Empty)
                {
                    _inventoryModel.ItemSets.Add(itemSet);
                }
            }

            _inventoryModel.ItemSets.Sort((o1, o2) => o1.SortValue != o2.SortValue ? o1.SortValue - o2.SortValue : (int)(o2.Count - o1.Count));
            
            OnChangeInventory(inventory, _inventoryModel.ItemSets);
        }
    }
}