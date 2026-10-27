using UnityEngine;

/// <summary> YSW : 마우스 입력으로 1인칭 시점을 회전시키고 웅크리기에 맞춰 눈 높이를 조절한다. </summary>
public class PlayerLook : MonoBehaviour
{
    #region ===== 설정 =====

    [Header("참조")]
    [Tooltip("상하 회전을 적용할 카메라. 비워두면 Camera.main을 사용한다.")]
    [SerializeField] private Transform _cameraPivot;

    [Header("시점 위치")]
    [Tooltip("서 있을 때 플레이어 기준 카메라(눈)의 로컬 위치")]
    [SerializeField] private Vector3 _eyeOffset = new Vector3(0f, 1.6f, 0f);

    [Header("감도")]
    [SerializeField] private float _mouseSensitivity = 2f;

    [Header("상하 회전 제한")]
    [SerializeField] private float _minPitch = -80f;
    [SerializeField] private float _maxPitch = 80f;

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    private PlayerMovement _playerMovement;

    private float _pitch;

    #endregion ===== 상태 =====

    private void Awake()
    {
        InitializeComponents();
        InitializeCameraPivot();
        InitializeCameraPosition();
    }

    private void OnEnable()
    {
        SetCursorLock(true);
    }

    private void OnDisable()
    {
        SetCursorLock(false);
    }

    private void Update()
    {
        UpdateLook();
        UpdateEyeHeight();
    }

    #region ===== 초기화 =====

    /// <summary> YSW : 눈 높이 계산에 필요한 컴포넌트를 가져온다. </summary>
    private void InitializeComponents()
    {
        _playerMovement = GetComponent<PlayerMovement>();
    }

    /// <summary> YSW : 시점 회전에 사용할 카메라를 설정한다. </summary>
    private void InitializeCameraPivot()
    {
        if (_cameraPivot == null && Camera.main != null)
        {
            _cameraPivot = Camera.main.transform;
        }

        if (_cameraPivot == null)
        {
            Debug.LogWarning($"{nameof(PlayerLook)} : 카메라를 찾을 수 없습니다.");
        }
    }

    /// <summary> YSW : 카메라를 플레이어의 자식으로 옮기고 눈 위치에 맞춘다. </summary>
    private void InitializeCameraPosition()
    {
        if (_cameraPivot == null)
        {
            return;
        }

        _cameraPivot.SetParent(transform, false);
        _cameraPivot.localPosition = _eyeOffset;
        _cameraPivot.localRotation = Quaternion.identity;

        _pitch = 0f;
    }

    #endregion ===== 초기화 =====

    #region ===== 시점 처리 =====

    /// <summary> YSW : 마우스 입력으로 플레이어는 좌우, 카메라는 상하로 회전시킨다. </summary>
    private void UpdateLook()
    {
        InputManager input = Managers.Input;

        if (input == null || !input.CanReceivePlayer || _cameraPivot == null)
        {
            return;
        }

        float yaw = input.MouseAxisX * _mouseSensitivity;

        _pitch -= input.MouseAxisY * _mouseSensitivity;
        _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);

        transform.Rotate(0f, yaw, 0f);
        _cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    /// <summary> YSW : 웅크리기 높이 비율에 맞춰 카메라 높이를 변경한다. (웅크리며 내려간 플레이어 기준점만큼 보정) </summary>
    private void UpdateEyeHeight()
    {
        if (_cameraPivot == null || _playerMovement == null)
        {
            return;
        }

        float eyeHeight = _eyeOffset.y * _playerMovement.HeightRatio + _playerMovement.CrouchPivotOffset;

        _cameraPivot.localPosition = new Vector3(_eyeOffset.x, eyeHeight, _eyeOffset.z);
    }

    /// <summary> YSW : 마우스 커서의 잠금 상태를 변경한다. </summary>
    private void SetCursorLock(bool isLock)
    {
        InputManager input = Managers.Input;

        if (input == null)
        {
            return;
        }

        input.SetCursorLock(isLock);
    }

    #endregion ===== 시점 처리 =====
}