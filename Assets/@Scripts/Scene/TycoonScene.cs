using System;
using UnityEngine;

public class TycoonScene : BaseScene
{
    public enum Phase
    {
        BeforeBusiness, // 영업 전
        DuringBusiness, // 영업 중
        AfterBusiness // 영업 후
    }

    private Phase _currentPhase;

    /// <summary> KDH : 페이즈 변경 이벤트 </summary>
    public event Action<Phase> OnPhaseChanged;

    protected override void OnAwake()
    {
    }

    protected override void OnStart()
    {
        SetCurrentPhase();
    }

    protected override void OnUpdate()
    {
    }

    /// <summary> KDH : 저장되어 있는 게임모드를 기준으로 현재 페이즈를 변경하고 변경 이벤트를 호출한다. </summary>
    private void SetCurrentPhase()
    {
        _currentPhase = Managers.Data.GetSavedGameMode() switch
        {
            GameMode.Tycoon => Phase.AfterBusiness,
            GameMode.Stealth => Phase.BeforeBusiness,
            _ => Phase.BeforeBusiness
        };

        OnPhaseChanged?.Invoke(_currentPhase);
    }
}