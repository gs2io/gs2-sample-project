using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gs2.Sample.Enhance
{
    public class EnhanceView : MonoBehaviour
    {
        [SerializeField]
        public CharacterItem characterItemPrefab;

        /// <summary>
        /// 所持している強化素材の数
        /// Number of enhancement materials the player has
        /// </summary>
        [SerializeField]
        public TextMeshProUGUI materialCount;

        [SerializeField]
        public Transform contentTransform;

        [SerializeField]
        public ScrollRect scrollRect;

        /// <summary>
        /// 動作確認用に強化対象を入手するボタン
        /// Button that grants an enhancement target for verification purposes
        /// </summary>
        [SerializeField]
        public Button getCharacterButton;

        /// <summary>
        /// 動作確認用に強化素材を入手するボタン
        /// Button that grants enhancement materials for verification purposes
        /// </summary>
        [SerializeField]
        public Button getMaterialButton;

        public void OnEnable()
        {
            StartCoroutine(ScrollTop());
        }

        private IEnumerator ScrollTop()
        {
            yield return null;
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 1.0f;
            }
        }

        public void SetMaterialCount(long count)
        {
            if (materialCount != null)
            {
                materialCount.SetText($"{count}");
            }
        }

        public void SetInteractable(bool interactable)
        {
            if (getCharacterButton != null)
            {
                getCharacterButton.interactable = interactable;
            }
            if (getMaterialButton != null)
            {
                getMaterialButton.interactable = interactable;
            }
        }

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
