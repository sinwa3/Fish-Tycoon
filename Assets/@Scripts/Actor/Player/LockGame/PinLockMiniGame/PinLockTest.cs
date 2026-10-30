
using System.Collections;
using UnityEngine;

/// <summary>SJY : 핀 조절형 자물쇠의 락픽 수량, 내구도, 자동 교체 및 성공 판정을 테스트</summary>
public class PinLockTest : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private PinLockController _pinLockController;
    [SerializeField] private LockpickDurability _lockpickDurability;

    [Header("테스트 설정")]
    [SerializeField] private int _startingLockpickCount = 5;
    [SerializeField] private float _replacementDelay = 1f;

    private int _currentLockpickCount;
    private string _lockpickCountInput;
    private string _message;
    private float _messageEndTime;
    private Coroutine _replacementCoroutine;

    /// <summary>SJY : 테스트용 컴포넌트 참조와 초기 락픽 수량을 설정</summary>
    private void Awake()
    {
        if (_pinLockController == null)
        {
            _pinLockController = GetComponent<PinLockController>();
        }

        if (_lockpickDurability == null)
        {
            _lockpickDurability = GetComponent<LockpickDurability>();
        }

        _currentLockpickCount = Mathf.Max(0, _startingLockpickCount);
        _lockpickCountInput = _currentLockpickCount.ToString();
        _message = "";
    }

    /// <summary>SJY : 자물쇠 해제 성공과 락픽 파손 이벤트를 구독</summary>
    private void Start()
    {
        if (_pinLockController == null || _lockpickDurability == null)
        {
            CPrint.Warning("[PinLockTest] 필요한 컴포넌트가 연결되지 않았습니다.");

            return;
        }

        _pinLockController.OnUnlockCompleted += HandleUnlockCompleted;
        _pinLockController.OnLockpickBroken += HandleLockpickBroken;
    }

    /// <summary>SJY : 숫자 1키로 시작하고 ESC키로 미니게임을 종료</summary>
    private void Update()
    {
        if (_pinLockController == null)
        {
            return;
        }

        if (Managers.Input.KeyDown_Esc && _pinLockController.IsPlaying)
        {
            StopTest();
        }

        if (Managers.Input.KeyDown_1 && !_pinLockController.IsPlaying)
        {
            StartTest();
        }
    }

    /// <summary>SJY : Game 뷰에 테스트용 락픽 수량, 핀 진행도 및 내구도를 표시</summary>
    private void OnGUI()
    {
        if (_pinLockController == null || _lockpickDurability == null)
        {
            return;
        }

        GUI.Box(new Rect(20f, 20f, 350f, 290f), "핀 조절형 자물쇠 테스트");

        GUI.enabled = !_pinLockController.IsPlaying;

        GUI.Label(new Rect(35f, 50f, 115f, 25f), "락픽 수량 설정");
        _lockpickCountInput = GUI.TextField(new Rect(150f, 50f, 70f, 25f), _lockpickCountInput);

        if (GUI.Button(new Rect(230f, 50f, 125f, 25f), "수량 적용"))
        {
            ApplyLockpickCount();
        }

        GUI.enabled = true;

        GUI.Label(new Rect(35f, 85f, 310f, 25f), $"현재 소지 락픽 : {_currentLockpickCount}개");
        GUI.Label(new Rect(35f, 115f, 310f, 25f), $"현재 선택 핀 : {Mathf.Min(_pinLockController.CurrentPinIndex + 1, _pinLockController.TotalPinCount)} / {_pinLockController.TotalPinCount}");
        GUI.Label(new Rect(35f, 145f, 310f, 25f), $"고정 완료 핀 : {_pinLockController.FixedPinCount}개");

        float currentDurability = _pinLockController.IsPlaying ? _lockpickDurability.CurrentDurability : 0f;
        float maxDurability = Mathf.Max(1f, _lockpickDurability.MaxDurability);
        float durabilityRatio = Mathf.Clamp01(currentDurability / maxDurability);

        GUI.Label(new Rect(35f, 175f, 310f, 25f), $"현재 락픽 내구도 : {currentDurability:F0} / {maxDurability:F0}");

        GUI.enabled = false;
        GUI.HorizontalSlider(new Rect(35f, 208f, 300f, 20f), durabilityRatio, 0f, 1f);
        GUI.enabled = true;

        string status = "대기 중";

        if (_pinLockController.IsReplacing)
        {
            status = "락픽 교체 중";
        }
        else if (_pinLockController.IsUnlocked)
        {
            status = "잠금 해제 성공";
        }
        else if (_pinLockController.IsPlaying)
        {
            status = "미니게임 진행 중";
        }

        GUI.Label(new Rect(35f, 232f, 310f, 25f), $"상태 : {status}");

        if (!_pinLockController.IsPlaying)
        {
            if (GUI.Button(new Rect(35f, 263f, 145f, 30f), "미니게임 시작"))
            {
                StartTest();
            }
        }
        else
        {
            if (GUI.Button(new Rect(35f, 263f, 145f, 30f), "미니게임 종료"))
            {
                StopTest();
            }
        }

        GUI.Label(new Rect(190f, 268f, 160f, 25f), "1 : 시작 / ESC : 종료");

        if (Time.unscaledTime < _messageEndTime)
        {
            GUI.Box(new Rect(20f, 320f, 350f, 65f), _message);
        }
    }

    #region ==== 테스트 제어 ====

    /// <summary>SJY : 입력한 락픽 수량을 테스트용 소지 개수에 적용</summary>
    private void ApplyLockpickCount()
    {
        if (!int.TryParse(_lockpickCountInput, out int count) || count < 0)
        {
            ShowMessage("0 이상의 정수를 입력해 주세요.", 2f);

            return;
        }

        _currentLockpickCount = count;

        ShowMessage($"락픽 {_currentLockpickCount}개 설정 완료", 1.5f);
    }

    /// <summary>SJY : 락픽 보유 여부를 확인하고 미니게임을 시작</summary>
    private void StartTest()
    {
        if (_pinLockController == null || _pinLockController.IsPlaying)
        {
            return;
        }

        if (_currentLockpickCount <= 0)
        {
            ShowMessage("소지한 락픽이 없습니다.", 2f);

            return;
        }

        _pinLockController.StartMiniGame();

        if (_pinLockController.IsPlaying)
        {
            ShowMessage("미니게임 시작!\nA/D : 선택, S : 튕기기, 좌클릭 : 고정", 2f);
        }
    }

    /// <summary>SJY : 교체 연출을 중단하고 미니게임을 종료</summary>
    private void StopTest()
    {
        if (_replacementCoroutine != null)
        {
            StopCoroutine(_replacementCoroutine);
            _replacementCoroutine = null;
        }

        _pinLockController.StopMiniGame();

        ShowMessage("미니게임 종료", 1.5f);
    }

    #endregion ==== 테스트 제어 ====

    #region ==== 이벤트 처리 ====

    /// <summary>SJY : 모든 핀 고정 성공 시 사용 중인 락픽 한 개를 소모</summary>
    private void HandleUnlockCompleted()
    {
        _currentLockpickCount = Mathf.Max(0, _currentLockpickCount - 1);

        CPrint.Log($"[PinLockTest] 잠금 해제 성공 / 남은 락픽 : {_currentLockpickCount}");

        _pinLockController.StopMiniGame();

        ShowMessage("잠금 해제 성공!\n락픽 1개 소모", 2f);
    }

    /// <summary>SJY : 락픽 파손 시 소모 처리하고 여분을 확인하여 자동 교체 또는 종료</summary>
    private void HandleLockpickBroken()
    {
        _currentLockpickCount = Mathf.Max(0, _currentLockpickCount - 1);

        CPrint.Log($"[PinLockTest] 락픽 파손 / 남은 락픽 : {_currentLockpickCount}");

        if (_currentLockpickCount <= 0)
        {
            _pinLockController.StopMiniGame();
            ShowMessage("락픽 파손!\n남은 락픽이 없어 미니게임 종료", 3f);

            return;
        }

        ShowMessage("락픽 파손!\n새 락픽으로 교체 중...", _replacementDelay);

        _replacementCoroutine = StartCoroutine(ReplaceLockpickRoutine());
    }

    #endregion ==== 이벤트 처리 ====

    #region ==== 락픽 교체 ====

    /// <summary>SJY : 교체 대기 후 내구도와 모든 핀을 초기화하여 처음부터 재개</summary>
    private IEnumerator ReplaceLockpickRoutine()
    {
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, _replacementDelay));

        if (_pinLockController != null && _pinLockController.IsPlaying && _pinLockController.IsReplacing)
        {
            _pinLockController.ReplaceLockpick();

            ShowMessage("새 락픽으로 교체 완료!\n1번 핀부터 다시 시작", 2f);
        }

        _replacementCoroutine = null;
    }

    #endregion ==== 락픽 교체 ====

    /// <summary>SJY : 테스트 화면에 지정된 시간 동안 메시지를 표시</summary>
    private void ShowMessage(string message, float duration)
    {
        _message = message;
        _messageEndTime = Time.unscaledTime + duration;
    }

    /// <summary>SJY : 테스트 종료 시 이벤트 구독을 해제</summary>
    private void OnDestroy()
    {
        if (_pinLockController == null)
        {
            return;
        }

        _pinLockController.OnUnlockCompleted -= HandleUnlockCompleted;
        _pinLockController.OnLockpickBroken -= HandleLockpickBroken;
    }
}
