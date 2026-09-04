using Gs2.Unity.Gs2Dictionary.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gs2.Sample.Dictionary
{
    public class EntryItem : MonoBehaviour
    {
        public Image image;
        public TextMeshProUGUI entryName;
        public TextMeshProUGUI state;

        private EzEntryModel _entryModel;
        private bool _registered;

        public void Initialize(
            EzEntryModel entryModel,
            bool registered
        )
        {
            _entryModel = entryModel;
            _registered = registered;
        }

        public void Start()
        {
            // 未登録のエントリーは名前を伏せる
            // Hide the name of unregistered entries
            entryName.SetText(
                _registered
                    ? (string.IsNullOrEmpty(_entryModel.Metadata) ? _entryModel.Name : _entryModel.Metadata)
                    : "？？？"
            );

            state.SetText(
                UIManager.Instance.GetLocalizationText(_registered ? "Registered" : "Unregistered")
            );

            if (image != null)
            {
                image.color = _registered ? Color.white : new Color(0.3f, 0.3f, 0.3f);
            }
        }
    }
}
