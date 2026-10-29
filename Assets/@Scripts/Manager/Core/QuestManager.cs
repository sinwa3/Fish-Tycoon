using System;

/// <summary> KDH : HUD에 퀘스트 진행도와 완료 정보를 전달한다. </summary>
public readonly struct QuestProgressEvent
{
    public SceneType Scene { get; }
    public QuestData CompletedQuest { get; }
    public QuestData NextQuest { get; }
    public QuestData CurrentQuest { get; }
    public int Progress { get; }
    public bool IsCompleted { get; }

    public QuestProgressEvent(SceneType scene, QuestData completedQuest, QuestData nextQuest, QuestData currentQuest,
        int progress, bool isCompleted)
    {
        Scene = scene;
        CompletedQuest = completedQuest;
        NextQuest = nextQuest;
        CurrentQuest = currentQuest;
        Progress = progress;
        IsCompleted = isCompleted;
    }
}

/// <summary> KDH : 현재 퀘스트의 진행, 보상, 순차 전환을 관리한다. </summary>
public class QuestManager
{
    public event Action<QuestProgressEvent> OnQuestProgressed;

    public bool IsCompletionSequence { get; private set; }

    /// <summary> KDH : 현재 진행 중인 퀘스트를 가져온다. </summary>
    public QuestData GetCurrentQuest(SceneType scene, out int progress)
    {
        progress = 0;
        
        if (Managers.Data == null)
        {
            return null;
        }

        var questState = Managers.Data.GetSubQuestState(scene);
        progress = questState.progress;
        
        if (Managers.Table == null || Managers.Table.Quest == null)
        {
            return null;
        }
        
        return Managers.Table.Quest.GetQuest(scene, questState.questID);
    }

    /// <summary> 게임 플레이에서 발생한 조건으로 현재 퀘스트를 1회 진행한다. </summary>
    public void ReportProgress(SceneType scene, QuestType questType)
    {
        if (IsCompletionSequence)
        {
            return;
        }
        
        QuestData currentQuest = GetCurrentQuest(scene, out int progress);

        // 현재 진행 중인 퀘스트가 아니면 진행하지 않는다.
        if (currentQuest == null || !currentQuest.TryGetType(out QuestType type) || questType != type)
        {
            return;
        }

        progress = Math.Min(progress + 1, currentQuest.Goal);

        if (progress < currentQuest.Goal)
        {
            Managers.Data.SetSubQuestState(scene, currentQuest.ID, progress);
            OnQuestProgressed?.Invoke(
                new QuestProgressEvent(scene, currentQuest, null, currentQuest, progress, false));

            return;
        }

        // 진행도가 목표에 도달했을 때
        QuestData nextQuest = Managers.Table.Quest.GetNextQuest(scene, currentQuest.ID);

        Managers.Data.AddGold(currentQuest.GoldReward);

        int nextQuestID = nextQuest?.ID ?? currentQuest.ID + 1;

        Managers.Data.SetSubQuestState(scene, nextQuestID, 0);

        OnQuestProgressed?.Invoke(new QuestProgressEvent(scene, currentQuest, nextQuest, nextQuest, progress, true));
    }

    /// <summary> KDH : 퀘스트 완료 연출 진행 상태를 설정한다. </summary>
    public void SetCompletionSequenceActive(bool isShowingCompletion)
    {
        IsCompletionSequence = isShowingCompletion;
    }
}