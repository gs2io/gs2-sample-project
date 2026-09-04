using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gs2.Sample.Dictionary
{
    public class DictionaryView : MonoBehaviour
    {
        [SerializeField]
        public EntryItem entryItemPrefab;

        /// <summary>
        /// 登録済み件数 / 全件数
        /// Number of registered entries / total number of entries
        /// </summary>
        [SerializeField]
        public TextMeshProUGUI progress;

        [SerializeField]
        public Transform contentTransform;

        [SerializeField]
        public ScrollRect scrollRect;

        /// <summary>
        /// 図鑑エントリーを1件登録するボタン
        /// Button that registers one dictionary entry
        /// </summary>
        [SerializeField]
        public Button registerButton;

        /// <summary>
        /// 図鑑エントリーをすべて削除するボタン
        /// Button that deletes all the dictionary entries
        /// </summary>
        [SerializeField]
        public Button resetButton;

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

        public void SetProgress(int registeredCount, int totalCount)
        {
            if (progress != null)
            {
                progress.SetText($"{registeredCount} / {totalCount}");
            }
        }

        public void SetInteractable(bool interactable)
        {
            if (registerButton != null)
            {
                registerButton.interactable = interactable;
            }
            if (resetButton != null)
            {
                resetButton.interactable = interactable;
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
