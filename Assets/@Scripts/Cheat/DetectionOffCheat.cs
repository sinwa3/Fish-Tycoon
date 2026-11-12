#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;

/// <summary> 집주인이 플레이어를 보거나 소리를 듣지 못하게 한다 </summary>
public class DetectionOffCheat : BaseCheat
{
    // 집주인이 읽는 오늘 감지 수치를 바꿨다가 끄면 되돌린다
    private EnemyController _enemy;
    private EnemyDetectionData _changedDetection;
    private float _originalSightDistance;
    private float _originalHearThreshold;

    private bool _isSightOff;
    private bool _isHearingOff;

    private bool IsAnyOff => _isSightOff || _isHearingOff;

    public override string Title => "감지";

    public override bool IsAvailable => IsCurrentScene<StealthScene>();

    public override void DrawButtons()
    {
        DrawButton(_isSightOff ? "시야 꺼짐 (누르면 켜기)" : "시야 끄기", ToggleSight);
        DrawButton(_isHearingOff ? "소리 꺼짐 (누르면 켜기)" : "소리 끄기", ToggleHearing);
        DrawLabel("소리를 끄면 병, 타이머 소리도 못 듣는다");
    }

    public override void OnUpdate()
    {
        UpdateTargetEnemy();
    }

    private void ToggleSight()
    {
        _isSightOff = !_isSightOff;
        ApplyDetection();
    }

    private void ToggleHearing()
    {
        _isHearingOff = !_isHearingOff;
        ApplyDetection();
    }

    /// <summary> 새 집주인이 생기면 이전 수치를 되돌리고 새 집주인 수치에 적용한다 </summary>
    private void UpdateTargetEnemy()
    {
        if (!IsAnyOff || !IsAvailable)
        {
            return;
        }

        // 집주인은 씬이 시작되고 조금 뒤에 생긴다
        if (_enemy == null)
        {
            _enemy = Object.FindObjectOfType<EnemyController>();
        }

        if (_enemy == null || _enemy.Detection == null || _enemy.Detection == _changedDetection)
        {
            return;
        }

        RestoreDetection();

        _changedDetection = _enemy.Detection;
        _originalSightDistance = _changedDetection.SightDistance;
        _originalHearThreshold = _changedDetection.SoundHearThreshold;

        ApplyDetection();
    }

    /// <summary> 켜고 끈 상태대로 수치를 바꾼다 (둘 다 켜지면 되돌린다) </summary>
    private void ApplyDetection()
    {
        if (_changedDetection == null)
        {
            return;
        }

        if (!IsAnyOff)
        {
            RestoreDetection();

            return;
        }

        _changedDetection.SightDistance = _isSightOff ? 0.0f : _originalSightDistance;
        _changedDetection.SoundHearThreshold = _isHearingOff ? float.MaxValue : _originalHearThreshold;
    }

    /// <summary> 바꾼 수치를 테이블 값으로 되돌린다 </summary>
    private void RestoreDetection()
    {
        if (_changedDetection == null)
        {
            return;
        }

        _changedDetection.SightDistance = _originalSightDistance;
        _changedDetection.SoundHearThreshold = _originalHearThreshold;
        _changedDetection = null;
    }
}
#endif
