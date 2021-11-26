using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Gs2.Sample.Quest
{
    /// <summary>
    /// クエストグループ情報表示　リスト内項目
    /// </summary>
    public class QuestGroupView : MonoBehaviour
    {
        public TextMeshProUGUI questTypeName;

        public Button button;
        
        public void Initialize(QuestGroupInformation questGroup, UnityAction onClick)
        {
            questTypeName.text = questTypeName.text
                .Replace("{quest_group_name}", questGroup.ScreenName) ;
            button.onClick.AddListener(onClick);
        }
    }
}