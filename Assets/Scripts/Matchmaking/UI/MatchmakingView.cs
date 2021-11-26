using System.Collections;
using System.Collections.Generic;
using Gs2.Unity.Gs2Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gs2.Sample.Matchmaking
{
    public class MatchmakingView : MonoBehaviour
    {
        /// <summary>
        /// 参加者リストを表示する GameObject
        /// </summary>
        [SerializeField]
        public GameObject joinedPlayersContent;

        /// <summary>
        /// 参加者リストにプレイヤー名を表示するプレハブ
        /// </summary>
        [SerializeField]
        public GameObject displayPlayerNamePrefab;

        public void OnOpenEvent()
        {
            gameObject.SetActive(true);
        }
        
        public void OnCloseEvent()
        {
            gameObject.SetActive(false);
        }
    }
}