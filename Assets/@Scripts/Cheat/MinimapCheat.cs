#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;

/// <summary> 잠입 집을 위에서 내려다본 미니맵을 화면 오른쪽 반에 띄운다. 대상 머리 위에서 잘라 보여 주므로 천장과 위층이 안 보인다 </summary>
public class MinimapCheat : BaseCheat
{
    #region ===== 수치 =====

    // 대상 발에서 카메라까지 높이. 이 위는 카메라 뒤라 안 그려진다 (층 간격 2.5보다 조금 낮게)
    private const float CutHeight = 2.2f;
    // 기준 대상 발과 높이 차이가 이보다 작으면 같은 층으로 본다 (층 간격 2.5의 반)
    private const float SameFloorHeight = 1.25f;
    private const float FarClip = 30.0f;
    private const float CameraDepth = 100.0f;
    // 집 둘레에 남길 여백
    private const float ViewMargin = 1.0f;
    private const float BigDotSize = 14.0f;
    private const float SmallDotSize = 8.0f;
    // 바라보는 방향 점을 찍을 거리
    private const float FacingDotDistance = 0.8f;
    private const int StateFontSize = 18;

    private static readonly Rect RightHalfRect = new Rect(0.5f, 0.0f, 0.5f, 1.0f);
    private static readonly Color FacingDotColor = new Color(1.0f, 0.6f, 0.0f);

    #endregion ===== 수치 =====

    #region ===== 상태 =====

    private Camera _camera;
    private EnemyController _enemy;
    private NavMeshAgent _agent;
    private PlayerHide _player;
    private CharacterController _playerController;
    private Bounds _houseBounds;
    private GUIStyle _stateStyle;

    private bool _isOn;
    private bool _isFollowingPlayer;

    private bool IsShowing => _isOn && IsAvailable;

    #endregion ===== 상태 =====

    public override string Title => "미니맵";

    public override bool IsAvailable => IsCurrentScene<StealthScene>();

    protected override void OnInitialize()
    {
        InitializeCamera();
    }

    public override void DrawButtons()
    {
        DrawButton(_isOn ? "미니맵 끄기" : "미니맵 켜기", () => _isOn = !_isOn);
        DrawButton(_isFollowingPlayer ? "층 기준: 플레이어" : "층 기준: 집주인", () => _isFollowingPlayer = !_isFollowingPlayer);
        DrawLabel("빨강 집주인, 주황 바라보는 쪽, 노랑 목적지, 하늘색 플레이어");
    }

    public override void OnUpdate()
    {
        UpdateTargets();
    }

    public override void OnLateUpdate()
    {
        LateUpdateCamera();
    }

    public override void DrawOverlay()
    {
        if (!IsShowing)
        {
            return;
        }

        DrawDots();
        DrawStateText();
    }

    #region ===== 초기화 =====

    /// <summary> 평행 투영으로 곧장 아래를 보는 카메라를 만든다. 천장 그림자가 집 안을 가리지 않게 그림자는 끈다 </summary>
    private void InitializeCamera()
    {
        GameObject cameraObject = new GameObject(nameof(MinimapCheat));
        cameraObject.transform.SetParent(Window.transform);
        cameraObject.transform.rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);

