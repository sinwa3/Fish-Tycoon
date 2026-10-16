using System;

public class TycoonScene : BaseScene
{
    /// <summary> KDH : 순서 중요 </summary>
    public enum Phase
    {
        BeforeBusiness, // 영업 전
        DuringBusiness, // 영업 중
        AfterBusiness, // 영업 후
    }

    private Phase _currentPhase;

    private TycoonHUD _tycoonHUD;
    
    /// <summary> KDH : 페이즈 변경 이벤트 </summary>
    public event Action<Phase> OnPhaseChanged;

    protected override void OnAwake()
    {
        _tycoonHUD = gameObject.GetComponentInChildren<TycoonHUD>(true);

        if (_tycoonHUD == null)
        {
            CPrint.Error("[TycoonScene] _tycoonHUD not found.");
        }
    }

    protected override void OnStart()
    {
        InitializePhase();
    }

    protected override void OnUpdate()
    {
    }

    /// <summary> KDH : 페이즈를 다음 단계로 진행시킨다. </summary>
    public void ChangeCurrentPhase()
    {
        switch (_currentPhase)
        {
            case Phase.DuringBusiness: // 영업 중 => 영업 후
                Managers.Data.ChangeSavedGameMode(GameMode.Tycoon);
                Managers.Data.Save();
                Managers.UI.OpenToastMessage("저장되었습니다.");
                break;
            case Phase.AfterBusiness: // 잠입 씬 전환
                Managers.Scene.LoadSceneWithLoading(SceneType.StealthScene);
                break;
        }

        if (_currentPhase < Phase.AfterBusiness)
        {
            _currentPhase++;
        }
        
        OnPhaseChanged?.Invoke(_currentPhase);
    }
    
    /// <summary> KDH : 저장되어 있는 게임모드를 기준으로 현재 페이즈를 초기화하고 해당 정보를 HUD에 업데이트한다. </summary>
    private void InitializePhase()
    {
        GameMode gamemode = Managers.Data.GetSavedGameMode();
        
        _currentPhase = gamemode switch
        {
            GameMode.Tycoon => Phase.AfterBusiness,
            GameMode.Stealth => Phase.BeforeBusiness,
            _ => throw new ArgumentOutOfRangeException()
        };

        _tycoonHUD.UpdatePhaseText(_currentPhase);
    }
}