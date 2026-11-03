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

    [Tooltip("완전히 웅크렸을 때 눈높이 비율 (서 있을 때 발~눈 높이 대비). 충돌 높이와 따로 조절된다.")]
    [Range(0.1f, 1f)]
    [SerializeField] private float _crouchEyeRatio = 0.45f;

    [Header("카메라")]
    [Tooltip("카메라가 그리기 시작하는 최소 거리. 크면 벽에 붙었을 때 벽이 뚫려 보인다.")]
    [SerializeField] private float _nearClipPlane = 0.05f;

    [Header("감도")]
    [SerializeField] private float _mouseSensitivity = 2f;

    [Header("상하 회전 제한")]
    [SerializeField] private float _minPitch = -80f;
    [SerializeField] private float _maxPitch = 80f;

    [Header("숨었을 때")]
    [Tooltip("숨어 있을 때 숨는 위치의 정면 기준으로 좌우를 둘러볼 수 있는 최대 각도")]
    [SerializeField] private float _hiddenYawLimit = 75f;

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    private PlayerMovement _playerMovement;

    private float _pitch;

    // 숨어 있는 동안 카메라가 붙어 있는 지점 (없으면 몸에 붙어 있음)
    private Transform _viewAnchor;
    private float _hiddenYaw;

    /// <summary> YSW : 카메라가 몸이 아닌 다른 지점(숨는 위치 등)에 붙어 있는지 여부 </summary>
    public bool IsViewAttached => _viewAnchor != null;

    #endregion ===== 상태 =====

    private void Awake()
    {
        InitializeComponents();
        InitializeCameraPivot();
        InitializeCameraClip();
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
            CPrint.Warning($"{nameof(PlayerLook)} : 카메라를 찾을 수 없습니다.");
        }
    }

    /// <summary> YSW : 벽에 가까이 붙어도 잘려 보이지 않도록 카메라의 최소 그리기 거리를 줄인다. </summary>
    private void InitializeCameraClip()
    {
        if (_cameraPivot == null)
        {
            return;
        }

        Camera playerCamera = _cameraPivot.GetComponent<Camera>();

        if (playerCamera == null)
        {
            return;
        }

        playerCamera.nearClipPlane = _nearClipPlane;
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

        if (IsViewAttached)
        {
            // 숨어 있을 때는 몸 대신 카메라만 숨는 위치 기준으로 둘러본다.
            _hiddenYaw = Mathf.Clamp(_hiddenYaw + yaw, -_hiddenYawLimit, _hiddenYawLimit);
            _cameraPivot.localRotation = Quaternion.Euler(_pitch, _hiddenYaw, 0f);

            return;
        }

        transform.Rotate(0f, yaw, 0f);
        _cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    /// <summary> YSW : 웅크리기 높이 비율에 맞춰 카메라 높이를 변경한다. (웅크리며 내려간 플레이어 기준점만큼 보정) </summary>
    private void UpdateEyeHeight()
    {
        if (_cameraPivot == null || _playerMovement == null || IsViewAttached)
        {
            return;
        }

        // 발 기준 눈높이를 웅크림 진행도에 맞춰 줄이고, 내려간 플레이어 기준점만큼 보정해 로컬 높이로 바꾼다.
        float standPivotHeight = _playerMovement.StandPivotHeight;
        float standEyeHeight = standPivotHeight + _eyeOffset.y;
        float eyeRatio = Mathf.Lerp(1f, _crouchEyeRatio, _playerMovement.CrouchProgress);
        float eyeHeight = standEyeHeight * eyeRatio - standPivotHeight + _playerMovement.CrouchPivotOffset;

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

    #region ===== 시점 고정 =====

    /// <summary> YSW : 카메라를 몸에서 떼어 지정한 지점(숨는 위치 등)에 붙인다. 지점의 정면을 바라보며 시작한다. </summary>
    public void AttachView(Transform anchor)
    {
        if (_cameraPivot == null || anchor == null)
        {
            return;
        }

        _viewAnchor = anchor;
        _hiddenYaw = 0f;
        _pitch = 0f;

        _cameraPivot.SetParent(anchor, false);
        _cameraPivot.localPosition = Vector3.zero;
        _cameraPivot.localRotation = Quaternion.identity;
    }

    /// <summary> YSW : 카메라를 다시 몸의 눈 위치로 되돌린다. </summary>
    public void DetachView()
    {
        if (_cameraPivot == null || !IsViewAttached)
        {
            return;
        }

        _viewAnchor = null;
        _hiddenYaw = 0f;

        InitializeCameraPosition();
        UpdateEyeHeight();
    }

    #endregion ===== 시점 고정 =====
}
