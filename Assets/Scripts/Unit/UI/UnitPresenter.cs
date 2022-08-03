using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gs2.Sample.Experience;
using Gs2.Sample.Inventory;
using Gs2.Unity.Gs2Distributor.Result;
using Gs2.Unity.Gs2Inventory.Model;
using Gs2.Unity.Gs2JobQueue.Model;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
#endif

namespace Gs2.Sample.Unit
{
    public class UnitPresenter : MonoBehaviour
    {
        [SerializeField]
        private UnitSetting _unitSetting;
        
        [SerializeField]
        private UnitModel _unitModel;
        [SerializeField]
        private UnitView _unitView;
        
        [SerializeField]
        private ExperiencePresenter _experiencePresenter;
        
        public UpdateInventoryEvent onUpdateInventoryEvent = new UpdateInventoryEvent();

        public enum State
        {
            MainMenu,

            /// <summary>
            /// インベントリ情報を取得中
            /// Inventory information is being retrieved.
            /// </summary>
            GetInventoryProcessing,
            /// <summary>
            /// インベントリ情報の取得に失敗
            /// Failed to retrieve inventory information
            /// </summary>
            GetInventoryFailed,
            
            /// <summary>
            /// インベントリメニュー
            /// inventory menu
            /// </summary>
            InventoryMenu,
        }
        
        /// <summary>
        /// 現在のステート
        /// Current State
        /// </summary>
        private State _inventoryState = State.MainMenu;
        
        public void Start()
        {
            Assert.IsNotNull(_unitSetting);
            Assert.IsNotNull(_unitModel);
            Assert.IsNotNull(_unitView);
        }

        private void SetState(State _state)
        {
            if (_inventoryState != _state)
            {
                switch (_state)
                {
                    default:
                        UIManager.Instance.CloseProcessing();
                        _unitView.OnCloseEvent();
                        break;

                    case State.GetInventoryProcessing:
                        UIManager.Instance.OpenProcessing();
                        break;
                    
                    case State.InventoryMenu:
                        UIManager.Instance.CloseProcessing();
                        _unitView.OnOpenEvent();
                        break;
                }
            }
            
            _inventoryState = _state;
        }

        private bool IsOpenInventory()
        {
            return _inventoryState == State.InventoryMenu;
        }
        
