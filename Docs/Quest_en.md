# Quest Description

This is a sample of using [GS2-Quest](https://app.gs2.io/docs/en/index.html#gs2-quest) to manage quests.

There are two types (two groups) of quests: main scenario quests and character scenario quests.  
Quests can have a cost to attempt the quest and a reward for completing the quest, but  
In this sample, the required cost is set to stamina and the clear reward is set to the currency charged.  
If the quest fails, the reward is set to return the stamina spent as cost.

## GS2-Deploy template

- [initialize_quest_template.yaml - Quest](../Templates/initialize_quest_template.yaml)

## QuestSetting QuestSetting

![Inspector Window](Quest.png)

| Setting Name | Description |
---|---
| questNamespaceName | Namespace name of GS2-Quest
| questKeyId | cryptographic key used for the signature calculation of the stamp sheet issued by GS2-Quest for the reward granting process
| distributorNamespaceName | namespace name of the GS2-Distributor who delivers the reward
| queueNamespaceName | Namespace name of GS2-JobQueue used for granting rewards

| Event | Description |
---|---
| OnListCompletedQuestModel(List<EzCompletedQuestList> completedQuests) | When the list of completed quests is retrieved. | when a list of completed quests is retrieved.
| OnListGroupQuestModel(List<EzQuestGroupModel> questGroups) | When a list of quest groups is retrieved.
| OnListQuestModel(List<EzQuestModel> quests) | OnListQuestModel(List<EzQuestModel> quests)
| OnGetProgress(EzProgress progress) | When a quest that is in progress and interrupted is retrieved. | OnGetProgress(EzProgress)
| OnStart(EzProgress progress) | When a quest is started. | OnStart(EzProgress)
| OnEnd(EzProgress progress, List<EzReward> rewards, bool isComplete) | When a quest is completed. | When a quest is completed.
| OnError(Gs2Exception error) | Called when an error occurs. | OnError(Gs2Exception error)

## Quest Flow

After login, get if there are any quests in progress.  
The QUEST STATE will be `None` if it does not exist and `QuestStarted` if it does.  
If no quests have been started, then `Start Quest` => Select Quest Group => Select Quest and press  
Start the quest.

From `Complete Quest', choose to complete or fail (discard) the quest and receive a reward or  
Receive a refund of the required cost.

### Get Quest Status

```c#
AsyncResult<EzGetProgressResult> result = null;
yield return client.Quest.GetProgress(
    r => { result = r; }
    session,
    questNamespaceName
);
```

### Get list of quest groups

Obtains a list of quest groups.

```c#
AsyncResult<EzListQuestGroupsResult> result = null;
yield return client.Quest.ListQuestGroups(
    r => { result = r; }
    questNamespaceName
);
```

Retrieve completed quests.

```c#
AsyncResult<EzDescribeCompletedQuestListsResult> result = null;
yield return client.Quest.DescribeCompletedQuestLists(
    r => { result = r; }
    session,
    questNamespaceName,
    null,
    30
);
```

### Get list of quests

Obtains a list of quests.

```c#
AsyncResult<EzListQuestsResult> result = null;
yield return client.Quest.ListQuests(
    r => { result = r; }
    questNamespaceName,
    SelectedQuestGroup.Name
);
```

### Starting a quest

Starts a quest.
The return value is a stamp sheet.
The stamp sheet consumes the amount of stamina set for the quest as the required cost of executing the stamp sheet, and  
Starts a quest.

```c#
AsyncResult<EzStartResult> result = null;
yield return client.Quest.Start(
    r => { result = r; }
    session,
    questNamespaceName,
    SelectedQuestGroup.Name,
    SelectedQuest,
    false,
    config: new List<EzConfig>
    {
        new EzConfig
        {
            Key = "slot",
            Value = MoneyModel.Slot.ToString(),
        }
    }
);

stampSheet = result.Result.StampSheet;
```

```c#
EzProgress progress = null;
var machine = new StampSheetStateMachine(
    stampSheet,
    client,
    distributorNamespaceName,
    questKeyId
);

Gs2Exception exception = null;
void OnError(Gs2Exception e)
{
    exception = e;
};

void OnComplete(EzStampSheet sheet, Gs2.Unity.Gs2Distributor.Result.EzRunStampSheetResult stampResult)
{
    var json = JsonMapper.ToObject(stampResult.Result);
    var result = CreateProgressByStampSheetResult.FromJson(json);
    var progress = EzProgress.FromModel(result.Item); var result = CreateProgressByStampSheetResult;
};

yield return machine.Execute(onError);
```

The following is the flow of the quest start stamp sheet.

![Quest Start](QuestStart_en.png)

### Completion of quest

Complete/fail (discard) the quest.  
rewards is the value of Rewards in EzProgress, the return value of Start.  
Set the reward actually obtained.

The return value of End is a stamp sheet.  
By executing the stamp sheet, the quest is completed and the reward is received.

```c#
AsyncResult<EzEndResult> result = null;
yield return client.Quest.End(
    r => { result = r; }
    session,
    questNamespaceName,
    isComplete,
    rewards,
    Progress.TransactionId,
    new List<EzConfig>
    {
        new EzConfig
        {
            Key = "slot",
            Value = slot.ToString(),
        }
    }
);
```

```c#
var machine = new StampSheetStateMachine(
    stampSheet,
    client,
    distributorNamespaceName,
    questKeyId
);

// Stamp sheet execution
yield return machine.Execute(onError);
```

The flow of the quest completion stamp sheet is as follows

![Quest Completed](QuestEnd_en.png)

The flow of the quest failure stamp sheet is as follows

![Quest Failure](QuestEnd2_en.png)

#### Delayed execution of reward distribution process

If you set up multiple resource acquisitions as rewards for completing a quest, you can use the  
The job queue ([GS2-JobQueue](https://app.gs2.io/docs/en/index.html#gs2-jobqueue)) is registered by the stamp sheet for the job to obtain the reward.  
When the client executes the job queue, the process of actually receiving the reward is executed.

Job Registration by Stamp Sheet

```c#
public UnityAction<EzStampSheet, EzRunStampSheetResult> GetSheetCompleteAction()
{
    return (sheet, sheetResult) =>
    {
        // Job registration by stamp sheet
        if (sheet.Action == "Gs2JobQueue:PushByUserId")
        {
            OnPushJob();
        }
    };
}
```

Job queue execution

```c#
AsyncResult<EzRunResult> result = null;
yield return _client.JobQueue.Run(
    r => { result = r; }
    _gameSession,
    _jobQueueNamespaceName
);
```
