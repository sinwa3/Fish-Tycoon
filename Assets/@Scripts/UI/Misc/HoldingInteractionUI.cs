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

    /// <summary> KDH : 현재 상호작용 타입에 따라 대상 아이콘과 안내 문구를 갱신한다. </summary>
    private void UpdateTargetUI()
    {
        InteractionType type = _holdingSystem != null ? _holdingSystem.CurrentType : InteractionType.None;
        bool visible = type != InteractionType.None;
        
        // 상호작용 대상이 존재하는 경우에만 UI 표시
        Get<Image>(Images.Bg)?.gameObject.SetActive(visible);
        
        // 현재 홀딩 진행률 반영
        UpdateProgressUI(_holdingSystem != null ? _holdingSystem.Progress : 0f);
        
        if (!visible)
        {
            return;
        }

        // 상호작용 타입에 맞는 아이콘 표시
        bool wardrobe = type is InteractionType.Wardrobe or InteractionType.WardrobeExit;
        Get<Image>(Images.WardrobeIcon)?.gameObject.SetActive(wardrobe);
        Get<Image>(Images.WardrobeArrowIcon)?.gameObject.SetActive(type == InteractionType.WardrobeExit);
        Get<Image>(Images.DoorIcon)?.gameObject.SetActive(type is InteractionType.Door or InteractionType.ExitDoor);
        Get<Image>(Images.FishIcon)?.gameObject.SetActive(type == InteractionType.Fish);
        Get<Image>(Images.InteractionIcon)?.gameObject.SetActive(type == InteractionType.Furniture);
        Get<Image>(Images.LockIcon)?.gameObject.SetActive(type == InteractionType.Lock);
        
        // 생선 아이콘이 전달된 경우에만 이미지 교체
        if (type == InteractionType.Fish)
        {
            Image fishIcon = Get<Image>(Images.FishIcon);
            if (fishIcon != null && _holdingSystem.Icon != null)
            {
                fishIcon.sprite = _holdingSystem.Icon;
            }
        }

        // 현재 상호작용 안내 문구 표시
        Get<TMP_Text>(Texts.ActionText)?.SetText(_holdingSystem.ActionText);
    }

    /// <summary> KDH : 홀딩 진행률을 이미지의 Fill Amount에 반영한다. </summary>
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
    
    /// <summary> KDH : 상호작용 대상 및 홀딩 진행률 변경 이벤트를 구독한다. </summary>
    private void SubscribeEvents()
    {
        if (_holdingSystem == null)
        {
            return;
        }

        // 중복 구독 방지
        UnsubscribeEvents();
        
        _holdingSystem.OnTargetChanged += UpdateTargetUI;
        _holdingSystem.OnProgressChanged += UpdateProgressUI;
    }

    /// <summary> KDH : 홀딩 시스템 이벤트 구독을 해제한다. </summary>
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
