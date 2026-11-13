using UnityEngine;
using UnityEngine.UI;

/// <summary> KDH : 화면 단위 UI의 초기화, 입력 처리 및 닫기 기능을 제공하는 기본 클래스다. </summary>
[RequireComponent(typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster))]
public abstract class BaseScreen : BaseUI
{
    public override int SortingOrder => 1;

    private void Awake()
    {
        OnAwake();

        Managers.UI.SetupCanvas(this);
        Managers.UI.CloseAllPopupUI();
    }

    protected abstract void OnAwake();

    private void Start()
    {
        OnStart();
    }

    protected abstract void OnStart();

    private void Update()
    {
        OnUpdate();
    }

    protected abstract void OnUpdate();

    /// <summary> KDH : Screen에서 처리할 입력을 확인한다. </summary>
    public virtual void OnInputKey()
    {
        if (!Managers.Input.KeyDown_Esc)
        {
            return;
        }

        Close();
    }

    protected virtual void Close()
    {
        Managers.UI.CloseAll();
    }
    
    protected virtual void OnDestroy()
    {
        DestroyOverride();
    }

    protected abstract void DestroyOverride();
}