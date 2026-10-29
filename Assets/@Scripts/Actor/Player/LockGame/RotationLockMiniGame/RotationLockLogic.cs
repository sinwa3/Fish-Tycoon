using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary> SJY : 회전형 자물쇠의 회전 제한 및 해제 판정 규칙을 관리한다. </summary>
public class RotationLockLogic : MonoBehaviour
{
    [Header("정답 각도 설정")]
    [SerializeField] private float _minTargetAngle = -90f;
    [SerializeField] private float _maxTargetAngle = 90f;
    [SerializeField] private float _angleTolerance = 8f;    // 정답으로 인정하는 오차
    [SerializeField] private float _angleFalloffRange = 40f;    // 회전 불가능한 최소 각도 차이

    [Header("실린더 설정")]
    [SerializeField] private float _maxCylinderAngle = -90f;
    [SerializeField] private float _wrongCylinderAngle = -5f;

    [Header("Gizmo 설정")]
    [SerializeField] private Transform _lockCylinderPivot;
    [SerializeField] private bool _showAngleGizmo = true;
    [SerializeField] private float _gizmoRadius = 1.5f;

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
            return _wrongCylinderAngle;
        }

        float rotationRatio = Mathf.InverseLerp(_angleFalloffRange, _angleTolerance, angleDifference);

        return Mathf.Lerp(_wrongCylinderAngle, _maxCylinderAngle, rotationRatio);
    }

    /// <summary>SJY : 현재 락픽 각도가 정답 허용 범위에 있는지 확인</summary>
    public bool IsCorrectAngle(float currentAngle)
    {
        return Mathf.Abs(currentAngle - _targetAngle) <= _angleTolerance;
    }

    /// <summary>SJY : 현재 락픽 각도와 정답 각도의 차이가 회전 제한 범위를 초과했는지 확인</summary>
    public bool IsOutsideFalloffRange(float currentAngle)
    {
        float angleDifference = Mathf.Abs(currentAngle - _targetAngle);

        return angleDifference > _angleFalloffRange;
    }

    /// <summary>SJY : 실린더가 최대 회전 각도에 도달했는지 확인하여 잠금 해제 성공 여부를 반환</summary>
    public bool IsUnlockCompleted(float currentAngle, float currentCylinderAngle)
    {
        if (!IsCorrectAngle(currentAngle))
        {
            return false;
        }

        return Mathf.Abs(currentCylinderAngle - _maxCylinderAngle) <= 0.01f;
    }


#if UNITY_EDITOR
    /// <summary>SJY : Scene 뷰에 정답 각도와 판정 범위 표시</summary>
    private void OnDrawGizmos()
    {
        if (!_showAngleGizmo || _lockCylinderPivot == null)
        {
            return;
        }

        Vector3 center = _lockCylinderPivot.position;
        Vector3 normal = transform.forward;
        Vector3 up = transform.up;

        float targetAngle = Application.isPlaying ? _targetAngle : 0f;
        float falloffRange = Mathf.Max(_angleFalloffRange, _angleTolerance);

        Vector3 falloffStart = Quaternion.AngleAxis(targetAngle - falloffRange, normal) * up;
        Vector3 toleranceStart = Quaternion.AngleAxis(targetAngle - _angleTolerance, normal) * up;
        Vector3 targetDirection = Quaternion.AngleAxis(targetAngle, normal) * up;

        Handles.color = new Color(1f, 0.8f, 0f, 0.1f);
        Handles.DrawSolidArc(center, normal, falloffStart, falloffRange * 2f, _gizmoRadius);

        Handles.color = new Color(0f, 1f, 0f, 0.1f);
        Handles.DrawSolidArc(center, normal, toleranceStart, _angleTolerance * 2f, _gizmoRadius);

        Handles.color = Color.green;
        Handles.DrawLine(center, center + targetDirection * (_gizmoRadius + 0.2f));
    }
#endif
}
