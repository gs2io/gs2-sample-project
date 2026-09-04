using TMPro;
using UnityEngine;

namespace Gs2.Sample
{
    /// <summary>
    /// TextMeshPro のテキストを言語に合わせて差し替える
    /// シーンやプレハブ上の固定文言にこのコンポーネントを付け、キーを指定する
    ///
    /// Replaces a TextMeshPro text according to the current language.
    /// Attach this to a static text in a scene or a prefab and set its key.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedText : MonoBehaviour
    {
        /// <summary>
        /// Assets/Resources/Localization/*.json のキー
        /// </summary>
        [SerializeField] private string key = null;

        /// <summary>
        /// 言語に対応するフォントへ差し替えるか
        ///
        /// 既定では false。このサンプルは Fallback でハングルを補う方式を採っており、
        /// フォントごと差し替えるとアウトライン等のマテリアル設定が既定に戻ってしまうため。
        /// 漢字の字形が言語で異なる場合（中国語など）に、UIManager.localeFonts と併せて有効にする。
        ///
        /// Whether to swap the font asset for the current language.
        /// Disabled by default: this sample covers Hangul with a fallback font, and swapping the
        /// font asset would reset material presets such as the outline.
        /// </summary>
        [SerializeField] private bool applyLocaleFont = false;

        private TMP_Text _text;

        private void OnEnable()
        {
            UIManager.OnLanguageChanged += Apply;
            Apply();
        }

        private void OnDisable()
        {
            UIManager.OnLanguageChanged -= Apply;
        }

        /// <summary>
        /// キーを差し替えて即座に反映する
        /// </summary>
        public void SetKey(string newKey)
        {
            key = newKey;
            Apply();
        }

        private void Apply()
        {
            if (string.IsNullOrEmpty(key))
                return;

            if (_text == null)
                _text = GetComponent<TMP_Text>();

            var uiManager = UIManager.Instance;
            if (uiManager == null)
                return;

            _text.SetText(uiManager.GetLocalizationText(key));

            if (applyLocaleFont)
            {
                var font = uiManager.GetLocaleFont(uiManager.Lang);
                if (font != null)
                    _text.font = font;
            }
        }
    }
}
