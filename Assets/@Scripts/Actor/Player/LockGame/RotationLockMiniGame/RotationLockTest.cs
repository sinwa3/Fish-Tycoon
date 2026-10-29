using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>SJY : 자물쇠 미니게임 테스트</summary>
public class RotationLockTest : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private RotationLockController _rotationLockController;
    [SerializeField] private LockpickDurability _lockpickDurability;

    [Header("테스트 설정")]
    [SerializeField] private int _startingLockpickCount = 3;
    [SerializeField] private float _replacementDelay = 0.5f;

    private int _currentLockpickCount;
    private string _lockpickCountInput;
    private string _message;
    private float _messageEndTime;
    private Coroutine _replacementCoroutine;

    /// <summary>SJY : 테스트에 사용할 컴포넌트 참조 초기화</summary>
    private void Awake()
    {
        if (_rotationLockController == null)
        {
            _rotationLockController = GetComponent<RotationLockController>();
        }

        if (_lockpickDurability == null)
        {
            _lockpickDurability = GetComponent<LockpickDurability>();
        }

        _currentLockpickCount = Mathf.Max(0, _startingLockpickCount);
        _lockpickCountInput = _currentLockpickCount.ToString();
        _message = "";
    }

    /// <summary>SJY : 성공 이벤트를 구독하고 테스트 미니게임을 시작</summary>
    void Start()
    {
        if (_rotationLockController == null || _lockpickDurability == null)
        {
            return;
        }

        _rotationLockController.OnUnlockCompleted += HandleUnlockCompleted;
        _rotationLockController.OnLockpickBroken += HandleLockpickBroken;
        //_rotationLockController.StartMiniGame();
    }

    /// <summary>SJY : ESC로 종료, 숫자 1로 재시작 (테스트 용)</summary>
    void Update()
    {
        if (_rotationLockController == null)
        {
            return;
        }

        if (Managers.Input.KeyDown_Esc)
        {
            _rotationLockController.StopMiniGame();
        }

        if (Managers.Input.KeyDown_1)
        {
            _rotationLockController.StartMiniGame();
        }

    }

    #region 테스트용 연출

    /// <summary>SJY : Game 뷰에 테스트용 락픽 수량과 내구도 UI를 표시</summary>
    private void OnGUI()
    {
        if (_rotationLockController == null || _lockpickDurability == null)
        {
            return;
        }

        GUI.Box(new Rect(20f, 20f, 340f, 260f), "회전형 자물쇠 테스트");

        GUI.Label(new Rect(35f, 55f, 120f, 25f), "락픽 개수 설정");

        GUI.enabled = !_rotationLockController.IsPlaying;
        _lockpickCountInput = GUI.TextField(new Rect(155f, 55f, 75f, 25f), _lockpickCountInput);

        if (GUI.Button(new Rect(240f, 55f, 105f, 25f), "수량 적용"))
        {
            ApplyLockpickCount();
        }

        GUI.enabled = true;

        GUI.Label(new Rect(35f, 95f, 300f, 25f), $"현재 소지 락픽 : {_currentLockpickCount}개");

        float currentDurability = _rotationLockController.IsPlaying ? _lockpickDurability.CurrentDurability : 0f;
        float maxDurability = Mathf.Max(1f, _lockpickDurability.MaxDurability);
        float durabilityRatio = Mathf.Clamp01(currentDurability / maxDurability);

        GUI.Label(new Rect(35f, 125f, 300f, 25f), $"현재 락픽 내구도 : {currentDurability:F0} / {maxDurability:F0}");

        GUI.enabled = false;
        GUI.HorizontalSlider(new Rect(35f, 158f, 300f, 20f), durabilityRatio, 0f, 1f);
        GUI.enabled = true;

        string status = "대기 중";

        if (_rotationLockController.IsReplacing)
        {
            status = "락픽 교체 중";
        }
        else if (_rotationLockController.IsUnlocked)
        {
            status = "잠금 해제 성공";
        }
        else if (_rotationLockController.IsPlaying)
        {
            status = "자물쇠 해제 시도 중";
        }

        GUI.Label(new Rect(35f, 185f, 300f, 25f), $"상태 : {status}");

        if (!_rotationLockController.IsPlaying)
        {
            if (GUI.Button(new Rect(35f, 220f, 145f, 30f), "미니게임 시작"))
            {
                StartTest();
            }
        }
        else
        {
            if (GUI.Button(new Rect(35f, 220f, 145f, 30f), "미니게임 종료"))
            {
                StopTest();
            }
        }

        GUI.Label(new Rect(195f, 226f, 140f, 25f), "1 : 시작 / ESC : 종료");

        if (Time.unscaledTime < _messageEndTime)
        {
            GUI.Box(new Rect(20f, 290f, 340f, 65f), _message);
        }
    }

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
        if (_rotationLockController.IsPlaying)
        {
            return;
        }

        if (_currentLockpickCount <= 0)
        {
            ShowMessage("소지한 락픽이 없습니다.", 2f);
            return;
        }

        _rotationLockController.StartMiniGame();

        if (_rotationLockController.IsPlaying)
        {
            ShowMessage("미니게임 시작", 1f);
        }
    }

    /// <summary>SJY : 진행 중인 교체 연출을 중단하고 테스트 미니게임을 종료</summary>
    private void StopTest()
    {
        if (_replacementCoroutine != null)
        {
            StopCoroutine(_replacementCoroutine);
            _replacementCoroutine = null;
        }

        _rotationLockController.StopMiniGame();
        ShowMessage("미니게임 종료", 1.5f);
    }

    /// <summary>SJY : 잠금 해제 성공 시 현재 사용한 락픽 한 개를 소모</summary>
    private void HandleUnlockCompleted()
    {
        _currentLockpickCount = Mathf.Max(0, _currentLockpickCount - 1);

        CPrint.Log($"[RotationLockTest] 자물쇠 해제 성공 / 남은 락픽 : {_currentLockpickCount}");

        _rotationLockController.StopMiniGame();
        ShowMessage("잠금 해제 성공!\n락픽 1개 소모", 2f);
    }

    /// <summary>SJY : 락픽 파손 시 한 개를 소모하고 여분이 있다면 자동 교체를 시작</summary>
    private void HandleLockpickBroken()
    {
        _currentLockpickCount = Mathf.Max(0, _currentLockpickCount - 1);

        CPrint.Log($"[RotationLockTest] 락픽 파손 / 남은 락픽 : {_currentLockpickCount}");

        if (_currentLockpickCount <= 0)
        {
            _rotationLockController.StopMiniGame();
            ShowMessage("락픽 파손!\n남은 락픽이 없어 미니게임 종료", 3f);
            return;
        }

        ShowMessage("락픽 파손!\n새 락픽으로 교체 중...", _replacementDelay);

        _replacementCoroutine = StartCoroutine(ReplaceLockpickRoutine());
    }

    /// <summary>SJY : 교체 연출 대기 후 새로운 락픽으로 미니게임을 재개</summary>
    private IEnumerator ReplaceLockpickRoutine()
    {
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, _replacementDelay));

        if (_rotationLockController != null && _rotationLockController.IsPlaying && _rotationLockController.IsReplacing)
        {
            _rotationLockController.ReplaceLockpick();
            ShowMessage("새 락픽으로 교체 완료!", 2f);
        }

        _replacementCoroutine = null;
    }

    /// <summary>SJY : 테스트 화면에 메시지를 지정된 시간 동안 표시</summary>
    private void ShowMessage(string message, float duration)
    {
        _message = message;
        _messageEndTime = Time.unscaledTime + duration;
    }
    #endregion

    /// <summary>SJY : 테스트 종료 시 성공 이벤트 구독 해제</summary>
    private void OnDestroy()
    {
        if (_rotationLockController != null)
        {
            _rotationLockController.OnUnlockCompleted -= HandleUnlockCompleted;
            _rotationLockController.OnLockpickBroken -= HandleLockpickBroken;
        }
    }
}
