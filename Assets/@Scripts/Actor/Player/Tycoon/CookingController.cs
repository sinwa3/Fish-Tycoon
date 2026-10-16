using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>SJY : 조리 미니게임의 상태 및 커맨드 진행 컨트롤</summary>
public class CookingController : MonoBehaviour
{
    [SerializeField] private float _failDelay = 0.5f;

    private CookingOrder _cookingOrder;
    private CookingState _state = CookingState.Idle;

    private char[] _commands;
    private int _currentCommandIndex;

    public CookingState State => _state;
    public int CurrentCommandIndex => _currentCommandIndex;
    public int TotalCommandCount => _commands?.Length ?? 0;

    public event Action<int> OnCommandPressed;
    public event Action<int> OnCommandFailed;
    public event Action OnCommandReset;
    public event Action<int> OnCookingCompleted;

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

    }

    /// <summary>SJY : 현재 음식의 커맨드를 테이블에서 가져와 조리 시작</summary>
    public void StartCooking()
    {
        if (_state != CookingState.Idle || _cookingOrder == null || !_cookingOrder.HasCurrentFood)
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

    /// <summary>SJY : 플레이어가 입력한 커맨드의 정답 여부 판단</summary>
    public void ProcessCommand(char command)
    {
        if (_state != CookingState.Playing || _commands == null)
        {
            return;
        }

        if (command != _commands[_currentCommandIndex =])
        {
            _state = CookingState.Failed;

            OnCommandFailed?.Invoke(CurrentCommandIndex);

            StartCoroutine(HandleFail());

            return;
        }

        OnCommandPressed?.Invoke(_currentCommandIndex);

        _currentCommandIndex++;

        if (_currentCommandIndex >= _commands.Length)
        {
            CompleteCooking();
        }

    }


    private IEnumerator HandleFail()
    {

    }

    private void CompleteCooking()
    {

    }

    public void Clear()
    {

    }


}
