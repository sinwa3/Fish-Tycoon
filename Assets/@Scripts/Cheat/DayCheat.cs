#if UNITY_EDITOR || DEVELOPMENT_BUILD

/// <summary> 날짜를 하루 넘긴다 </summary>
public class DayCheat : BaseCheat
{
    public override string Title => $"날짜 ({Managers.Data?.GetCurrentDay() ?? 0}일 차)";

    public override bool IsAvailable => Managers.Data != null;

    public override void DrawButtons()
    {
        DrawButton("다음 날로", () => Managers.Data?.SetNextDay());
        DrawLabel("저장 데이터의 날짜만 바뀐다. 화면과 집주인 수치는 씬을 다시 불러야 바뀐다");
    }
}
#endif
