#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;

/// <summary> 게임 속도를 바꾼다 (창을 닫을 때 적용) </summary>
public class SpeedCheat : BaseCheat
{
    private static readonly float[] TimeScales = { 0.5f, 1.0f, 2.0f, 4.0f };

    public override string Title => $"속도 (x{Window.ResumeTimeScale})";

    public override void DrawButtons()
    {
        GUILayout.BeginHorizontal();

        foreach (float timeScale in TimeScales)
        {
            DrawButton($"x{timeScale}", () => Window.SetResumeTimeScale(timeScale));
        }

        GUILayout.EndHorizontal();
    }
}
#endif
