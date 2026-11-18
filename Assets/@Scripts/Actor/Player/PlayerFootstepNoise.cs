using UnityEngine;

/// <summary>
/// YSW : 플레이어가 걸은 거리만큼 발소리 소음을 낸다. (보폭마다 한 번, 발밑 위치 기준)
/// 소음 크기는 웅크리기 / 걷기 / 뛰기 상태에 따라 다르고, 잠입 씬(StealthScene)의 소음 이벤트로 적에게 전달된다.
/// </summary>
[RequireComponent(typeof(PlayerMovement))]
public class PlayerFootstepNoise : MonoBehaviour
{
    #region ===== 설정 =====

    [Header("소음 크기")]
    [Tooltip("웅크리고 이동할 때 한 걸음의 소음 크기")]
    [SerializeField, Min(0f)] private float _crouchLoudness = 0.4f;

    [Tooltip("걸을 때 한 걸음의 소음 크기")]
    [SerializeField, Min(0f)] private float _walkLoudness = 0.7f;

    [Tooltip("뛸 때 한 걸음의 소음 크기")]
    [SerializeField, Min(0f)] private float _runLoudness = 1.2f;

    [Header("보폭")]
    [Tooltip("웅크리고 이동할 때 발소리 한 번당 이동 거리(m)")]
    [SerializeField, Min(0.1f)] private float _crouchStride = 1.0f;

    [Tooltip("걸을 때 발소리 한 번당 이동 거리(m)")]
    [SerializeField, Min(0.1f)] private float _walkStride = 1.6f;

    [Tooltip("뛸 때 발소리 한 번당 이동 거리(m)")]
    [SerializeField, Min(0.1f)] private float _runStride = 2.2f;

    [Header("디버그")]
    [Tooltip("발소리가 날 때마다 씬 뷰에 소리 크기만큼 원을 잠깐 그린다.")]
    [SerializeField] private bool _drawDebug;

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    private PlayerMovement _playerMovement;
    private CharacterController _characterController;

    // 지난 프레임의 발밑 위치 (실제로 움직인 거리를 재기 위해)
    private Vector3 _lastFootPosition;
    private bool _hasLastFootPosition;

    // 마지막 발소리 이후 걸은 거리
    private float _distanceSinceStep;

    #endregion ===== 상태 =====

    private void Awake()
    {
        InitializeComponents();
    }

    private void OnDisable()
    {
        ResetStep();
    }

    private void Update()
    {
        UpdateFootstep();
    }

    #region ===== 초기화 =====

    /// <summary> YSW : 이동 상태와 발밑 위치 계산에 필요한 컴포넌트를 가져온다. </summary>
    private void InitializeComponents()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _characterController = GetComponent<CharacterController>();
    }

    #endregion ===== 초기화 =====

    #region ===== 발소리 =====

    /// <summary> YSW : 땅을 딛고 실제로 움직인 수평 거리를 모아, 보폭만큼 걸을 때마다 발소리 소음을 낸다. </summary>
    private void UpdateFootstep()
    {
        if (!CanMakeFootstep())
        {
            ResetStep();

            return;
        }

        Vector3 footPosition = GetFootPosition();

        if (!_hasLastFootPosition)
        {
            _lastFootPosition = footPosition;
            _hasLastFootPosition = true;

            return;
        }

        Vector3 moved = footPosition - _lastFootPosition;
        moved.y = 0f;

        _lastFootPosition = footPosition;

        // 벽에 막혀 제자리에서 비비는 경우는 실제 이동 거리가 없으므로 소리가 나지 않는다.
        _distanceSinceStep += moved.magnitude;

        float stride = GetCurrentStride();

        if (_distanceSinceStep < stride)
        {
            return;
        }

        _distanceSinceStep -= stride;

        MakeNoise(footPosition, GetCurrentLoudness());
    }

    /// <summary> YSW : 이동이 켜져 있고(숨어 있지 않음) 땅을 딛고 있을 때만 발소리가 난다. </summary>
    private bool CanMakeFootstep()
    {
        if (_playerMovement == null || !_playerMovement.enabled)
        {
            return false;
        }

        if (_characterController == null || !_characterController.enabled)
        {
            return false;
        }

        return _characterController.isGrounded;
    }

    /// <summary> YSW : 잠입 씬에 발소리 소음을 알린다. (잠입 씬이 아니면 무시) </summary>
    private void MakeNoise(Vector3 position, float loudness)
    {
        if (loudness <= 0f)
        {
            return;
        }

        StealthScene stealthScene = Managers.Scene?.CurrentScene as StealthScene;

        if (stealthScene == null)
        {
            return;
        }

        stealthScene.RaiseNoiseMade(position, loudness);

        if (_drawDebug)
        {
            DrawDebugNoise(position, loudness);
        }
    }

    /// <summary> YSW : 발소리 상태와 쌓인 거리를 초기화한다. </summary>
    private void ResetStep()
    {
        _hasLastFootPosition = false;
        _distanceSinceStep = 0f;
    }

    #endregion ===== 발소리 =====

    #region ===== 상태별 값 =====

    /// <summary> YSW : 현재 이동 상태(웅크리기 / 뛰기 / 걷기)에 맞는 소음 크기를 반환한다. </summary>
    private float GetCurrentLoudness()
    {
        if (_playerMovement.IsCrouching)
        {
            return _crouchLoudness;
        }

        return _playerMovement.IsRunning ? _runLoudness : _walkLoudness;
    }

    /// <summary> YSW : 현재 이동 상태에 맞는 보폭을 반환한다. </summary>
    private float GetCurrentStride()
    {
        if (_playerMovement.IsCrouching)
        {
            return _crouchStride;
        }

        return _playerMovement.IsRunning ? _runStride : _walkStride;
    }

    /// <summary> YSW : 캐릭터 컨트롤러 바닥면 중앙(발밑)의 월드 위치를 반환한다. (웅크리면 줄어든 높이 기준) </summary>
    private Vector3 GetFootPosition()
    {
        Vector3 center = transform.TransformPoint(_characterController.center);

        return center + Vector3.down * (_characterController.height * 0.5f);
    }

    #endregion ===== 상태별 값 =====

    #region ===== 디버그 =====

    /// <summary> YSW : 발밑에 소음 크기에 비례한 원을 1초 동안 그린다. (씬 뷰에서만 보인다) </summary>
    private void DrawDebugNoise(Vector3 position, float loudness)
    {
        const int Segments = 16;
        const float Duration = 1f;

        Color color = Color.Lerp(Color.green, Color.red, loudness / Mathf.Max(_runLoudness, 0.01f));
        Vector3 previous = position + Vector3.forward * loudness;

        for (int i = 1; i <= Segments; i++)
        {
            float angle = i * Mathf.PI * 2f / Segments;
            Vector3 next = position + new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * loudness;

            Debug.DrawLine(previous, next, color, Duration);
            previous = next;
        }
    }

    #endregion ===== 디버그 =====
}