        /// <summary>
        /// インベントリの初期化
        /// Inventory initialization
        /// </summary>
        /// <param name="client"></param>
        /// <param name="session"></param>
        /// <param name="config"></param>
        public IEnumerator Initialize()
        {
            UIManager.Instance.AddLog("UnitPresenter::Initialize");

            yield return _unitModel.GetInventoryModel(
                GameManager.Instance.Domain,
                _unitSetting.inventoryNamespaceName,
                _unitSetting.inventoryModelName,
                _unitSetting.onGetInventoryModel,
                _unitSetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        public async UniTask InitializeAsync()
        {
            UIManager.Instance.AddLog("UnitPresenter::InitializeAsync");
        
            await _unitModel.GetInventoryModelAsync(
                GameManager.Instance.Domain,
                _unitSetting.inventoryNamespaceName,
                _unitSetting.inventoryModelName,
                _unitSetting.onGetInventoryModel,
                _unitSetting.onError
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

        public IEnumerator OpenInventory()
        {
            SetState(State.GetInventoryProcessing);
            
            _unitSetting.onConsume.AddListener(ConsumeAction);
            
            yield return Refresh();

            yield return _experiencePresenter.RefreshItemExperience();
            
            SetState(State.InventoryMenu);
        }
        
        public IEnumerator Refresh()
        {
            void RefreshInventoryAction(
                EzInventory inventory, 
                List<EzItemSet> itemSets
            )
            {
                if (inventory.InventoryName != _unitModel.Model.Name)
                {
                    return;
                }
                
                _unitModel.Inventory = inventory;
                _unitModel.ItemSets = itemSets;
                _unitModel.ItemSets.Sort((o1, o2) => o1.SortValue != o2.SortValue ? o1.SortValue - o2.SortValue : (int)(o2.Count - o1.Count));
                
                _unitSetting.onGetInventory.RemoveListener(RefreshInventoryAction);
                
                OnChangeInventory(inventory, itemSets);
            }

            _unitSetting.onGetInventory.AddListener(RefreshInventoryAction);
            
            yield return _unitModel.GetInventory(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _unitSetting.inventoryNamespaceName,
                _unitSetting.inventoryModelName,
                _unitSetting.onGetInventory,
                _unitSetting.onError
            );
        }
#if GS2_ENABLE_UNITASK
        public async UniTask OpenInventoryAsync()
        {
            SetState(State.GetInventoryProcessing);
            
            _unitSetting.onConsume.AddListener(ConsumeAction);
            
            await RefreshAsync();

            await _experiencePresenter.RefreshItemExperienceAsync();
            
            SetState(State.InventoryMenu);
        }
        
        public async UniTask RefreshAsync()
        {
            void RefreshInventoryAction(
                EzInventory inventory, 
                List<EzItemSet> itemSets
            )
            {
                _unitSetting.onGetInventory.RemoveListener(RefreshInventoryAction);
                
                if (inventory.InventoryName != _unitModel.Model.Name)
                {
                    return;
                }
                
                _unitModel.Inventory = inventory;
                _unitModel.ItemSets = itemSets;
                _unitModel.ItemSets.Sort((o1, o2) => o1.SortValue != o2.SortValue ? o1.SortValue - o2.SortValue : (int)(o2.Count - o1.Count));
                
                OnChangeInventory(inventory, itemSets);
            }

            _unitSetting.onGetInventory.AddListener(RefreshInventoryAction);
            
            await _unitModel.GetInventoryAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _unitSetting.inventoryNamespaceName,
                _unitSetting.inventoryModelName,
                _unitSetting.onGetInventory,
                _unitSetting.onError
            );
        }
#endif
        
        public void OnChangeInventory(
            EzInventory inventory,
            List<EzItemSet> itemSets
            )
        {
            for (var i = 0; i < _unitView.contentTransform.childCount; i++)
            {
                var _gameObject = _unitView.contentTransform.GetChild(i).gameObject;
                if (_gameObject == _unitView.unitItemPrefab.gameObject)
                    continue;
                if (_gameObject == _unitView.unitItemEmptyPrefab.gameObject)
                    continue;
                Destroy(_gameObject);
            }

            foreach (var itemSet in _unitModel.ItemSets)
            {
                var item = Instantiate(_unitView.unitItemPrefab, _unitView.contentTransform);
                item.Initialize(
                    _unitModel.ItemModels.First(itemModel => itemModel.Name == itemSet.ItemName),
                    itemSet
                );
                
                item.onClickItem.AddListener(_experiencePresenter.OnClickItem);
                
                item.gameObject.SetActive(true);
            }

            var units = _unitModel.Inventory;
            for (var i = _unitModel.ItemSets.Count; i < units.CurrentInventoryMaxCapacity; i++)
            {
                var item = Instantiate(_unitView.unitItemEmptyPrefab, _unitView.contentTransform);
                item.gameObject.SetActive(true);
            }

            _unitView.capacity.SetText(
                $"{units.CurrentInventoryCapacityUsage} / {units.CurrentInventoryMaxCapacity}");
        }
        
        public void Close()
        {
            _unitSetting.onConsume.RemoveListener(ConsumeAction);
            
            SetState(State.MainMenu);
        }

        /// <summary>
        /// アイテムを消費する リクエスト
        /// Consume Item Request
        /// </summary>
        public void OnClickDecreaseItem(
            EzItemSet itemSet
        )
        {
#if GS2_ENABLE_UNITASK
            _unitModel.ConsumeAsync(
                GameManager.Instance.Domain,
                GameManager.Instance.Session,
                _unitSetting.inventoryNamespaceName,
                _unitSetting.inventoryModelName,
                itemSet.ItemName,
                1,
                _unitSetting.onConsume,
                _unitSetting.onError
            ).Forget();
#else
            StartCoroutine(
                _unitModel.Consume(
                    GameManager.Instance.Domain,
                    GameManager.Instance.Session,
                    _unitSetting.inventoryNamespaceName,
                    _unitSetting.inventoryModelName,
                    itemSet.ItemName,
                    1,
                    _unitSetting.onConsume,
                    _unitSetting.onError
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
            if (inventory.InventoryName != _unitModel.Model.Name)
            {
                return;
            }
            
            _unitModel.Inventory = inventory;
            foreach (var itemSet in itemSets)
            {
                _unitModel.ItemSets = _unitModel.ItemSets.Where(item => item.Name != itemSet.Name).ToList();
                if (itemSet.Count != 0)
                {
                    _unitModel.ItemSets.Add(itemSet);
                }
            }
            _unitModel.ItemSets.Sort((o1, o2) => o1.SortValue != o2.SortValue ? o1.SortValue - o2.SortValue : (int)(o2.Count - o1.Count));
            
            OnChangeInventory(inventory, _unitModel.ItemSets);
        }
    }
}