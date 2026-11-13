#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;

/// <summary> 생선 테이블에서 골라 넣고 빼는 치트의 기본 클래스 (넣고 빼는 곳은 상속한 클래스가 정한다) </summary>
public abstract class BaseFishCheat : BaseCountCheat
{
    public override bool IsAvailable => Managers.Data != null && Managers.Table?.Fish != null && IsTargetScene;

    /// <summary> 이 치트를 쓰는 씬인지 </summary>
    protected abstract bool IsTargetScene { get; }

    protected override void CollectIDs(List<int> ids)
    {
        ids.AddRange(Managers.Table.Fish.Map.Keys);
    }

    protected override string GetName(int id)
    {
        FishData fish = Managers.Table.Fish.GetFish(id);

        return fish != null ? $"{id} {fish.Name}" : id.ToString();
    }
}
#endif
