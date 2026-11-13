#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;

/// <summary> 골드를 더한다 </summary>
public class GoldCheat : BaseCheat
{
    private static readonly int[] AddAmounts = { 100, 1000, 10000 };

    public override string Title => $"골드 ({Managers.Data?.GetCurrentGold() ?? 0})";

    public override bool IsAvailable => Managers.Data != null;

    public override void DrawButtons()
    {
        GUILayout.BeginHorizontal();

        foreach (int amount in AddAmounts)
        {
            DrawButton($"+{amount:N0}", () => Managers.Data?.AddGold(amount));
        }

        GUILayout.EndHorizontal();
    }
}
#endif
