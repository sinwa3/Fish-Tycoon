using UnityEngine;

/// <summary> YSW : 플레이어가 상호작용해 숨을 수 있는 엄폐 오브젝트 </summary>
public class HidingSpot : MonoBehaviour, IInteractable
{
    #region ===== 설정 =====

    private const float GizmoFieldOfView = 60f;
    private const float GizmoViewDistance = 1.5f;
    private const float GizmoNearClip = 0.3f;
    private const float GizmoAspect = 16f / 9f;

    [Header("위치")]
    [Tooltip("숨었을 때 카메라(눈)가 위치할 곳. 문 바로 바깥쪽에 두고 파란 화살표(정면)가 밖을 보도록 한다. (안쪽에 두면 문 안쪽 면만 보인다) 비워두면 이 오브젝트 위치를 사용한다.")]
    [SerializeField] private Transform _hidePoint;

    [Tooltip("나올 때 플레이어 몸이 나타날 곳. 비워두면 숨기 전 자리에 다시 나타난다.")]
    [SerializeField] private Transform _exitPoint;

    [Header("안내")]
    [SerializeField] private string _interactionText = "숨기";

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    /// <summary> YSW : 숨었을 때 카메라(눈)가 위치할 지점 </summary>
    public Transform HidePoint => _hidePoint != null ? _hidePoint : transform;

    /// <summary> YSW : 나올 때 플레이어가 위치할 지점 (없으면 null) </summary>
    public Transform ExitPoint => _exitPoint;

    public string InteractionText => _interactionText;

    public InteractionType Type => InteractionType.Wardrobe;

    #endregion ===== 상태 =====

    private void OnDrawGizmos()
    {
        DrawPointGizmos();
    }

    #region ===== 상호작용 =====

    /// <summary> YSW : 엄폐 오브젝트는 항상 상호작용할 수 있다. </summary>
    public bool CanInteract(GameObject interactor)
    {
        return true;
    }

    /// <summary> YSW : 상호작용한 플레이어를 이 엄폐 오브젝트에 숨긴다. </summary>
    public void Interact(GameObject interactor)
    {
        if (interactor == null)
        {
            return;
        }

        PlayerHide playerHide = interactor.GetComponent<PlayerHide>();

        if (playerHide == null)
        {
            CPrint.Warning($"{nameof(HidingSpot)} : [{interactor.name}]에 PlayerHide가 없습니다.");

            return;
        }

        playerHide.EnterHiding(this);
    }

    #endregion ===== 상호작용 =====

    #region ===== 디버그 =====

    /// <summary> YSW : 씬 뷰에 숨었을 때 시점(초록, 선은 바라보는 방향)과 나오는 위치(노랑)를 표시한다. </summary>
    private void DrawPointGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(HidePoint.position, 0.1f);

        // 숨었을 때 카메라가 보게 될 범위를 사각뿔로 표시한다. (가까운 면 0.3 = 카메라 Near Clip)
        Matrix4x4 previousMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(HidePoint.position, HidePoint.rotation, Vector3.one);
        Gizmos.DrawFrustum(Vector3.zero, GizmoFieldOfView, GizmoViewDistance, GizmoNearClip, GizmoAspect);
        Gizmos.matrix = previousMatrix;

        if (_exitPoint == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_exitPoint.position, 0.2f);
        Gizmos.DrawLine(HidePoint.position, _exitPoint.position);
    }

    #endregion ===== 디버그 =====
}
