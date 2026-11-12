#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Reflection;
using UnityEngine;

/// <summary> 치트 창에 들어가는 치트 하나의 기본 클래스 </summary>
public abstract class BaseCheat
{
    // 씬 페이즈는 공개되어 있지 않아 리플렉션으로 읽는다
    private const string PhaseFieldName = "_currentPhase";

    private static readonly FieldInfo TycoonPhaseField = typeof(TycoonScene).GetField(PhaseFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo StealthPhaseField = typeof(StealthScene).GetField(PhaseFieldName, BindingFlags.Instance | BindingFlags.NonPublic);

    /// <summary> 페이즈를 읽을 수 있는지 (팀 코드에서 필드 이름이 바뀌면 false) </summary>
    protected static bool CanReadPhase => TycoonPhaseField != null && StealthPhaseField != null;

    // 버튼을 누른 GUI 이벤트 안에서 바로 실행하면 같은 프레임의 다음 GUI 이벤트와 버튼 수가 달라져 오류가 나서 다음 Update에 실행한다
    private Action _pendingAction;

    protected CheatWindow Window { get; private set; }

    /// <summary> 치트 창에 보일 제목 </summary>
    public abstract string Title { get; }

    /// <summary> 지금 씬에서 쓸 수 있는지 (못 쓰면 창에 안 보인다) </summary>
    public virtual bool IsAvailable => true;

    /// <summary> 치트 창을 받아 둔다 </summary>
    public void Initialize(CheatWindow window)
    {
        Window = window;
        OnInitialize();
    }

    /// <summary> 눌러 둔 버튼의 동작을 실행한다 </summary>
    public void RunPendingAction()
    {
        Action action = _pendingAction;
        _pendingAction = null;

        action?.Invoke();
    }

    /// <summary> 치트 창 안에 버튼을 그린다 </summary>
    public abstract void DrawButtons();

    /// <summary> 창이 닫혀 있어도 화면에 그릴 것을 그린다 </summary>
    public virtual void DrawOverlay()
    {
    }

    public virtual void OnUpdate()
    {
    }

    public virtual void OnLateUpdate()
    {
    }

    protected virtual void OnInitialize()
    {
    }

    /// <summary> 버튼을 그리고 누르면 동작을 다음 Update에 실행한다 </summary>
    protected void DrawButton(string text, Action action)
    {
        if (GUILayout.Button(text, Window.ButtonStyle))
        {
            _pendingAction = action;
        }
    }

    /// <summary> 설명 글을 그린다 </summary>
    protected void DrawLabel(string text)
    {
        GUILayout.Label(text, Window.LabelStyle);
    }

    /// <summary> 지금 씬이 T 씬인지 돌려준다 </summary>
    protected static bool IsCurrentScene<T>() where T : BaseScene
    {
        return Managers.Scene?.CurrentScene is T;
    }

    /// <summary> 가게 씬이 그 페이즈인지 돌려준다 </summary>
    protected static bool IsTycoonPhase(TycoonScene.Phase phase)
    {
        if (TycoonPhaseField == null || Managers.Scene?.CurrentScene is not TycoonScene scene)
        {
            return false;
        }

        return (TycoonScene.Phase)TycoonPhaseField.GetValue(scene) == phase;
    }

    /// <summary> 잠입 씬이 그 페이즈인지 돌려준다 </summary>
    protected static bool IsStealthPhase(StealthScene.Phase phase)
    {
        if (StealthPhaseField == null || Managers.Scene?.CurrentScene is not StealthScene scene)
        {
            return false;
        }

        return (StealthScene.Phase)StealthPhaseField.GetValue(scene) == phase;
    }
}
#endif
