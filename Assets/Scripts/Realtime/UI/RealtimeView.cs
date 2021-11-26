using TMPro;
using UnityEngine;

namespace Gs2.Sample.Realtime
{
    public class RealtimeView : MonoBehaviour
    {
        /// <summary>
        /// 他プレイヤーをアタッチするGameObject
        /// </summary>
        [SerializeField]
        public GameObject joinedPlayersContent;

        /// <summary>
        /// 他プレイヤー表示のプレハブ
        /// </summary>
        [SerializeField]
        public GameObject OtherPlayerPrefab;

        [SerializeField]
        private TextMeshProUGUI Count;

        [SerializeField]
        private TextMeshProUGUI Result;
        
        /// <summary>
        /// タップ制御用マスク
        /// </summary>
        [SerializeField]
        public GameObject MaskObject;

        // Start is called before the first frame update
        void Start()
        {
            OnDisableEvent();
        }

        public void OnEnableEvent()
        {
            MaskObject.SetActive(false);
        }
        
        public void OnDisableEvent()
        {
            MaskObject.SetActive(true);
        }

        public void SetPlayerCount(int count)
        {
            Count.SetText(count.ToString());
        }
        
        public void SetResult(string text)
        {
            Result.SetText(text);
        }
    }
}