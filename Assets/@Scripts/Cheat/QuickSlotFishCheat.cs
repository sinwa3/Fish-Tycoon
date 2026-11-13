#if UNITY_EDITOR || DEVELOPMENT_BUILD

/// <summary> 잠입 중 퀵슬롯의 생선을 넣고 뺀다 </summary>
public class QuickSlotFishCheat : BaseFishCheat
{
    public override string Title => "생선 (잠입 퀵슬롯)";

    protected override bool IsTargetScene => IsCurrentScene<StealthScene>();

    protected override int GetCount(int id)
    {
        return CheatQuickSlot.GetCount(QuickSlotItemType.Fish, id);
    }

    /// <summary> 같은 생선 칸이나 빈 칸에 넣는다 </summary>
    protected override void Add(int id, int count)
    {
        if (!Managers.Data.TryAddFishToQuickSlot(id, count))
        {
            Managers.UI?.OpenToastMessage("퀵슬롯이 가득 찼습니다.");
        }
    }

    protected override void Remove(int id, int count)
    {
        CheatQuickSlot.Remove(QuickSlotItemType.Fish, id, count);
    }
}
#endif
