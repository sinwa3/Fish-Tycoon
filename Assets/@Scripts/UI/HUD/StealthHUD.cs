using TMPro;
using UnityEngine.UI;

public class StealthHUD : BaseHUD
{
    public enum Texts
    {
        CurrentDayText,
        PhaseText,
        NextBtnText
    }

    public enum Images
    {
        QuickSlotBar
    }
    
    #region ===== 참조 =====

    private StealthScene _stealthScene;

    #endregion ===== 참조 =====
    
    protected override void OnAwake()
    {
        Bind<TMP_Text>(typeof(Texts));
        Bind<Image>(typeof(Images));
        
        _stealthScene = gameObject.FindParent<StealthScene>();
    }

    protected override void OnStart()
    {
        SetCurrentDayText();
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
        else if (Managers.Input.MouseDown_Wheel)
        {
            Managers.Input.SetCursorLock(false);
        }
        else if (Managers.Input.MouseDown_Left)
        {
            Managers.Input.SetCursorLock(true);
        }
    }

    #region ===== 초기화 =====
    
    /// <summary> KDH : 잠입 씬 첫 진입시 현재 날짜를 세팅한다. </summary>
    private void SetCurrentDayText()
    {
        Get<TMP_Text>(Texts.CurrentDayText)?.SetText($"{Managers.Data.GetCurrentDay()}일 차");
    }
    
    #endregion ===== 초기화 =====
    
    #region ===== 업데이트 =====
    
    /// <summary> KDH : 현재 상태를 UI에 업데이트 한다. </summary>
    public void UpdatePhaseText(StealthScene.Phase phase)
    {
        string phaseStr = phase switch
        {
            StealthScene.Phase.Playing => "잠입",
            StealthScene.Phase.Settlement => "정산",
            _ => string.Empty
        };

        Get<TMP_Text>(Texts.PhaseText)?.SetText(phaseStr);
        
        // NextBtnText
        string nextBtnStr = phase switch
        {
            StealthScene.Phase.Playing => "[E] 정산하기",
            StealthScene.Phase.Settlement => "[E] 다음 날로 이동",
            _ => string.Empty
        };
        
        Get<TMP_Text>(Texts.NextBtnText)?.SetText(nextBtnStr);
    }
    
    #endregion ===== 업데이트 =====
    
    #region ===== 활성화 =====

    /// <summary> KDH : 퀵슬롯을 표시/비표시한다. </summary>
    public void SetQuickSlotVisible(bool isVisible)
    {
        Get<Image>(Images.QuickSlotBar)?.gameObject.SetActive(isVisible);
    }
    
    #endregion ===== 활성화 =====
}
