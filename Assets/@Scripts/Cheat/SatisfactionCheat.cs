#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;

/// <summary> 만족도를 올리거나 내린다 </summary>
public class SatisfactionCheat : BaseCheat
{
    private static readonly int[] Amounts = { 10, 100 };

    public override string Title => $"만족도 ({Managers.Data?.GetSatisfaction() ?? 0})";

    public override bool IsAvailable => Managers.Data != null;

    public override void DrawButtons()
    {
        GUILayout.BeginHorizontal();

        foreach (int amount in Amounts)
        {
            DrawButton($"+{amount}", () => Managers.Data?.AddSatisfaction(amount));
        }

        foreach (int amount in Amounts)
        {
            DrawButton($"-{amount}", () => Managers.Data?.SubtractSatisfaction(amount));
        }

        GUILayout.EndHorizontal();
        DrawLabel("최고 평판 등급이면 더 오르지 않는다");
    }
}
#endif
