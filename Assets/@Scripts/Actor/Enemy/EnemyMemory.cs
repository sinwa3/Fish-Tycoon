using UnityEngine;

/// <summary> KYH : 감지 결과를 담는 기억. 우선순위 함수와 상태가 읽는다 </summary>
public class EnemyMemory
{
    #region ===== 게이지 단계 =====

    /// <summary> KYH : 의심 게이지 단계 </summary>
    public AlertLevel AlertLevel { get; private set; }

    /// <summary> KYH : 의심 게이지 단계를 바꾼다 </summary>
    public void SetAlertLevel(AlertLevel level)
    {
        AlertLevel = level;
    }

    #endregion ===== 게이지 단계 =====

    #region ===== 마지막 자극 =====

    /// <summary> KYH : 마지막 자극 위치 </summary>
    public Vector3 LastStimulusPosition { get; private set; }

    /// <summary> KYH : 마지막 자극 세기 </summary>
    public float LastStimulusStrength { get; private set; }

    /// <summary> KYH : 마지막 자극 시각 </summary>
    public float LastStimulusTime { get; private set; }

    /// <summary> KYH : 마지막 자극 종류 </summary>
    public StimulusType LastStimulusType { get; private set; }

    /// <summary> KYH : 새 자극을 기록한다 </summary>
    public void ReportStimulus(Vector3 position, float strength, StimulusType type)
    {
        LastStimulusPosition = position;
        LastStimulusStrength = strength;
        LastStimulusTime = Time.time;
        LastStimulusType = type;
    }

    #endregion ===== 마지막 자극 =====

    #region ===== 마지막 목격 =====

    /// <summary> KYH : 플레이어를 마지막으로 본 위치 </summary>
    public Vector3 LastSeenPosition { get; private set; }

    /// <summary> KYH : 플레이어를 마지막으로 본 시각 </summary>
    public float LastSeenTime { get; private set; }

    /// <summary> KYH : 플레이어를 본 위치와 시각을 기록한다 </summary>
    public void ReportPlayerSeen(Vector3 position)
    {
        LastSeenPosition = position;
        LastSeenTime = Time.time;
    }

    #endregion ===== 마지막 목격 =====

    #region ===== 조사 목표 =====

    /// <summary> KYH : 조사할 곳이 있는지 </summary>
    public bool HasInvestigateTarget { get; private set; }

    /// <summary> KYH : 조사할 위치 </summary>
    public Vector3 InvestigateTarget { get; private set; }

    /// <summary> KYH : 조사할 위치를 기록한다 </summary>
    public void SetInvestigateTarget(Vector3 position)
    {
        InvestigateTarget = position;
        HasInvestigateTarget = true;
    }

    /// <summary> KYH : 조사 목표를 지운다 </summary>
    public void ClearInvestigateTarget()
    {
        InvestigateTarget = Vector3.zero;
        HasInvestigateTarget = false;
    }

    #endregion ===== 조사 목표 =====

    #region ===== 게이지 감소 허락 =====

    /// <summary> KYH : 의심 게이지가 줄어들 수 있는지 </summary>
    public bool IsGaugeDecayAllowed { get; private set; } = true;

    /// <summary> KYH : 의심 게이지 감소를 허락하거나 막는다 </summary>
    public void SetGaugeDecayAllowed(bool isAllowed)
    {
        IsGaugeDecayAllowed = isAllowed;
    }

    #endregion ===== 게이지 감소 허락 =====
}
