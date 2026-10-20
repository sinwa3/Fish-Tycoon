using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>SJY : 조리 미니게임의 상태 및 커맨드 진행 컨트롤</summary>
public class CookingController : MonoBehaviour
{
    [Header("조리 설정")]
    [SerializeField] private float _failDelay = 0.5f;
    [SerializeField] private float _servingDuration = 0.5f;

    private CookingOrder _cookingOrder;
    private CookingState _state = CookingState.Idle;

    private char[] _commands;
    private int _currentCommandIndex;

    // 임시 변수 추후에 수정 필요
    private int _orderedCount;

    public CookingState State => _state;
    public int CurrentCommandIndex => _currentCommandIndex;
    public int TotalCommandCount => _commands?.Length ?? 0;

    private bool HasOrderedFood => _cookingOrder != null && _cookingOrder.CurrentIndex < _orderedCount;
    private bool HasNextOrderedFood => _cookingOrder != null && _cookingOrder.CurrentIndex + 1 < _orderedCount;
    public int? CurrentFoodID => HasOrderedFood ? _cookingOrder.CurrentFoodID : null;
    public int? NextFoodID => HasNextOrderedFood ? _cookingOrder.NextFoodID : null;

    #region ==== 이벤트 ====

    public event Action<int> OnCommandPressed;
    public event Action<int> OnCommandFailed;
    public event Action OnCommandReset;
    public event Action<int> OnCookingCompleted;
    public event Action OnServingFinished;

    public event Action OnFoodChanged;

    #endregion ==== 이벤트 ====


    /// <summary>SJY : 영업 시작 시 당일 음식 목록 초기화</summary>
    public void Initialize(IReadOnlyList<int> foodIds)
    {
        if (foodIds == null)
        {
            CPrint.Error("[CookingController] 음식 리스트가 null입니다.");

            return;
        }

        StopAllCoroutines();

        _cookingOrder = new CookingOrder();
        _cookingOrder.InitializeFoodList(foodIds);

        _commands = null;
        _currentCommandIndex = 0;
        _state = CookingState.Idle;

        _orderedCount = 0;
        OnFoodChanged?.Invoke();
    }

    /// <summary>SJY : 손님이 주문한 음식 개수 갱신</summary>
    public void UpdateOrderedCount(int orderedCount)
    {
        if (_cookingOrder == null)
        {
            return;
        }

        if (orderedCount < 0 || orderedCount > _cookingOrder.TotalCount)
        {
            CPrint.Warning($"[CookingController] 잘못된 주문 수 : {orderedCount}");

            return;
        }

        _orderedCount = orderedCount;

        OnFoodChanged?.Invoke();
    }

    /// <summary>SJY : 현재 음식의 커맨드를 테이블에서 가져와 조리 시작</summary>
    public void StartCooking()
    {
        if (_state != CookingState.Idle || !CurrentFoodID.HasValue)
        {
            return;
        }

        int foodId = _cookingOrder.CurrentFoodID.Value;

        if (!Managers.Table.Food.Map.TryGetValue(foodId, out FoodData foodData))
        {
            CPrint.Error($"[CookingController] 음식 ID {foodId}를 찾을 수 없습니다.");

            return;
        }

        string command = foodData.Command;

        if (string.IsNullOrWhiteSpace(command))
        {
            CPrint.Error($"[CookingController] 음식 ID {foodId}의 커맨드가 비어 있습니다.");
            return;
        }

        string[] commandStrings = command.Split(',');

        char[] parsedCommands = new char[commandStrings.Length];

        for (int i = 0; i <commandStrings.Length; i++)
        {
            if (commandStrings[i].Length != 1 || "WASD".IndexOf(commandStrings[i][0]) < 0)
            {
                CPrint.Error($"[CookingController] 음식 ID {foodId}의 {i}번 커맨드가 잘못되었습니다.");

                return;
            }

            parsedCommands[i] = commandStrings[i][0];
        }

        _commands = parsedCommands;
        _currentCommandIndex = 0;
        _state = CookingState.Playing;

        OnCommandReset?.Invoke();

        CPrint.Log($"[CookingController] 현재 음식 ID : {foodId}");
        CPrint.Log($"[CookingController] 현재 음식 커맨드 : {command}");
    }

    /// <summary>SJY : 커맨드 이미지 표시용 정답 데이터를 읽어오는 메서드</summary>
    public char GetCommand(int index)
    {
        if (_commands == null || index < 0 || index >= _commands.Length)
        {
            return '\0';
        }

        return _commands[index];
    }

    /// <summary>SJY : 플레이어가 입력한 커맨드의 정답 여부 판단</summary>
    public void ProcessCommand(char command)
    {
        if (_state != CookingState.Playing || _commands == null)
        {
            return;
        }

        if (command != _commands[_currentCommandIndex])
        {
            _state = CookingState.Failed;

            OnCommandFailed?.Invoke(CurrentCommandIndex);

            StartCoroutine(Co_HandleFail());

            return;
        }

        OnCommandPressed?.Invoke(_currentCommandIndex);

        _currentCommandIndex++;

        if (_currentCommandIndex >= _commands.Length)
        {
            CompleteCooking();
        }

    }

    /// <summary>SJY : 커맨드 입력 실패 시 일정 시간 입력 제한 후 초기화용 코루틴</summary>
    private IEnumerator Co_HandleFail()
    {
        yield return new WaitForSeconds(_failDelay);

        _currentCommandIndex = 0;
        _state = CookingState.Playing;

        OnCommandReset?.Invoke();

    }

    /// <summary>SJY : 모든 커맨드 입력에 성공했을 때 조리 완료 처리</summary>
    private void CompleteCooking()
    {
        _state = CookingState.Serving;

        int foodId = _cookingOrder.CurrentFoodID.Value;

        CPrint.Log($"[CookingController] 조리 성공 음식 ID : {foodId}");

        OnCookingCompleted?.Invoke(foodId);

        StartCoroutine(Co_HandleServing());
    }

    /// <summary>SJY : 커맨드 입력 성공시 서빙 연출이되는 딜레이 코루틴</summary>
    private IEnumerator Co_HandleServing()
    {
        yield return new WaitForSeconds(_servingDuration);

        OnServingFinished?.Invoke();

        FinishServing();
    }

    /// <summary>SJY : 서빙 후 다음 요리를 위한 초기화 메서드</summary>
    public void FinishServing()
    {
        if (_state != CookingState.Serving)
        {
            return;
        }
            
        _cookingOrder.MoveNext();

        _commands = null;
        _currentCommandIndex = 0;
        _state = CookingState.Idle;

        OnFoodChanged?.Invoke();
    }

    /// <summary>SJY : 영업 종료 시 조리 데이터 초기화</summary>
    public void Clear()
    {
        StopAllCoroutines();

        _cookingOrder?.Clear();

        _commands = null;
        _currentCommandIndex = 0;
        _state = CookingState.Idle;

        _orderedCount = 0;
        OnFoodChanged?.Invoke();
    }

}
