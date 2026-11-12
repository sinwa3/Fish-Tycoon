#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;

/// <summary> 로딩 씬을 거쳐 다른 씬으로 간다 </summary>
public class SceneCheat : BaseCheat
{
    private static readonly (string Name, SceneType Type)[] Scenes =
    {
        ("타이틀", SceneType.TitleScene),
        ("가게", SceneType.TycoonScene),
        ("잠입", SceneType.StealthScene),
    };

    public override string Title => "씬 이동";

    public override bool IsAvailable => Managers.Scene?.CurrentScene != null;

    public override void DrawButtons()
    {
        GUILayout.BeginHorizontal();

        foreach ((string name, SceneType type) in Scenes)
        {
            DrawButton(name, () => LoadScene(type));
        }

        GUILayout.EndHorizontal();
    }

    /// <summary> 멈춘 시간을 되돌린 뒤 씬을 바꾼다 </summary>
    private void LoadScene(SceneType type)
    {
        Window.Close();
        Managers.Scene.LoadSceneWithLoading(type);
    }
}
#endif
