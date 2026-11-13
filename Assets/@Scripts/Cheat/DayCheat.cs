#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Reflection;
using UnityEngine;

/// <summary> 날짜를 하루 넘기거나 되돌린다 </summary>
public class DayCheat : BaseCheat
{
    private const int FirstDay = 1;

    // 날짜를 줄이는 공개 메서드가 없어 저장 데이터를 리플렉션으로 꺼낸다
    private static readonly FieldInfo SaveDataField = typeof(DataManager).GetField("_saveData", BindingFlags.Instance | BindingFlags.NonPublic);

    public override string Title => $"날짜 ({Managers.Data?.GetCurrentDay() ?? 0}일 차)";

    public override bool IsAvailable => Managers.Data != null;

    // 잠입 중이거나 오늘 영업 정산을 끝냈으면 다음은 잠입이다 (영업 정산 때 저장 모드가 Tycoon으로 바뀐다)
    private static bool IsStealthNext => IsCurrentScene<StealthScene>() || Managers.Data.GetSavedGameMode() == GameMode.Tycoon;

    protected override void OnInitialize()
    {
        if (SaveDataField == null)
        {
            CPrint.Warning("[DayCheat] 리플렉션할 필드 없음");
        }
    }

    public override void DrawButtons()
    {
        GUILayout.BeginHorizontal();

        DrawButton("이전 날로", SetPreviousDay);
        DrawButton("다음 날로", SetNextDay);

        GUILayout.EndHorizontal();

        DrawLabel($"{FirstDay}일 차에서 {TycoonScene.FinalDay}일 차까지만 바뀐다. {TycoonScene.FinalDay}일 차는 잠입이 없어 다음이 잠입이면 넘어가지 않는다");
        DrawLabel("저장 데이터의 날짜만 바뀐다. 화면과 집주인 수치는 씬을 다시 불러야 바뀐다");
    }

    /// <summary> 날짜를 하루 넘긴다 </summary>
    private void SetNextDay()
    {
        int nextDay = Managers.Data.GetCurrentDay() + 1;

        if (nextDay > TycoonScene.FinalDay || IsStealthOnFinalDay(nextDay))
        {
            return;
        }

        Managers.Data.SetNextDay();
    }

    /// <summary> 날짜를 하루 줄인다 </summary>
    private void SetPreviousDay()
    {
        if (SaveDataField?.GetValue(Managers.Data) is not SaveData saveData)
        {
            return;
        }

        int previousDay = saveData.CurrentDay - 1;

        if (previousDay < FirstDay || IsStealthOnFinalDay(previousDay))
        {
            return;
        }

        saveData.CurrentDay = previousDay;
    }

    /// <summary> 그 날짜로 바꾸면 마지막 날에 잠입하게 되는지 돌려준다 </summary>
    private static bool IsStealthOnFinalDay(int day)
    {
        return day == TycoonScene.FinalDay && IsStealthNext;
    }
}
#endif
