using System;

public class StealthScene : BaseScene
{
    /// <summary> KDH : 순서 중요 </summary>
    public enum Phase
    {
        Playing, // 잠입 중
        Settlement, // 정산
    }
    
    private Phase _currentPhase;
    
    private StealthHUD _stealthHUD;
    
    /// <summary> KDH : 페이즈 변경 이벤트 </summary>
    public event Action<Phase> OnPhaseChanged;
    
    protected override void OnAwake()
    {
        IsCursorLock = true;
        
        _stealthHUD = GetComponentInChildren<StealthHUD>(true);
        
        if (_stealthHUD == null)
        {
            CPrint.Error("[StealthScene] _stealthHUD not found.");
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
            case Phase.Playing: // 잠입 => 정산
                Managers.Data.SetNextDay();
                Managers.Data.ChangeSavedGameMode(GameMode.Stealth);
                
                // (임시)
                int randID = UnityEngine.Random.Range(0, Managers.Table.Fish.Map.Count);
                int randCnt = UnityEngine.Random.Range(1, 10);
                Managers.Data.TryAddFish(randID, randCnt);
                Managers.UI.OpenToastMessage($"(임시) 생선({randID})가 {randCnt}개 지급되었습니다.");
                
                Managers.Data.SaveData();
                Managers.UI.OpenToastMessage("저장되었습니다.");
                break;
            case Phase.Settlement: // 정산 => 타이쿤 씬
                Managers.Scene.LoadSceneWithLoading(SceneType.TycoonScene);
                break;
        }

        if (_currentPhase < Phase.Settlement)
        {
            _currentPhase++;
        }
        
        OnPhaseChanged?.Invoke(_currentPhase);
    }
    
    /// <summary> KDH : 페이즈 상태를 초기화하고 페이즈 변화 이벤트를 발생한다. </summary>
    private void InitializePhase()
    {
        _currentPhase = Phase.Playing;
        
        _stealthHUD?.UpdatePhaseText(_currentPhase);
    }
}
