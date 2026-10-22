using UnityEngine;

/// <summary> YSW : 플레이어가 엄폐 오브젝트에 숨고 나오는 상태를 관리한다. </summary>
[RequireComponent(typeof(PlayerMovement))]
public class PlayerHide : MonoBehaviour
{
    #region ===== 상태 =====

    private CharacterController _characterController;
    private PlayerMovement _playerMovement;

    private Vector3 _returnPosition;
    private Quaternion _returnRotation;

    /// <summary> YSW : 현재 숨어 있는 엄폐 오브젝트 (숨어 있지 않으면 null) </summary>
    public HidingSpot CurrentSpot { get; private set; }

    /// <summary> YSW : 숨어 있는 상태 여부 </summary>
    public bool IsHiding => CurrentSpot != null;

    #endregion ===== 상태 =====

    private void Awake()
    {
        InitializeComponents();
    }

    #region ===== 초기화 =====

    /// <summary> YSW : 숨기에 필요한 컴포넌트를 가져온다. </summary>
    private void InitializeComponents()
    {
        _characterController = GetComponent<CharacterController>();
        _playerMovement = GetComponent<PlayerMovement>();
    }

    #endregion ===== 초기화 =====

    #region ===== 숨기 처리 =====

    /// <summary> YSW : 이동을 멈추고 지정한 엄폐 오브젝트의 숨는 위치로 이동한다. </summary>
    public void EnterHiding(HidingSpot hidingSpot)
    {
        if (IsHiding || hidingSpot == null)
        {
            return;
        }

        _returnPosition = transform.position;
        _returnRotation = transform.rotation;

        SetMovementEnabled(false);

        Transform hidePoint = hidingSpot.HidePoint;
        transform.SetPositionAndRotation(hidePoint.position, GetYawRotation(hidePoint));

        CurrentSpot = hidingSpot;
    }

    /// <summary> YSW : 엄폐 오브젝트에서 나와 나오는 위치로 이동하고 이동을 다시 활성화한다. </summary>
    public void ExitHiding()
    {
        if (!IsHiding)
        {
            return;
        }

        Transform exitPoint = CurrentSpot.ExitPoint;

        if (exitPoint != null)
        {
            transform.SetPositionAndRotation(exitPoint.position, GetYawRotation(exitPoint));
        }
        else
        {
            transform.SetPositionAndRotation(_returnPosition, _returnRotation);
        }

        CurrentSpot = null;

        SetMovementEnabled(true);
    }

    /// <summary> YSW : 이동과 충돌 처리를 켜거나 끈다. </summary>
    private void SetMovementEnabled(bool isEnabled)
    {
        _playerMovement.enabled = isEnabled;
        _characterController.enabled = isEnabled;
    }

    /// <summary> YSW : 지정한 지점의 좌우 회전만 반환한다. (플레이어가 기울지 않도록) </summary>
    private Quaternion GetYawRotation(Transform point)
    {
        return Quaternion.Euler(0f, point.eulerAngles.y, 0f);
    }

    #endregion ===== 숨기 처리 =====
}
