using System;
using UnityEngine;

/// <summary> YSW : 달리기에 사용하는 스테미너를 소모하고 회복한다. </summary>
[RequireComponent(typeof(PlayerMovement))]
public class PlayerStamina : MonoBehaviour
{
    #region ===== 설정 =====

    [Header("스테미너")]
    [SerializeField] private float _maxStamina = 100f;

    [Tooltip("달리는 동안 초당 소모량")]
    [SerializeField] private float _drainPerSecond = 20f;

    [Tooltip("달리지 않을 때 초당 회복량")]
    [SerializeField] private float _regenPerSecond = 15f;

    [Tooltip("달리기를 멈춘 뒤 회복이 시작되기까지 걸리는 시간(초)")]
    [SerializeField] private float _regenDelay = 1f;

    [Header("탈진")]
    [Tooltip("스테미너를 모두 소진하면 이 비율까지 회복해야 다시 달릴 수 있다.")]
    [Range(0f, 1f)]
    [SerializeField] private float _exhaustRecoverRatio = 0.3f;

    #endregion ===== 설정 =====

    #region ===== 상태 =====

    private PlayerMovement _playerMovement;

    private float _regenDelayTimer;

    /// <summary> YSW : 현재 스테미너 </summary>
    public float CurrentStamina { get; private set; }

    /// <summary> YSW : 최대 스테미너 </summary>
    public float MaxStamina => _maxStamina;

    /// <summary> YSW : 최대치 대비 현재 스테미너 비율 (0 ~ 1) </summary>
    public float StaminaRatio => _maxStamina > 0f ? CurrentStamina / _maxStamina : 0f;

    /// <summary> YSW : 스테미너를 모두 소진해 회복 중인 상태 여부 </summary>
    public bool IsExhausted { get; private set; }

    /// <summary> YSW : 달리기가 가능한 상태인지 나타낸다. </summary>
    public bool CanRun => !IsExhausted && CurrentStamina > 0f;

    /// <summary> YSW : 스테미너가 변경될 때 (현재값, 최대값)을 전달한다. </summary>
    public event Action<float, float> OnStaminaChanged;

    #endregion ===== 상태 =====

    private void Awake()
    {
        InitializeComponents();
        InitializeStamina();
    }

    private void Update()
    {
        UpdateStamina();
        UpdateExhausted();
    }

    #region ===== 초기화 =====

    /// <summary> YSW : 스테미너 계산에 필요한 컴포넌트를 가져온다. </summary>
    private void InitializeComponents()
    {
        _playerMovement = GetComponent<PlayerMovement>();
    }

    /// <summary> YSW : 스테미너를 최대치로 채운다. </summary>
    private void InitializeStamina()
    {
        CurrentStamina = _maxStamina;
    }

    #endregion ===== 초기화 =====

    #region ===== 스테미너 처리 =====

    /// <summary> YSW : 달리는 중이면 소모하고, 아니면 회복한다. </summary>
    private void UpdateStamina()
    {
        if (_playerMovement.IsRunning)
        {
            DrainStamina();

            return;
        }

        RegenerateStamina();
    }

    /// <summary> YSW : 스테미너를 소모하고 회복 대기 시간을 초기화한다. </summary>
    private void DrainStamina()
    {
        _regenDelayTimer = _regenDelay;

        SetStamina(CurrentStamina - _drainPerSecond * Time.deltaTime);
    }

    /// <summary> YSW : 회복 대기 시간이 지나면 스테미너를 회복한다. </summary>
    private void RegenerateStamina()
    {
        if (_regenDelayTimer > 0f)
        {
            _regenDelayTimer -= Time.deltaTime;

            return;
        }

        if (CurrentStamina >= _maxStamina)
        {
            return;
        }

        SetStamina(CurrentStamina + _regenPerSecond * Time.deltaTime);
    }

    /// <summary> YSW : 소진되면 탈진 상태로, 일정 비율까지 회복되면 탈진을 해제한다. </summary>
    private void UpdateExhausted()
    {
        if (!IsExhausted && CurrentStamina <= 0f)
        {
            IsExhausted = true;

            return;
        }

        if (IsExhausted && StaminaRatio >= _exhaustRecoverRatio)
        {
            IsExhausted = false;
        }
    }

    /// <summary> YSW : 스테미너를 범위 안으로 제한해 변경하고 변경 이벤트를 알린다. </summary>
    private void SetStamina(float value)
    {
        float clampedValue = Mathf.Clamp(value, 0f, _maxStamina);

        if (Mathf.Approximately(CurrentStamina, clampedValue))
        {
            return;
        }

        CurrentStamina = clampedValue;

        OnStaminaChanged?.Invoke(CurrentStamina, _maxStamina);
    }

    #endregion ===== 스테미너 처리 =====
}
