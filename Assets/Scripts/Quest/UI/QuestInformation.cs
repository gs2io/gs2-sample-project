using System.Collections.Generic;
using System.Linq;
using Gs2.Gs2Stamina.Request;
using Gs2.Sample.Core;
using Gs2.Unity.Gs2Quest.Model;
using Gs2.Util.LitJson;

namespace Gs2.Sample.Quest
{
    public class QuestInformation
    {
        public string Id;
        public string Name;
        public string ScreenName;
        public int? consumeStamina;
        public bool open;
        public bool completed;

        public QuestInformation(EzQuestModel model, EzCompletedQuestList currentCompletedQuestList)
        {
            Id = model.QuestModelId;
            Name = model.Name;
            ScreenName = model.Metadata;
            var action = GetConsumeAction<ConsumeStaminaByUserIdRequest>(
                model,
                "Gs2Stamina:ConsumeStaminaByUserId"
            );
            if (action != null)
            {
                consumeStamina = action.ConsumeValue;
            }

            if (currentCompletedQuestList == null)
            {
                currentCompletedQuestList = new EzCompletedQuestList
                {
                    CompleteQuestNames = new List<string>()
                };
            }
            
            var premiseQuestNames = new HashSet<string>(model.PremiseQuestNames);
            premiseQuestNames.ExceptWith(currentCompletedQuestList.CompleteQuestNames);
            open = premiseQuestNames.Count == 0;
            completed = currentCompletedQuestList.CompleteQuestNames.Contains(model.Name);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="quest"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetConsumeAction<T>(
            EzQuestModel quest,
            string action
        )
        {
            var item = quest.ConsumeActions.FirstOrDefault(consumeAction => consumeAction.Action == action);
            if (item == null)
            {
                return default;
            }
            return (T)typeof(T).GetMethod("FromJson")?.Invoke(null, new object[] { Gs2Util.RemovePlaceholder(JsonMapper.ToObject(item.Request)) });
        }

    }
}