using UnityEngine;

/// <summary> KYH : 집주인이 한 지점을 바라보거나 제자리에서 두리번거리게 한다 </summary>
public class EnemyLook : MonoBehaviour
{
    // 지금 하고 있는 바라보기
    private enum LookMode
    {
        None,
        LookAt,
        LookAround
    }

    #region ===== 수치 =====

    private const float MinLookDistance = 0.0001f;
    // 초당 회전 각도
    private const float TurnSpeed = 180.0f;

    #endregion ===== 수치 =====

    #region ===== 상태 =====

    private LookMode _lookMode = LookMode.None;

    // LookAt으로 바라볼 위치
    private Vector3 _lookTarget;

    // 두리번 시간, 지난 시간
    private float _lookAroundDuration;
    private float _lookAroundTimer;

    /// <summary> KYH : 두리번이 끝났는지 </summary>
    public bool IsLookAroundFinished { get; private set; }

    /// <summary> KYH : 두리번 중인지 </summary>
    public bool IsLookAround => _lookMode == LookMode.LookAround;

    #endregion ===== 상태 =====

    private void Update()
    {
        UpdateLook();
    }

    #region ===== 시작, 멈춤 =====

    /// <summary> KYH : 한 지점을 향해 몸을 돌리기 시작 </summary>
    public void LookAt(Vector3 position)
    {
        _lookTarget = position;
        _lookMode = LookMode.LookAt;
    }

    /// <summary> KYH : 정한 시간 동안 제자리에서 좌우로 두리번거리기 시작한다 </summary>
    public void LookAround(float duration)
    {
        _lookAroundTimer = 0.0f;
        _lookAroundDuration = duration;
        IsLookAroundFinished = false;
        _lookMode = LookMode.LookAround;
    }

    /// <summary> KYH : 하던 바라보기를 멈춘다 </summary>
    public void StopLook()
    {
        _lookMode = LookMode.None;
    }

    #endregion ===== 시작, 멈춤 =====

    #region ===== 회전 =====

    /// <summary> KYH : 지금 모드에 맞게 몸을 돌린다 </summary>
    private void UpdateLook()
    {
        switch (_lookMode)
        {
            case LookMode.None:
                break;
            case LookMode.LookAt:
                UpdateLookAt();
                break;
            case LookMode.LookAround:
                UpdateLookAround();
                break;
        }
    }

    /// <summary> KYH : 바라볼 위치로 y축만 회전한다 </summary>
    private void UpdateLookAt()
    {
        Vector3 lookDirection = _lookTarget - transform.position;
        lookDirection = Vector3.ProjectOnPlane(lookDirection, Vector3.up);

        if (lookDirection.sqrMagnitude < MinLookDistance)
        {
            return;
        }

        Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, TurnSpeed * Time.deltaTime);
    }

    /// <summary> KYH : 두리번 시간이 지나면 끝낸다 (몸 움직임은 애니메이션이 한다) </summary>
    private void UpdateLookAround()
    {
        _lookAroundTimer += Time.deltaTime;

        if (_lookAroundTimer >= _lookAroundDuration)
        {
            IsLookAroundFinished = true;
            _lookMode = LookMode.None;
        }
    }

    #endregion ===== 회전 =====
}
