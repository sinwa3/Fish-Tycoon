using UnityEngine;

/// <summary> KYH : 소음을 받아 기억에 적고 의심 게이지를 올린다 </summary>
public class EnemyHearing : MonoBehaviour
{
    #region ===== 수치 =====

    // 소리 위치를 바닥에서 띄우는 높이 (바닥에 레이가 맞지 않게)
    private const float SoundSourceHeight = 0.1f;

    #endregion ===== 수치 =====

    #region ===== 상태 =====

    private EnemyController _enemy;
    private StealthScene _stealthScene;

    #endregion ===== 상태 =====

    private void Awake()
    {
        InitializeComponents();
    }

    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    #region ===== 초기화 =====

    /// <summary> KYH : 같은 오브젝트의 컨트롤러를 캐싱한다 </summary>
    private void InitializeComponents()
    {
        _enemy = GetComponent<EnemyController>();

        if (_enemy == null)
        {
            CPrint.Warning("[EnemyHearing] 적 컨트롤러 없음");
        }
    }

    #endregion ===== 초기화 =====

    #region ===== 이벤트 =====

    /// <summary> KYH : 잠입 씬의 소음 이벤트를 구독한다 </summary>
    private void SubscribeEvents()
    {
        _stealthScene = Managers.Scene.CurrentScene as StealthScene;

        if (_stealthScene == null)
        {
            return;
        }

        _stealthScene.OnNoiseMade += HearNoise;
    }

    /// <summary> KYH : 소음 이벤트 구독을 해제한다 </summary>
    private void UnsubscribeEvents()
    {
        if (_stealthScene == null)
        {
            return;
        }

        _stealthScene.OnNoiseMade -= HearNoise;
        _stealthScene = null;
    }

    #endregion ===== 이벤트 =====

    #region ===== 소음 받기 =====

    /// <summary> KYH : 들린 세기가 기준값 이상이면 기억에 적고 게이지를 올린다 </summary>
    public void HearNoise(Vector3 position, float loudness)
    {
        if (!TryHear(position, loudness, out float heardLoudness))
        {
            return;
        }

        _enemy.SuspicionGauge.AddSoundGauge(heardLoudness * _enemy.Detection.SoundGaugePerLoudness);
    }

    /// <summary> KYH : 유인 소음이 들리면 게이지를 조사 시작 값까지 올리고 그 자리를 조사 목표로 잡는다 </summary>
    public void HearLure(Vector3 position, float loudness)
    {
        if (!TryHear(position, loudness, out _))
        {
            return;
        }

        _enemy.SuspicionGauge.RaiseToAlertGauge();
        _enemy.Memory.SetInvestigateTarget(position);
    }

    /// <summary> KYH : 소음이 들렸으면 기억에 적고 true, 들린 세기는 heardLoudness로 돌려준다 </summary>
    private bool TryHear(Vector3 position, float loudness, out float heardLoudness)
    {
        heardLoudness = 0.0f;

        if (IsIgnoringNoise())
        {
            return false;
        }

        heardLoudness = GetHeardLoudness(position, loudness);

        if (heardLoudness < _enemy.Detection.SoundHearThreshold)
        {
            return false;
        }

        ReportToMemory(position, heardLoudness);

        return true;
    }

    /// <summary> KYH : 소음을 무시하는지 돌려준다 (수면하는 날, 추격 중) </summary>
    private bool IsIgnoringNoise()
    {
        if (_enemy == null)
        {
            return true;
        }

        if (_enemy.IsSleepDay)
        {
            return true;
        }

        if (_enemy.Memory.AlertLevel == AlertLevel.Detected && !_enemy.Memory.IsGaugeDecayAllowed)
        {
            return true;
        }

        return false;
    }

    #endregion ===== 소음 받기 =====

    #region ===== 들린 세기 =====

    /// <summary> KYH : 거리, 벽, 문 감쇠를 곱한 들린 세기를 돌려준다 </summary>
    private float GetHeardLoudness(Vector3 position, float loudness)
    {
        Vector3 soundPosition = GetSoundSourcePoint(position);
        Vector3 earPosition = GetEarPosition();

        float distanceRatio = GetDistanceFalloff(Vector3.Distance(soundPosition, earPosition), loudness);

        int doorCount = CountBlockers(soundPosition, earPosition, LayerKey.Mask.Door);
        // 층 사이 바닥도 Wall 레이어다
        int wallCount = CountBlockers(soundPosition, earPosition, LayerKey.Mask.Wall);

        float doorRatio = Mathf.Pow(_enemy.Detection.DoorSoundMultiplier, doorCount);
        float wallRatio = Mathf.Pow(_enemy.Detection.WallSoundMultiplier, wallCount);

        return loudness * distanceRatio * wallRatio * doorRatio;
    }

    /// <summary> KYH : 거리에 따른 감쇠를 돌려준다 (0 ~ 1) </summary>
    private float GetDistanceFalloff(float distance, float loudness)
    {
        if (loudness <= 0.0f)
        {
            return 0.0f;
        }

        float radius = loudness * _enemy.Detection.SoundRangePerLoudness;

        return Mathf.Clamp01(1.0f - distance / radius);
    }

    /// <summary> KYH : 두 점 사이에서 이 레이어 콜라이더를 몇 개 지나는지 돌려준다 </summary>
    private int CountBlockers(Vector3 from, Vector3 to, int layers)
    {
        Vector3 direction = to - from;
        RaycastHit[] hits = Physics.RaycastAll(from, direction, direction.magnitude, layers, QueryTriggerInteraction.Ignore);

        return hits.Length;
    }

    /// <summary> KYH : 소리가 나는 점을 돌려준다 </summary>
    private Vector3 GetSoundSourcePoint(Vector3 position)
    {
        return position + Vector3.up * SoundSourceHeight;
    }

    /// <summary> KYH : 집주인 귀 위치를 돌려준다 </summary>
    private Vector3 GetEarPosition()
    {
        return transform.position + Vector3.up * EnemySight.EyeHeight;
    }

    #endregion ===== 들린 세기 =====

    #region ===== 기억 =====

    /// <summary> KYH : 들은 위치와 세기를 기억에 적는다 </summary>
    private void ReportToMemory(Vector3 position, float heardLoudness)
    {
        _enemy.Memory.ReportStimulus(position, heardLoudness, StimulusType.Sound);
    }

    #endregion ===== 기억 =====
}
