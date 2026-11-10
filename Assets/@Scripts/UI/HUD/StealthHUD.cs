using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StealthHUD : BaseHUD
{
    public enum Texts
    {
        CurrentDayText,

        ControlGuideText,
        
        StaminaValueText,
    }

    public enum Images
    {
        StaminaFillImage,
        
        QuickSlotBar,
    }

    #region ===== 참조 =====

    private StealthScene _stealthScene;

    private QuickSlotBar _quickSlotBar;

    private ReputationTooltip _reputationTooltip;
    
    #endregion ===== 참조 =====

    #region ===== 상태 =====

    private bool _isCursorGuideLocked;
    
    #endregion ===== 상태 =====
    
    #region ===== 애니메이션 =====

    // 퀘스트
    private Coroutine _subQuestCoroutine;

    #endregion ===== 애니메이션 =====

    protected override void OnAwake()
    {
        Bind<TMP_Text>(typeof(Texts));
        Bind<Image>(typeof(Images));

        _quickSlotBar = Get<Image>(Images.QuickSlotBar)?.GetComponent<QuickSlotBar>();
        _quickSlotBar?.SetSelectionEnabled(true);

        _stealthScene = gameObject.FindParent<StealthScene>();
        
        InitializeReputationTooltip();
    }

    protected override void OnStart()
    {
        _isCursorGuideLocked = Cursor.lockState == CursorLockMode.Locked;
        
        SetCurrentDayText();
        
        SubscribeStaminaChanged();
    }

    protected override void OnUpdate()
    {
        UpdateControlGuideText();
    }

    public override void OnInputKey()
    {
        base.OnInputKey();

        if (Managers.Input.KeyDown_Esc)
        {
            Managers.UI.OpenPopup<PausePopup>();
        }
        else if (Managers.Input.MouseDown_Wheel && Cursor.lockState == CursorLockMode.Locked)
        {
            // 커서 잠금 해제
            Managers.Input.SetCursorLock(false);
            _isCursorGuideLocked = false;
                
            Managers.Quest.ReportProgress(SceneType.StealthScene, QuestType.UnLockCursor);
        }
        else if (Managers.Input.MouseDown_Left && Cursor.lockState == CursorLockMode.None)
        {
            // 커서 잠금
            Managers.Input.SetCursorLock(true);
            _isCursorGuideLocked = true;
            
            Managers.Quest.ReportProgress(SceneType.StealthScene, QuestType.LockCursor);
        }
        else
        {
            _quickSlotBar?.OnInputKey();
        }
    }

    private void OnDestroy()
    {
        UnsubscribeStaminaChanged();
    }

    #region ===== 초기화 =====

    /// <summary> KDH : 잠입 씬 첫 진입시 현재 날짜를 세팅한다. </summary>
    private void SetCurrentDayText()
    {
        Get<TMP_Text>(Texts.CurrentDayText)?.SetText($"{Managers.Data.GetCurrentDay()}");
    }

    /// <summary> KDH : 평판 툴팁을 초기화한다. </summary>
    private void InitializeReputationTooltip()
    {
        if (_reputationTooltip == null)
        {
            _reputationTooltip = Managers.Resource.Spawn(ResourceKey.Path.TooltipUI + nameof(ReputationTooltip),
                transform).gameObject.GetOrAddComponent<ReputationTooltip>();
        }

        _reputationTooltip.gameObject.SetActive(false);
    }
    
    #endregion ===== 초기화 =====

    #region ===== 업데이트 =====

    /// <summary> KDH : 커서 잠금 상태에 맞는 조작법을 표시한다. </summary>
    private void UpdateControlGuideText()
    {
        TMP_Text controlGuideText = Get<TMP_Text>(Texts.ControlGuideText);

        if (_stealthScene == null || controlGuideText == null)
        {
            return;
        }
        
        string crouchInstruction = _stealthScene.PlayerMovement != null
            ? (_stealthScene.PlayerMovement.IsCrouching ? "[Ctrl] 웅크리기 해제" : "[Ctrl] 웅크리기")
            : "[Ctrl] 웅크리기";
        string cursorInstruction = _isCursorGuideLocked ? "[휠 클릭] 커서 락 해제" : "[좌클릭] 커서 락";

        controlGuideText.SetText($"[W A S D] 이동" +
                                 $"\n" +
                                 $"\n[Left Shift] 달리기" +
                                 $"\n{crouchInstruction}" +
                                 $"\n" +
                                 $"\n{cursorInstruction}" +
                                 $"\n[숫자키 / 휠] 퀵슬롯 선택");
    }
    
    /// <summary> KDH : 플레이어 스태미너를 게이지와 수치에 반영한다. </summary>
    private void UpdateStaminaUI(float currentStamina, float maxStamina)
    {
        Image staminaFillImage = Get<Image>(Images.StaminaFillImage);
        
        if (staminaFillImage != null)
        {
            staminaFillImage.fillAmount = maxStamina > 0f ? Mathf.Clamp01(currentStamina / maxStamina) : 0f;
        }

        Get<TMP_Text>(Texts.StaminaValueText)?.SetText($"{currentStamina:0} / {maxStamina:0}");
    }
    
    #endregion ===== 업데이트 =====

    #region ===== 활성화 =====

    /// <summary> KDH : 퀵슬롯을 표시/비표시한다. </summary>
    public void SetQuickSlotVisible(bool isVisible)
    {
        Get<Image>(Images.QuickSlotBar)?.gameObject.SetActive(isVisible);
    }

    #endregion ===== 활성화 =====

    #region ===== 이벤트 =====

    /// <summary> KDH : 플레이어 스태미너 변경 이벤트를 구독한다. </summary>
    private void SubscribeStaminaChanged()
    {
        if (_stealthScene?.PlayerStamina != null)
        {
            _stealthScene.PlayerStamina.OnStaminaChanged += UpdateStaminaUI;
        }
    }

    /// <summary> KDH : 플레이어 스태미너 변경 이벤트 구독을 해제한다. </summary>
    private void UnsubscribeStaminaChanged()
    {
        if (_stealthScene?.PlayerStamina != null)
        {
            _stealthScene.PlayerStamina.OnStaminaChanged -= UpdateStaminaUI;
        }
    }
    
    #endregion ===== 이벤트 =====
}
