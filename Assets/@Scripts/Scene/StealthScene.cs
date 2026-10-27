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
        // (임시)
        int randID = UnityEngine.Random.Range(0, Managers.Table.Fish.Map.Count);
        int randCnt = UnityEngine.Random.Range(1, 10);
        if (Managers.Data.TryAddFishToQuickSlot(randID, randCnt))
        {
            Managers.UI.OpenToastMessage($"(임시) 생선({Managers.Table.Fish.Map[randID].Name}) {randCnt}개 지급되었습니다.");
        }

        InitializePhase();
    }

    protected override void OnUpdate()
    {
    }

    #region ===== 초기화 =====

    /// <summary> KDH : 페이즈 상태를 초기화하고 페이즈 변화 이벤트를 발생한다. </summary>
    private void InitializePhase()
    {
        _currentPhase = Phase.Playing;

        _stealthHUD?.UpdatePhaseText(_currentPhase);
    }

    #endregion ===== 초기화 =====

    #region ===== 업데이트 =====

    /// <summary> KDH : 페이즈를 다음 단계로 진행시킨다. </summary>
    public void ChangeCurrentPhase()
    {
        switch (_currentPhase)
        {
            case Phase.Playing: // 잠입 => 정산
                Managers.Data.SetNextDay();
                Managers.Data.ChangeSavedGameMode(GameMode.Stealth);

                Managers.Data.ClearConsumables();
                Managers.Data.SettleQuickSlotFish();

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

        UpdatePhaseUI();
    }

    /// <summary> KDH : 현재 페이즈에 맞춰 UI를 업데이트한다. </summary>
    private void UpdatePhaseUI()
    {
        if (_stealthHUD == null)
        {
            return;
        }

        _stealthHUD.UpdatePhaseText(_currentPhase);

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
}