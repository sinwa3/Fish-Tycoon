using UnityEngine;

/// <summary> KYH : 시야, 소리로 의심 게이지를 올리고 내려 단계를 기억에 적는다 </summary>
public class EnemySuspicionGauge : MonoBehaviour
{
    #region ===== 수치 =====

    // 의심 게이지 최대 (발각)
    private const float MaxGauge = 100.0f;
    // 발각 단계로 올라가는 값
    private const float DetectedEnterGauge = MaxGauge;
    // 소리로 오를 수 있는 최대 (발각 바로 아래)
    private const float SoundGaugeCap = DetectedEnterGauge - 1.0f;

    #endregion ===== 수치 =====

    #region ===== 상태 =====

    private EnemyController _enemy;

    /// <summary> KYH : 지금 게이지 값 </summary>
    public float Gauge { get; private set; }

    #endregion ===== 상태 =====

    private void Awake()
    {
        InitializeComponents();
    }

    private void Update()
    {
        UpdateSuspicionGauge(Time.deltaTime);
    }

    #region ===== 초기화 =====

    /// <summary> KYH : 같은 오브젝트의 컨트롤러를 캐싱한다 </summary>
    private void InitializeComponents()
    {
        _enemy = GetComponent<EnemyController>();

        if (_enemy == null)
        {
            CPrint.Warning("[EnemySuspicionGauge] 적 컨트롤러 없음");
        }
    }

    #endregion ===== 초기화 =====

    #region ===== 게이지 갱신 =====

    /// <summary> KYH : 게이지 값, 단계, 감소 막기를 차례로 갱신한다 </summary>
    private void UpdateSuspicionGauge(float deltaTime)
    {
        if (_enemy == null || _enemy.Sight == null)
        {
            return;
        }

        UpdateGaugeValue(deltaTime);
        UpdateAlertLevel();
        UpdateDecayBlock();
    }

    /// <summary> KYH : 보이면 게이지를 올리고 안 보이면 내린다 </summary>
    private void UpdateGaugeValue(float deltaTime)
    {
        if (IsSightRaisingGauge())
        {
            Gauge += GetSightRiseRate() * deltaTime;
        }
        else if (_enemy.Memory.IsGaugeDecayAllowed)
        {
            Gauge -= _enemy.Detection.GaugeDecayRate * deltaTime;
        }

        Gauge = Mathf.Clamp(Gauge, 0.0f, MaxGauge);
    }

    /// <summary> KYH : 시야로 게이지가 오르는지 돌려준다 </summary>
    private bool IsSightRaisingGauge()
    {
        return _enemy.Sight.IsPlayerVisible && !_enemy.IsSleepDay;
    }

    /// <summary> KYH : 거리에 따른 초당 상승량을 돌려준다 </summary>
    private float GetSightRiseRate()
    {
        return Mathf.Lerp(_enemy.Detection.SightGaugeRiseNear, _enemy.Detection.SightGaugeRiseFar, _enemy.Sight.GetSightDistanceRatio());
    }

    #endregion ===== 게이지 갱신 =====

    #region ===== 소리 =====

    /// <summary> KYH : 들린 세기만큼 게이지를 올린다 (발각 바로 아래까지) </summary>
    public void AddSoundGauge(float amount)
    {
        if (Gauge >= SoundGaugeCap)
        {
            return;
        }

        Gauge = Mathf.Min(Gauge + amount, SoundGaugeCap);
    }

    #endregion ===== 소리 =====

    #region ===== 단계 =====

    /// <summary> KYH : 단계를 기억에 적고 조사 단계로 오르면 조사 목표를 적는다 </summary>
    private void UpdateAlertLevel()
    {
        AlertLevel currentLevel = _enemy.Memory.AlertLevel;
        AlertLevel nextLevel = GetNextAlertLevel(currentLevel);

        if (currentLevel < AlertLevel.Alert && nextLevel >= AlertLevel.Alert)
        {
            _enemy.Memory.SetInvestigateTarget(_enemy.Memory.LastStimulusPosition);
        }

        _enemy.Memory.SetAlertLevel(nextLevel);
    }

    /// <summary> KYH : 게이지에 맞는 단계를 돌려준다 </summary>
    private AlertLevel GetNextAlertLevel(AlertLevel currentLevel)
    {
        AlertLevel nextLevel = currentLevel;

        while (nextLevel < AlertLevel.Detected && Gauge >= GetEnterGauge(nextLevel + 1))
        {
            nextLevel++;
        }

        while (nextLevel > AlertLevel.Calm && Gauge < GetExitGauge(nextLevel))
        {
            nextLevel--;
        }

        return nextLevel;
    }

    /// <summary> KYH : 이 단계로 올라가는 게이지 값을 돌려준다 </summary>
    private float GetEnterGauge(AlertLevel level)
    {
        switch (level)
        {
            case AlertLevel.Suspicious:
                return _enemy.Detection.SuspiciousEnterGauge;
            case AlertLevel.Alert:
                return _enemy.Detection.AlertEnterGauge;
            case AlertLevel.Detected:
                return DetectedEnterGauge;
            default:
                return 0.0f;
        }
    }

    /// <summary> KYH : 이 단계에서 내려가는 게이지 값을 돌려준다 </summary>
    private float GetExitGauge(AlertLevel level)
    {
        switch (level)
        {
            case AlertLevel.Suspicious:
                return _enemy.Detection.SuspiciousExitGauge;
            case AlertLevel.Alert:
                return _enemy.Detection.AlertExitGauge;
            case AlertLevel.Detected:
                return _enemy.Detection.DetectedExitGauge;
            default:
                return 0.0f;
        }
    }

    #endregion ===== 단계 =====

    #region ===== 감소 막기 =====

    /// <summary> KYH : 발각 단계에서 보이면 게이지 감소를 막는다 </summary>
    private void UpdateDecayBlock()
    {
        if (_enemy.Memory.AlertLevel == AlertLevel.Detected && _enemy.Sight.IsPlayerVisible)
        {
            _enemy.Memory.SetGaugeDecayAllowed(false);
        }
    }

    #endregion ===== 감소 막기 =====
}
