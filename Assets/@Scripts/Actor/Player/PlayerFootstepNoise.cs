using UnityEngine;

/// <summary>
/// YSW : 플레이어가 걸은 거리만큼 발소리 소음을 낸다. (보폭마다 한 번, 발밑 위치 기준)
/// 소음 크기는 웅크리기 / 걷기 / 뛰기 상태에 따라 다르고, 잠입 씬(StealthScene)의 소음 이벤트로 적에게 전달된다.
/// 같은 순간에 플레이어가 듣는 발소리 효과음(PlayerFoot)도 상태에 맞는 볼륨으로 재생한다.
/// 양말을 신고 있으면 양말의 적용 값(m)만큼 발소리가 들리는 범위가 줄어든다.
/// </summary>
[RequireComponent(typeof(PlayerMovement))]
public class PlayerFootstepNoise : MonoBehaviour
{
    #region ===== 설정 =====

    // 집주인 감지 테이블을 읽지 못했을 때 쓰는 소리 크기 1당 들리는 범위(m)
    private const float DefaultSoundRangePerLoudness = 5f;

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

    [Header("효과음 볼륨")]
    [Tooltip("웅크리고 이동할 때 발소리 효과음 볼륨 (0 ~ 1)")]
    [SerializeField, Range(0f, 1f)] private float _crouchVolume = 0.3f;

    [Tooltip("걸을 때 발소리 효과음 볼륨 (0 ~ 1)")]
    [SerializeField, Range(0f, 1f)] private float _walkVolume = 0.6f;

    [Tooltip("뛸 때 발소리 효과음 볼륨 (0 ~ 1)")]
    [SerializeField, Range(0f, 1f)] private float _runVolume = 1f;

    [Tooltip("양말로 발소리가 작아져도 효과음은 이 비율 아래로 줄이지 않는다. (플레이어가 자기 발소리는 계속 들을 수 있도록)")]
    [SerializeField, Range(0f, 1f)] private float _minVolumeRatio = 0.4f;

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

    // 소리 크기 1당 적에게 들리는 범위(m). 집주인 감지 테이블에서 한 번만 읽는다. (0 이하면 아직 안 읽음)
    private float _soundRangePerLoudness;

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

    /// <summary>
    /// YSW : 잠입 씬에 발소리 소음을 알리고 효과음을 재생한다. (잠입 씬이 아니면 무시)
    /// 양말로 줄어든 소음 크기가 0 이하면 적에게는 들리지 않고, 효과음만 작게 재생한다.
    /// </summary>
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

        float reducedLoudness = ApplySocksReduction(loudness);

        if (reducedLoudness > 0f)
        {
            stealthScene.RaiseNoiseMade(position, reducedLoudness);
        }

        PlayFootstepSound(reducedLoudness / loudness);

        if (_drawDebug)
        {
            DrawDebugNoise(position, reducedLoudness);
        }
    }

    /// <summary>
    /// YSW : 플레이어가 듣는 발소리 효과음을 현재 상태에 맞는 볼륨으로 재생한다. (1인칭 내 발소리이므로 2D)
    /// 양말로 소음이 줄어든 비율만큼 볼륨도 줄이되, 최소 비율 아래로는 줄이지 않는다.
    /// </summary>
    private void PlayFootstepSound(float loudnessRatio)
    {
        float volume = GetCurrentVolume() * Mathf.Max(Mathf.Clamp01(loudnessRatio), _minVolumeRatio);

        if (volume <= 0f)
        {
            return;
        }

        Managers.Sound?.PlaySfx(SoundKey.Name.SfxType.PlayerFoot, volume);
    }

    /// <summary> YSW : 발소리 상태와 쌓인 거리를 초기화한다. </summary>
    private void ResetStep()
    {
        _hasLastFootPosition = false;
        _distanceSinceStep = 0f;
    }

    #endregion ===== 발소리 =====

    #region ===== 양말 =====

    /// <summary>
    /// YSW : 신고 있는 양말만큼 소음 크기를 줄여 반환한다.
    /// 양말 적용 값은 "들리는 범위 감소(m)"이고, 적이 듣는 범위는 소음 크기 × 소리 범위(m)이므로
    /// 줄일 범위를 소리 범위로 나눈 만큼 소음 크기를 뺀다. (예 : 걷기 0.7 × 5m = 3.5m, 양말 -1m → 2.5m = 0.5)
    /// </summary>
    private float ApplySocksReduction(float loudness)
    {
        float reduceRange = GetSocksReduceRange();

        if (reduceRange <= 0f)
        {
            return loudness;
        }

        return Mathf.Max(0f, loudness - reduceRange / GetSoundRangePerLoudness());
    }

    /// <summary> YSW : 신고 있는 양말이 줄여주는 발소리 범위(m)를 반환한다. (아이템 테이블 적용 값은 음수로 들어 있다, 양말이 없으면 0) </summary>
    private static float GetSocksReduceRange()
    {
        DataManager data = Managers.Data;
        ItemTable itemTable = Managers.Table?.Item;

        if (data == null || itemTable == null)
        {
            return 0f;
        }

        Item socks = itemTable.GetItem(data.GetEquippedItemID(EquipmentType.Socks));

        return socks != null ? Mathf.Abs(socks.BuffValue) : 0f;
    }

    /// <summary> YSW : 오늘 집주인 감지 테이블의 소리 범위(소리 크기 1당 들리는 거리)를 반환한다. 처음 한 번만 읽는다. </summary>
    private float GetSoundRangePerLoudness()
    {
        if (_soundRangePerLoudness > 0f)
        {
            return _soundRangePerLoudness;
        }

        _soundRangePerLoudness = DefaultSoundRangePerLoudness;

        EnemyDetectionTable detectionTable = Managers.Table?.EnemyDetection;
        DataManager data = Managers.Data;

        if (detectionTable == null || data == null)
        {
            return _soundRangePerLoudness;
        }

        EnemyDetectionData detection = detectionTable.GetDetectionByDay(data.GetCurrentDay());

        if (detection != null && detection.SoundRangePerLoudness > 0f)
        {
            _soundRangePerLoudness = detection.SoundRangePerLoudness;
        }

        return _soundRangePerLoudness;
    }

    #endregion ===== 양말 =====

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

    /// <summary> YSW : 현재 이동 상태에 맞는 발소리 효과음 볼륨을 반환한다. </summary>
    private float GetCurrentVolume()
    {
        if (_playerMovement.IsCrouching)
        {
            return _crouchVolume;
        }

        return _playerMovement.IsRunning ? _runVolume : _walkVolume;
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

    /// <summary> YSW : 발밑에 적에게 들리는 최대 범위(소음 크기 × 소리 범위) 원을 1초 동안 그린다. (씬 뷰에서만 보인다) </summary>
    private void DrawDebugNoise(Vector3 position, float loudness)
    {
        const int Segments = 24;
        const float Duration = 1f;

        float radius = loudness * GetSoundRangePerLoudness();
        Color color = Color.Lerp(Color.green, Color.red, loudness / Mathf.Max(_runLoudness, 0.01f));
        Vector3 previous = position + Vector3.forward * radius;

        for (int i = 1; i <= Segments; i++)
        {
            float angle = i * Mathf.PI * 2f / Segments;
            Vector3 next = position + new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * radius;

            Debug.DrawLine(previous, next, color, Duration);
            previous = next;
        }
    }

    #endregion ===== 디버그 =====
}
