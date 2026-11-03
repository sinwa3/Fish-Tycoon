using UnityEngine;

/// <summary> YSW : 플레이어가 상호작용해 열고 닫는 문. 경첩(오브젝트 기준점)을 축으로 Y 회전하며, 항상 플레이어 반대쪽으로 열린다. </summary>
public class Door : MonoBehaviour, IInteractable
{
    #region ===== 설정 =====

    private const float DefaultLeafOffset = 0.5f;

    [Header("회전")]
    [Tooltip("열렸을 때 닫힌 상태에서 돌아가는 각도")]
    [SerializeField] private float _openAngle = 90f;

    [Tooltip("초당 회전 각도 (클수록 빨리 열리고 닫힌다)")]
    [SerializeField] private float _rotateSpeed = 180f;

    [Header("안내")]
    [SerializeField] private string _openText = "열기";
    [SerializeField] private string _closeText = "닫기";

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    // 시작할 때의 회전 (씬에는 문을 닫힌 상태로 배치한다)
    private Quaternion _closedRotation;
    private Quaternion _targetRotation;

    // 닫혀 있을 때 기준점(경첩)에서 문짝 중심까지의 로컬 위치 (어느 쪽으로 열지 계산할 때 사용)
    private Vector3 _leafLocalCenter;

    /// <summary> YSW : 문이 열린 상태(또는 열리는 중)인지 여부 </summary>
    public bool IsOpen { get; private set; }

    /// <summary> YSW : 문이 아직 목표 각도까지 회전하는 중인지 여부 </summary>
    public bool IsRotating => Quaternion.Angle(transform.localRotation, _targetRotation) > 0.01f;

    public string InteractionText => IsOpen ? _closeText : _openText;

    #endregion ===== 상태 =====

    private void Awake()
    {
        InitializeRotation();
        InitializeLeafCenter();
    }

    private void Update()
    {
        UpdateRotation();
    }

    #region ===== 초기화 =====

    /// <summary> YSW : 시작할 때의 회전을 닫힌 상태로 저장한다. </summary>
    private void InitializeRotation()
    {
        _closedRotation = transform.localRotation;
        _targetRotation = _closedRotation;
    }

    /// <summary> YSW : 렌더러 범위로 문짝 중심 위치를 구한다. (없으면 기준점 옆 0.5m로 가정) </summary>
    private void InitializeLeafCenter()
    {
        Renderer leafRenderer = GetComponentInChildren<Renderer>();

        _leafLocalCenter = leafRenderer != null
            ? transform.InverseTransformPoint(leafRenderer.bounds.center)
            : Vector3.right * DefaultLeafOffset;

        _leafLocalCenter.y = 0f;
    }

    #endregion ===== 초기화 =====

    #region ===== 상호작용 =====

    /// <summary> YSW : 문은 언제든 열고 닫을 수 있다. </summary>
    public bool CanInteract(GameObject interactor)
    {
        return true;
    }

    /// <summary> YSW : 닫혀 있으면 상호작용한 쪽의 반대편으로 열고, 열려 있으면 닫는다. </summary>
    public void Interact(GameObject interactor)
    {
        if (IsOpen)
        {
            Close();

            return;
        }

        Vector3 interactorPosition = interactor != null ? interactor.transform.position : transform.position;

        Open(interactorPosition);
    }

    /// <summary> YSW : 지정한 위치(플레이어)에서 멀어지는 방향으로 문을 연다. </summary>
    public void Open(Vector3 awayFromPosition)
    {
        float direction = GetOpenDirection(awayFromPosition);

        _targetRotation = _closedRotation * Quaternion.Euler(0f, direction * _openAngle, 0f);
        IsOpen = true;
    }

    /// <summary> YSW : 문을 닫힌 각도로 되돌린다. </summary>
    public void Close()
    {
        _targetRotation = _closedRotation;
        IsOpen = false;
    }

    #endregion ===== 상호작용 =====

    #region ===== 회전 처리 =====

    /// <summary> YSW : 목표 회전까지 일정 속도로 돌린다. </summary>
    private void UpdateRotation()
    {
        if (!IsRotating)
        {
            return;
        }

        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, _targetRotation, _rotateSpeed * Time.deltaTime);
    }

    /// <summary> YSW : 양쪽으로 열었을 때 문짝 중심이 지정한 위치에서 더 멀어지는 방향(+1 또는 -1)을 반환한다. </summary>
    private float GetOpenDirection(Vector3 awayFromPosition)
    {
        float positiveDistance = GetLeafDistanceWhenOpened(1f, awayFromPosition);
        float negativeDistance = GetLeafDistanceWhenOpened(-1f, awayFromPosition);

        return positiveDistance >= negativeDistance ? 1f : -1f;
    }

    /// <summary> YSW : 지정한 방향으로 다 열렸을 때 문짝 중심과 지정한 위치 사이의 수평 거리(제곱)를 반환한다. </summary>
    private float GetLeafDistanceWhenOpened(float direction, Vector3 fromPosition)
    {
        Quaternion parentRotation = transform.parent != null ? transform.parent.rotation : Quaternion.identity;
        Quaternion openedRotation = parentRotation * _closedRotation * Quaternion.Euler(0f, direction * _openAngle, 0f);

        Vector3 leafOffset = Vector3.Scale(_leafLocalCenter, transform.lossyScale);
        Vector3 leafCenter = transform.position + openedRotation * leafOffset;

        Vector3 toPosition = fromPosition - leafCenter;
        toPosition.y = 0f;

        return toPosition.sqrMagnitude;
    }

    #endregion ===== 회전 처리 =====
}
