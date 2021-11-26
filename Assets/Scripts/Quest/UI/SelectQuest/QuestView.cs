using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Gs2.Sample.Quest
{
    /// <summary>
    /// クエスト情報表示　リスト内項目
    /// </summary>
    public class QuestView : MonoBehaviour
    {
        public TextMeshProUGUI questName;

        public TextMeshProUGUI consumeStamina;

        public TextMeshProUGUI status;

        public Button button;
        
        public void Initialize(QuestInformation quest, UnityAction onClick)
        {
            questName.text = questName.text
                .Replace("{quest_name}", quest.ScreenName);
            consumeStamina.text = consumeStamina.text
                .Replace("{consume_stamina}", quest.consumeStamina.ToString());
            status.text = status.text
                .Replace("{status}", quest.completed ? "Completed" : "Open");
            button.onClick.AddListener(onClick);
            
            gameObject.SetActive(quest.open);
        }
    }
}