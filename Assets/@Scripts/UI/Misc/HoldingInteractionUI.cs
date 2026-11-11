using TMPro;
using UnityEngine.UI;

/// <summary> KDH : 홀딩 시스템 이벤트를 받아 대상 아이콘과 시계방향 진행 게이지를 표시한다. </summary>
public class HoldingInteractionUI : BaseUI
{
    private enum Images
    {
        Bg,
        
        HoldingProgressFill,
        
        DoorIcon,
        InteractionIcon,
        FishIcon,
        WardrobeIcon,
        WardrobeArrowIcon,
        LockIcon,
    }

    private enum Texts
    {
        ActionText,
    }
    
    #region ===== 참조 =====

    private HoldingSystem _holdingSystem;

    #endregion ===== 참조 =====

    private void Awake()
    {
        Bind<Image>(typeof(Images));
        Bind<TMP_Text>(typeof(Texts));
    }

    private void OnEnable()
    {
        SubscribeEvents();
        UpdateTargetUI();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    #region ===== 초기화 =====

    /// <summary> KDH : 홀딩 시스템을 HUD에 연결한다. </summary>
    public void Bind(HoldingSystem holdingSystem)
    {
        _holdingSystem = holdingSystem;

        if (isActiveAndEnabled)
        {
            SubscribeEvents();
        }

        UpdateTargetUI();
    }
    
    #endregion ===== 초기화 =====

    #region ===== 업데이트 =====

    private void UpdateTargetUI()
    {
        InteractionType type = _holdingSystem != null ? _holdingSystem.CurrentType : InteractionType.None;
        bool visible = type != InteractionType.None;
        
        Get<Image>(Images.Bg)?.gameObject.SetActive(visible);
        
        UpdateProgressUI(_holdingSystem != null ? _holdingSystem.Progress : 0f);
        
        if (!visible)
        {
            return;
        }

        bool wardrobe = type is InteractionType.Wardrobe or InteractionType.WardrobeExit;
        Get<Image>(Images.WardrobeIcon)?.gameObject.SetActive(wardrobe);
        Get<Image>(Images.WardrobeArrowIcon)?.gameObject.SetActive(type == InteractionType.WardrobeExit);
        Get<Image>(Images.DoorIcon)?.gameObject.SetActive(type == InteractionType.Door);
        Get<Image>(Images.FishIcon)?.gameObject.SetActive(type == InteractionType.Fish);
        Get<Image>(Images.InteractionIcon)?.gameObject.SetActive(type == InteractionType.Furniture);
        Get<Image>(Images.LockIcon)?.gameObject.SetActive(type == InteractionType.Lock);
        if (type == InteractionType.Fish)
        {
            Image fishIcon = Get<Image>(Images.FishIcon);
            // YSW : 아이콘을 넘기지 않으면(획득 전 생선 정보 숨김) 프리팹의 기본 생선 아이콘을 유지한다.
            if (fishIcon != null && _holdingSystem.Icon != null)
            {
                fishIcon.sprite = _holdingSystem.Icon;
            }
        }

        Get<TMP_Text>(Texts.ActionText)?.SetText(_holdingSystem.ActionText);
    }

    private void UpdateProgressUI(float progress)
    {
        Image progressFill = Get<Image>(Images.HoldingProgressFill);
        if (progressFill == null)
        {
            return;
        }

        progressFill.fillAmount = progress;
    }

    #endregion ===== 업데이트 =====
    
    #region ===== 이벤트 =====
    
    private void SubscribeEvents()
    {
        if (_holdingSystem == null)
        {
            return;
        }

        UnsubscribeEvents();
        _holdingSystem.OnTargetChanged += UpdateTargetUI;
        _holdingSystem.OnProgressChanged += UpdateProgressUI;
    }

    private void UnsubscribeEvents()
    {
        if (_holdingSystem == null)
        {
            return;
        }

        _holdingSystem.OnTargetChanged -= UpdateTargetUI;
        _holdingSystem.OnProgressChanged -= UpdateProgressUI;
    }
    
    #endregion ===== 이벤트 =====
}
