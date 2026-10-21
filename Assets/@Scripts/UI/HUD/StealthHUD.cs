using TMPro;
using UnityEngine.UI;

public class StealthHUD : BaseHUD
{
    public enum Texts
    {
        CurrentDayText,
        PhaseText
    }
    
    #region ===== 참조 =====

    private StealthScene _stealthScene;

    #endregion ===== 참조 =====
    
    protected override void OnAwake()
    {
        Bind<TMP_Text>(typeof(Texts));
        
        _stealthScene = gameObject.FindParent<StealthScene>();
    }

    protected override void OnStart()
    {
        SetCurrentDayText();
        
        SubscribePhaseChanged();
    }

    protected override void OnUpdate()
    {
    }

    public override void OnInputKey()
    {
        base.OnInputKey();

        if (Managers.Input.KeyDown_Esc)
        {
            Managers.UI.OpenPopup<PausePopup>();
        }
        else if (Managers.Input.KeyDown_E)
        {
            _stealthScene?.ChangeCurrentPhase();
        }
    }

    private void OnDestroy()
    {
        UnsubscribePhaseChanged();
    }

    #region ===== 초기화 =====
    
    /// <summary> KDH : 잠입 씬 첫 진입시 현재 날짜를 세팅한다. </summary>
    private void SetCurrentDayText()
    {
        Get<TMP_Text>(Texts.CurrentDayText).SetText($"{Managers.Data.GetCurrentDay()}일 차");
    }
    
    #endregion ===== 초기화 =====
    
    #region ===== 업데이트 =====
    
    /// <summary> KDH : 현재 상태를 UI에 업데이트 한다. </summary>
    public void UpdatePhaseText(StealthScene.Phase phase)
    {
        string message = phase switch
        {
            StealthScene.Phase.Playing => "잠입",
            StealthScene.Phase.Settlement => "정산",
            _ => string.Empty
        };

        Get<TMP_Text>(Texts.PhaseText)?.SetText(message);
    }
    
    #endregion ===== 업데이트 =====
    
    #region ===== 이벤트 =====
    
    /// <summary> KDH : 페이즈 변경되었을 때 상태 텍스트 변경을 구독 </summary>
    private void SubscribePhaseChanged()
    {
        if (_stealthScene != null)
        {
            _stealthScene.OnPhaseChanged += UpdatePhaseText;
        }
    }

    /// <summary> KDH : 페이즈 변경되었을 때 상태 텍스트 변경을 구독해제 </summary>
    private void UnsubscribePhaseChanged()
    {
        if (_stealthScene != null)
        {
            _stealthScene.OnPhaseChanged -= UpdatePhaseText;
        }
    }
    
    #endregion ===== 이벤트 =====
}
