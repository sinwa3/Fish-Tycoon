using UnityEngine;

/// <summary> KYH : 설치한 타이머 장치가 시간이 지나면 집주인을 부르는 소음을 낸다 </summary>
public class TimerNoise : MonoBehaviour
{
    // 울리기까지 남은 시간
    private float _remainTime;
    private bool _isRunning;

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

    /// <summary> KYH : 남은 시간을 줄이고 다 되면 소음을 내고 사라진다 </summary>
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
        Destroy(gameObject);
    }

    /// <summary> KYH : 타이머 장치 위치로 집주인을 부르는 유인 소음을 낸다 </summary>
    private void MakeNoise()
    {
        EnemyHearing[] hearings = FindObjectsOfType<EnemyHearing>();

        foreach (EnemyHearing hearing in hearings)
        {
            hearing.HearLure(transform.position, ConsumableTempValue.TimerNoiseLoudness);
        }
    }
}