        _camera = cameraObject.AddComponent<Camera>();
        _camera.orthographic = true;
        _camera.nearClipPlane = 0.01f;
        _camera.farClipPlane = FarClip;
        _camera.depth = CameraDepth;
        _camera.rect = RightHalfRect;
        _camera.clearFlags = CameraClearFlags.SolidColor;
        _camera.backgroundColor = Color.black;
        _camera.enabled = false;
        _camera.GetUniversalAdditionalCameraData().renderShadows = false;
    }

    /// <summary> 집 전체가 화면에 들어오도록 NavMesh 범위를 잰다 </summary>
    private void InitializeHouseBounds()
    {
        Vector3[] vertices = NavMesh.CalculateTriangulation().vertices;

        if (vertices.Length == 0)
        {
            return;
        }

        _houseBounds = new Bounds(vertices[0], Vector3.zero);

        foreach (Vector3 vertex in vertices)
        {
            _houseBounds.Encapsulate(vertex);
        }
    }

    #endregion ===== 초기화 =====

    #region ===== 대상 =====

    /// <summary> 집주인과 플레이어를 찾고, 새로 찾으면 그 씬의 집 범위를 다시 잰다 </summary>
    private void UpdateTargets()
    {
        if (!IsShowing)
        {
            return;
        }

        bool isFoundNow = false;

        // 집주인은 씬이 시작되고 조금 뒤에 생긴다
        if (_enemy == null)
        {
            _enemy = Object.FindObjectOfType<EnemyController>();
            _agent = _enemy != null ? _enemy.GetComponent<NavMeshAgent>() : null;
            isFoundNow |= _enemy != null;
        }

        if (_player == null)
        {
            _player = Object.FindObjectOfType<PlayerHide>();
            _playerController = _player != null ? _player.GetComponent<CharacterController>() : null;
            isFoundNow |= _player != null;
        }

        if (isFoundNow)
        {
            InitializeHouseBounds();
        }
    }

    /// <summary> 따라가는 대상의 발 높이를 돌려준다 </summary>
    private float GetTargetFootHeight()
    {
        if (_isFollowingPlayer && _playerController != null)
        {
            return _playerController.bounds.min.y;
        }

        return _enemy != null ? _enemy.transform.position.y : _houseBounds.min.y;
    }

    /// <summary> 그 높이가 기준 대상과 같은 층인지 돌려준다 </summary>
    private bool IsOnViewFloor(float footHeight)
    {
        return Mathf.Abs(footHeight - GetTargetFootHeight()) < SameFloorHeight;
    }

    private bool IsEnemyOnViewFloor => _enemy != null && IsOnViewFloor(_enemy.transform.position.y);

    private bool IsPlayerOnViewFloor => _playerController != null && IsOnViewFloor(_playerController.bounds.min.y);

    #endregion ===== 대상 =====

    #region ===== 카메라 =====

    /// <summary> 집 가운데 위에 두고 높이만 대상이 있는 층에 맞춘다 </summary>
    private void LateUpdateCamera()
    {
        _camera.enabled = IsShowing;

        if (!IsShowing)
        {
            return;
        }

        _camera.transform.position = new Vector3(_houseBounds.center.x, GetTargetFootHeight() + CutHeight, _houseBounds.center.z);

        float halfWidth = _houseBounds.extents.x + ViewMargin;
        float halfDepth = _houseBounds.extents.z + ViewMargin;
        _camera.orthographicSize = Mathf.Max(halfDepth, halfWidth / _camera.aspect);
    }

    #endregion ===== 카메라 =====

    #region ===== 그리기 =====

    /// <summary> 집주인, 바라보는 쪽, 목적지, 플레이어를 점으로 찍는다. 기준 대상과 다른 층에 있는 점은 안 찍는다 </summary>
    private void DrawDots()
    {
        if (_agent != null && IsOnViewFloor(_agent.destination.y))
        {
            DrawDot(_agent.destination, SmallDotSize, Color.yellow);
        }

        if (IsEnemyOnViewFloor)
        {
            Vector3 enemyPosition = _enemy.transform.position;

            DrawDot(enemyPosition + _enemy.transform.forward * FacingDotDistance, SmallDotSize, FacingDotColor);
            DrawDot(enemyPosition, BigDotSize, Color.red);
        }

        if (IsPlayerOnViewFloor)
        {
            DrawDot(_player.transform.position, BigDotSize, Color.cyan);
        }
    }

    /// <summary> 월드 위치에 맞는 화면 자리에 점 하나를 찍는다 </summary>
    private void DrawDot(Vector3 worldPosition, float size, Color color)
    {
        Vector3 screenPoint = _camera.WorldToScreenPoint(worldPosition);

        GUI.color = color;
        GUI.DrawTexture(new Rect(screenPoint.x - size * 0.5f, Screen.height - screenPoint.y - size * 0.5f, size, size), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    /// <summary> 집주인 상태와 게이지를 미니맵 위에 적는다 </summary>
    private void DrawStateText()
    {
        _stateStyle ??= new GUIStyle(GUI.skin.label) { fontSize = StateFontSize, normal = { textColor = Color.white } };

        string target = _isFollowingPlayer ? "플레이어" : "집주인";
        string otherFloor = _isFollowingPlayer ? (IsEnemyOnViewFloor ? "" : " (집주인 다른 층)") : (IsPlayerOnViewFloor ? "" : " (플레이어 다른 층)");
        string text = _enemy != null
            ? $"{target} 층{otherFloor} / {_enemy.CurrentStateName} / 게이지 {_enemy.SuspicionGauge.Gauge:0} / {_enemy.Memory.AlertLevel}"
            : "집주인 없음";

        GUI.Label(new Rect(_camera.pixelRect.x + 10.0f, 10.0f, _camera.pixelRect.width - 20.0f, 30.0f), text, _stateStyle);
    }

    #endregion ===== 그리기 =====
}
#endif
