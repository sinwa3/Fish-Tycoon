using System.Collections.Generic;

public enum QuestType
{
    Serving,
    LockCursor,
    UnLockCursor,
    PurchaseItem,
}

/// <summary> KDH : 테이블 1행의 칼럼명과 일치확인, 그 값에 맞는 자료형 수정 필수 </summary>
public class QuestData : TableData
{
    [CsvField("씬")]
    public string SceneName;

    [CsvField("유형")]
    public string TypeName;

    [CsvField("목표")]
    public int Goal;

    [CsvField("보상")]
    public int GoldReward;

    [CsvField("내용")]
    public string Description;
    
    public bool TryGetType(out QuestType questType)
    {
        
        return Utils.TryParseEnum(TypeName, out questType);
    }

    public bool TryGetScene(out SceneType questScene)
    {
        return Utils.TryParseEnum(SceneName, out questScene);
    }
}

/// <summary> KDH : ID 순서로 진행하는 서브 퀘스트 테이블이다. </summary>
public class QuestTable : TableLoader<QuestData>
{
    protected override string TableName => "QuestTable";

    private Dictionary<SceneType, Dictionary<int, QuestData>> _questsByScene = new();
    
    protected override void OnLoaded()
    {
        base.OnLoaded();

        _questsByScene.Clear();

        foreach (KeyValuePair<int, QuestData> pair in TableMap)
        {
            if (!pair.Value.TryGetScene(out SceneType scene))
            {
                continue;
            }
            
            if (!_questsByScene.TryGetValue(scene, out Dictionary<int, QuestData> quests))
            {
                quests = new Dictionary<int, QuestData>();
                _questsByScene.Add(scene, quests);
            }
            
            quests.Add(pair.Key, pair.Value);
        }
    }

    /// <summary> KDH : 씬과 ID에 해당하는 퀘스트를 가져온다. </summary>
    public QuestData GetQuest(SceneType scene, int id)
    {
        if (!_questsByScene.TryGetValue(scene, out Dictionary<int, QuestData> quests))
        {
            return null;
        }

        // id가 0인경우 맨 첫 퀘스트를 가져온다.
        return id == 0 ? GetNextQuest(scene, 0) : quests.GetValueOrDefault(id);
    }

    /// <summary> KDH : 현재 ID보다 큰 퀘스트 중 가장 가까운 다음 퀘스트를 가져온다. </summary>
    public QuestData GetNextQuest(SceneType scene, int id)
    {
        if (!_questsByScene.TryGetValue(scene, out Dictionary<int, QuestData> quests))
        {
            return null;
        }

        QuestData nextQuest = null;
        int nextID = int.MaxValue;

        foreach (KeyValuePair<int, QuestData> pair in quests)
        {
            if (pair.Key <= id || pair.Key >= nextID)
            {
                continue;
            }

            nextID = pair.Key;
            nextQuest = pair.Value;
        }

        return nextQuest;
    }
}