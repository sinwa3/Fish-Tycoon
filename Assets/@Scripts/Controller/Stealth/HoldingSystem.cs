using System;
using UnityEngine;

/// <summary> KDH : 홀딩 UI에 표시할 상호작용 종류. </summary>
public enum InteractionType
{
    None,
    Door,
    Furniture,
    Wardrobe,
    WardrobeExit,
    Fish,
}

/// <summary> KDH : PlayerInteractor가 전달한 표시 데이터와 진행률을 HUD 이벤트로 전달한다. </summary>
public class HoldingSystem : MonoBehaviour
{
    #region ===== 상태 =====

    public InteractionType CurrentType { get; private set; }
    public string ActionText { get; private set; }
    public Sprite Icon { get; private set; }
    
    public float Progress { get; private set; }
    
    public event Action OnTargetChanged;
    public event Action<float> OnProgressChanged;

    #endregion ===== 상태 =====

    /// <summary> KDH : 감지한 대상의 종류, 안내 문구와 선택 아이콘을 UI에 전달한다. </summary>
    public void SetTarget(InteractionType type, string actionText, Sprite icon = null)
    {
        if (CurrentType == type && ActionText == actionText && Icon == icon)
        {
            return;
        }

        CurrentType = type;
        ActionText = actionText;
        Icon = icon;
        OnTargetChanged?.Invoke();
    }

    /// <summary> KDH : 입력 담당자가 계산한 0~1 진행률을 전달한다. </summary>
    public void SetProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);
        if (Mathf.Approximately(Progress, progress))
        {
            return;
        }

        Progress = progress;
        OnProgressChanged?.Invoke(progress);
    }

    /// <summary> KDH : 대상 상실 또는 입력 차단 시 HUD를 숨기고 진행률을 초기화한다. </summary>
    public void ClearTarget()
    {
        SetProgress(0f);
        SetTarget(InteractionType.None, string.Empty);
    }

    private void OnDisable()
    {
        ClearTarget();
    }
}
