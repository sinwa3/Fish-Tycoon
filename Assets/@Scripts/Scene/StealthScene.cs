using System;
using UnityEngine;

public class StealthScene : BaseScene
{
    /// <summary> KDH : 순서 중요 </summary>
    public enum Phase
    {
        Playing, // 잠입 중
        Settlement, // 정산
    }

    #region ===== 참조 =====

    private StealthHUD _stealthHUD;

    [SerializeField] private GameObject _player;
    private PlayerMovement _playerMovement;
    public PlayerMovement PlayerMovement => _playerMovement ??= _player?.GetOrAddComponent<PlayerMovement>();
    private PlayerStamina _playerStamina;
    public PlayerStamina PlayerStamina => _playerStamina ??= _player?.GetOrAddComponent<PlayerStamina>();
    
    [SerializeField] private Camera _camera;
    public Camera Camera => _camera;
    
    [SerializeField] private PixelationTransition _pixelationTransition;
    
    [SerializeField] private HoldingSystem _holdingSystem;
    
    #endregion ===== 참조 =====
    
    #region ===== 상태 =====
    
    private Phase _currentPhase;
    
    #endregion ===== 상태 =====
    
    #region ===== 이벤트 =====

    public event Action<Vector3, float> OnNoiseMade;

    #endregion ===== 이벤트 =====
    
    protected override void OnAwake()
    {
        IsCursorLock = true;

        _stealthHUD = GetComponentInChildren<StealthHUD>(true);

        if (_stealthHUD == null)
        {
            CPrint.Error("[StealthScene] _stealthHUD not found.");
        }

        if (_camera == null)
        {
            _camera = Camera.main;
        }
        
        // 플레이어가 잡혔을 때 종료 화면 활성화 구독
        if (Managers.Event != null)
        {
            Managers.Event.OnPlayerArrested += OnArrested;
            Managers.Event.OnPlayerEscaped += OnEscaped;
        }
    }

    protected override void OnStart()
    {
        // 플레이어와 HUD에 홀딩 시스템을 연결한다.
        _player?.GetComponent<PlayerInteractor>()?.SetHoldingSystem(_holdingSystem);
        _stealthHUD?.SetHoldingSystem(_holdingSystem);
        
        InitializePhase();

        if (_pixelationTransition != null && _pixelationTransition.isActiveAndEnabled)
        {
            Managers.Input?.SetInputBlocked(this, true);
            _pixelationTransition.Play(OnEntryTransitionFinished);
        }
        else
        {
            OnEntryTransitionFinished();
        }
    }

    private void OnEntryTransitionFinished()
    {
        Managers.Input?.SetInputBlocked(this, false);
        Managers.Tutorial.Show(TutorialTrigger.StealthEntry);
    }
    
    private void OnDestroy()
    {
        if (Managers.Event != null)
        {
            Managers.Event.OnPlayerArrested -= OnArrested;
            Managers.Event.OnPlayerEscaped -= OnEscaped;
        }
    }

    protected override void OnUpdate()
    {
    }

    #region ===== 초기화 =====

    /// <summary> KDH : 페이즈 상태를 초기화한다. </summary>
    private void InitializePhase()
    {
        _currentPhase = Phase.Playing;
    }

    #endregion ===== 초기화 =====

    #region ===== 업데이트 =====

    /// <summary> KDH : 페이즈를 다음 단계로 진행시킨다. </summary>
    private void ChangeCurrentPhase()
    {
        if (_currentPhase < Phase.Settlement)
        {
            _currentPhase++;
        }

        UpdatePhaseUI();
    }

    /// <summary> KDH : 현재 페이즈에 맞춰 UI를 업데이트한다. </summary>
    private void UpdatePhaseUI()
    {
        if (_stealthHUD == null)
        {
            return;
        }

        switch (_currentPhase)
        {
            case Phase.Playing:
                _stealthHUD.SetQuickSlotVisible(true);
                break;
            case Phase.Settlement:
                _stealthHUD.SetQuickSlotVisible(false);
                break;
        }
    }

    #endregion ===== 업데이트 =====

    #region ===== 이벤트 =====

    /// <summary> KYH : 소음이 났음을 알린다. </summary>
    public void RaiseNoiseMade(Vector3 position, float loudness)
    {
        OnNoiseMade?.Invoke(position, loudness);
    }

    /// <summary> KDH : 플레이어가 잡혔을 때 종료화면을 표시한다. </summary>
    private void OnArrested()
    {
        if (_currentPhase == Phase.Settlement)
        {
            return;
        }

        ChangeCurrentPhase();
        Managers.UI.OpenScreen<StealthEndScreen>().ShowResult(true);
    }
    
    /// <summary> KDH : 플레이어가 탈출했을 때 종료화면을 표시한다. </summary>
    private void OnEscaped()
    {
        if (_currentPhase == Phase.Settlement)
        {
            return;
        }
        
        ChangeCurrentPhase();
        Managers.UI.OpenScreen<StealthEndScreen>().ShowResult(false);
    }
    
    #endregion ===== 이벤트 =====
}
