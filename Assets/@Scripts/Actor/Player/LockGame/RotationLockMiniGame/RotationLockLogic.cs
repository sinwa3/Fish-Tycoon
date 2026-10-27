using UnityEngine;

/// <summary> SJY : 회전형 자물쇠의 회전 제한 및 해제 판정 규칙을 관리한다. </summary>
public class RotationLockLogic : MonoBehaviour
{
    [Header("정답 각도 설정")]
    [SerializeField] private float _minTargetAngle = -90f;
    [SerializeField] private float _maxTargetAngle = 90f;
    [SerializeField] private float _angleTolerance = 8f;
    [SerializeField] private float _angleFalloffRange = 40f;

    [Header("실린더 설정")]
    [SerializeField] private float _maxCylinderAngle = 90f;

    private float _targetAngle;

    public float MaxCylinderAngle => _maxCylinderAngle;
    public float TargetAngle => _targetAngle;

    /// <summary>SJY : 미니게임 시작 시 정답 각도를 무작위로 설정</summary>
    public void Initialize()
    {
        _targetAngle = Random.Range(_minTargetAngle, _maxTargetAngle);

        CPrint.Log($"[RotationLockLogic] 정답 각도 : {_targetAngle:F1}");
    }

    /// <summary>SJY : 현재 락픽 각도와 정답 각도의 차이에 따라 실린더의 최대 회전 가능 각도를 계산</summary>
    public float CalculateMaxCylinderAngle(float currentAngle)
    {
        float angleDifference = Mathf.Abs(currentAngle - _targetAngle);

        if (angleDifference <= _angleTolerance)
        {
            return _maxCylinderAngle;
        }

        if (angleDifference >= _angleFalloffRange)
        {
            return 0f;
        }

        float rotationRatio = Mathf.InverseLerp(_angleFalloffRange, _angleTolerance, angleDifference);

        return _maxCylinderAngle * rotationRatio;
    }


}
