using UnityEngine;

/// <summary> KYH : 설치한 타이머 장치가 시간이 지나면 집주인을 부르는 소음을 낸다 </summary>
public class TimerNoise : MonoBehaviour
{
    // Alarm 클립 길이
    [SerializeField] private float _alarmDuration = 2.0f;

    // 울리는 애니메이션을 트는 자식 시계 모델
    private Animator _animator;
    // 울리기까지 남은 시간
    private float _remainTime;
    private bool _isRunning;

    private void Awake()
    {
        InitializeComponents();
    }

    private void Update()
    {
        UpdateTimer(Time.deltaTime);
    }

    /// <summary> KYH : 울릴 시간을 받아 타이머를 시작한다 </summary>
    public void StartTimer(float delay)
    {
        _remainTime = delay;
        _isRunning = true;
    }

    /// <summary> KYH : 자식 시계 모델의 애니메이터를 찾아 둔다 </summary>
    private void InitializeComponents()
    {
        _animator = GetComponentInChildren<Animator>(true);
    }

    /// <summary> KYH : 남은 시간을 줄이고 다 되면 소음을 내고 울린 뒤 사라진다 </summary>
    private void UpdateTimer(float deltaTime)
    {
        if (!_isRunning)
        {
            return;
        }

        _remainTime -= deltaTime;

        if (_remainTime > 0.0f)
        {
            return;
        }

        _isRunning = false;

        MakeNoise();
        PlayAlarm();
        Destroy(gameObject, _alarmDuration);
    }

    /// <summary> KYH : 울리는 애니메이션을 처음부터 튼다 </summary>
    private void PlayAlarm()
    {
        if (_animator == null)
        {
            return;
        }

        _animator.Play(AnimatorKey.Hash.Alarm);
    }

    /// <summary> KYH : 타이머 장치 위치로 집주인을 부르는 유인 소음을 낸다 </summary>
    private void MakeNoise()
    {
        Managers.Sound?.PlaySfx(SoundKey.Name.SfxType.ItemTimer);

        EnemyHearing[] hearings = FindObjectsOfType<EnemyHearing>();

        foreach (EnemyHearing hearing in hearings)
        {
            hearing.HearLure(transform.position, ConsumableTempValue.TimerNoiseLoudness);
        }
    }
}
