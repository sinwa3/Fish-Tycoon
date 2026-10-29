using UnityEngine;

/// <summary> KYH : 플레이어가 보이는지 검사하고 기억에 적는다 </summary>
public class EnemySight : MonoBehaviour
{
    #region ===== 상태 =====

    // 레이캐스트가 맞힐 레이어
    [SerializeField] private LayerMask _sightBlockLayers = ~0;

    private EnemyController _enemy;

    // 다음 검사까지 남은 시간
    private float _checkTimer;

    /// <summary> KYH : 마지막 검사에서 플레이어가 보였는지 </summary>
    public bool IsPlayerVisible { get; private set; }

    #endregion ===== 상태 =====

    #region ===== 플레이어 =====

    private Transform _player;
    private PlayerHide _playerHide;
    private PlayerMovement _playerMovement;
    private CharacterController _playerController;

    #endregion ===== 플레이어 =====

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        InitializePlayer();
    }

    private void Update()
    {
        UpdateSightCheck();
    }

    private void OnDrawGizmosSelected()
    {
        DrawSightGizmos();
    }

    #region ===== 초기화 =====

    /// <summary> KYH : 같은 오브젝트의 컨트롤러를 캐싱한다 </summary>
    private void InitializeComponents()
    {
        _enemy = GetComponent<EnemyController>();

        if (_enemy == null)
        {
            CPrint.Warning("[EnemySight] 적 컨트롤러 없음");
        }
    }

    /// <summary> KYH : 플레이어와 그 컴포넌트를 찾아 캐싱한다 </summary>
    private void InitializePlayer()
    {
        PlayerHide playerHide = FindObjectOfType<PlayerHide>();

        if (playerHide == null)
        {
            CPrint.Warning("[EnemySight] 플레이어 없음");

            return;
        }

        _playerHide = playerHide;
        _player = playerHide.transform;
        _playerMovement = playerHide.GetComponent<PlayerMovement>();
        _playerController = playerHide.GetComponent<CharacterController>();
    }

    #endregion ===== 초기화 =====

    #region ===== 검사 주기 =====

    /// <summary> KYH : 검사 간격마다 시야를 검사한다 </summary>
    private void UpdateSightCheck()
    {
        _checkTimer -= Time.deltaTime;

        if (_checkTimer > 0.0f)
        {
            return;
        }

        _checkTimer = EnemyTempValue.SightCheckInterval;
        IsPlayerVisible = CheckPlayerVisible();

        if (IsPlayerVisible)
        {
            ReportToMemory(_player.position);
        }
    }

    #endregion ===== 검사 주기 =====

    #region ===== 시야 검사 =====

    /// <summary> KYH : 플레이어가 보이는지 돌려준다 </summary>
    private bool CheckPlayerVisible()
    {
        if (_player == null)
        {
            return false;
        }

        if (_playerHide.IsHiding)
        {
            return false;
        }

        Vector3 eyePosition = GetEyePosition();
        Vector3 playerTargetPoint = GetPlayerTargetPoint();

        return
            IsInSightDistance(eyePosition, playerTargetPoint) &&
            IsInSightAngle(playerTargetPoint) &&
            IsLineOfSightClear(eyePosition, playerTargetPoint);
    }

    /// <summary> KYH : 시야 거리 안인지 돌려준다 </summary>
    private bool IsInSightDistance(Vector3 eyePosition, Vector3 targetPosition)
    {
        float allowDistance = EnemyTempValue.SightDistance * GetCrouchSightMultiplier();
        Vector3 targetDirection = targetPosition - eyePosition;

        return targetDirection.sqrMagnitude <= allowDistance * allowDistance;
    }

    /// <summary> KYH : 시야각 안인지 돌려준다 </summary>
    private bool IsInSightAngle(Vector3 targetPosition)
    {
        Vector3 targetDirection = Vector3.ProjectOnPlane(targetPosition - transform.position, Vector3.up);

        return Vector3.Angle(transform.forward, targetDirection) <= EnemyTempValue.SightAngle * 0.5f;
    }

    /// <summary> KYH : 가리는 것이 없는지 돌려준다 </summary>
    private bool IsLineOfSightClear(Vector3 eyePosition, Vector3 targetPosition)
    {
        Vector3 targetDirection = targetPosition - eyePosition;

        bool isHit = Physics.Raycast
            (
            eyePosition,
            targetDirection,
            out RaycastHit hit,
            targetDirection.magnitude,
            _sightBlockLayers,
            QueryTriggerInteraction.Ignore
            );

        return !isHit || hit.collider.transform.IsChildOf(_player);
    }

    /// <summary> KYH : 웅크림에 따른 시야 거리 배율을 돌려준다 </summary>
    private float GetCrouchSightMultiplier()
    {
        return _playerMovement.IsCrouching ? EnemyTempValue.CrouchSightMultiplier : 1.0f;
    }

    /// <summary> KYH : 집주인 눈 위치를 돌려준다 </summary>
    private Vector3 GetEyePosition()
    {
        return transform.position + Vector3.up * EnemyTempValue.EyeHeight;
    }

    /// <summary> KYH : 가림을 검사할 플레이어 쪽 점을 돌려준다 </summary>
    private Vector3 GetPlayerTargetPoint()
    {
        Bounds bounds = _playerController.bounds;

        return new Vector3(bounds.center.x, bounds.max.y - EnemyTempValue.SightTargetTopOffset, bounds.center.z);
    }

    #endregion ===== 시야 검사 =====

    #region ===== 기억 =====

    /// <summary> KYH : 본 위치를 기억에 적는다 </summary>
    private void ReportToMemory(Vector3 playerPosition)
    {
        if (_enemy == null)
        {
            return;
        }

        _enemy.Memory.ReportPlayerSeen(playerPosition);
        _enemy.Memory.ReportStimulus(playerPosition, EnemyTempValue.SightStimulusStrength, StimulusType.Sight);
    }

    #endregion ===== 기억 =====

    #region ===== 디버그 =====

    /// <summary> KYH : 시야 범위를 그린다 </summary>
    private void DrawSightGizmos()
    {
        Gizmos.color = IsPlayerVisible ? Color.red : Color.yellow;

        Vector3 eyePosition = GetEyePosition();
        Gizmos.DrawWireSphere(eyePosition, EnemyTempValue.SightDistance);

        Vector3 rightEdgeDirection = Quaternion.Euler(0.0f, EnemyTempValue.SightAngle * 0.5f, 0.0f) * transform.forward;
        Vector3 leftEdgeDirection = Quaternion.Euler(0.0f, -EnemyTempValue.SightAngle * 0.5f, 0.0f) * transform.forward;
        Gizmos.DrawLine(eyePosition, eyePosition + rightEdgeDirection * EnemyTempValue.SightDistance);
        Gizmos.DrawLine(eyePosition, eyePosition + leftEdgeDirection * EnemyTempValue.SightDistance);
    }

    #endregion ===== 디버그 =====
}
