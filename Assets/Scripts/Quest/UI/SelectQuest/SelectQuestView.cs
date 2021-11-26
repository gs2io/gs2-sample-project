using System;
using System.Collections.Generic;
using System.Linq;
using Gs2.Core.Util;
using Gs2.Sample.Core;
using Gs2.Sample.Money;
using Gs2.Sample.Stamina;
using Gs2.Unity.Gs2Quest.Model;
using UnityEngine;

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
        public GameObject QuestGroupInfo;

        /// <summary>
        /// クエスト　項目の親
        /// </summary>
        public GameObject questsContent;

        /// <summary>
        /// クエスト項目 クローン元のGameObject
        /// </summary>
        /// <returns></returns>
        public GameObject QuestInfo;

        public void OnOpenEvent()
        {
            gameObject.SetActive(true);
        }
        
        public void OnCloseEvent()
        {
            gameObject.SetActive(false);
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