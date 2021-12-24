using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Gs2.Sample.Quest
{
    public class SelectQuestView : MonoBehaviour
    {
        public GameObject QuestGroupMenu;
        
        public GameObject QuestMenu;
        
        public GameObject QuestClear;

        /// <summary>
        /// クエストグループ　項目の親
        /// </summary>
        public GameObject questGroupsContent;

        /// <summary>
        /// クエストグループ項目 クローン元のGameObject
        /// </summary>
        /// <returns></returns>
        public GameObject questGroupInfo;

        /// <summary>
        /// クエスト　項目の親
        /// </summary>
        public GameObject questsContent;

        /// <summary>
        /// クエスト項目 クローン元のGameObject
        /// </summary>
        /// <returns></returns>
        public GameObject questInfo;

        public Button questStart;
        
        public Button questEnd;

        private void Start()
        {
            Assert.IsNotNull(questStart);
            Assert.IsNotNull(questEnd);
        }

        public void OnCloseEvent()
        {
            QuestGroupMenu.gameObject.SetActive(false);
            QuestMenu.gameObject.SetActive(false); 
            QuestClear.gameObject.SetActive(false);
        }
        
        public void OnOpenQuestGroupMenuEvent()
        {
            QuestGroupMenu.SetActive(true);
        }
        
        public void OnCloseQuestGroupMenuEvent()
        {
            QuestGroupMenu.SetActive(false);
        }
        
        public void OnOpenQuestMenuEvent()
        {
            QuestMenu.SetActive(true);
        }
        
        public void OnCloseQuestMenuEvent()
        {
            QuestMenu.SetActive(false);
        }
        
        public void OnOpenQuestClearEvent()
        {
            QuestClear.SetActive(true);
        }
        
        public void OnCloseQuestClearEvent()
        {
            QuestClear.SetActive(false);
        }
    }
}