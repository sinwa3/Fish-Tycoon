using TMPro;
using UnityEngine.UI;

/// <summary> KDH : 잠입 씬에서 분기에 따라 표시하는 종료화면 (정산) </summary>
public class StealthEndScreen : BaseScreen
{
    private enum Buttons
    {
        NextBtn,
    }

    private enum Images
    {
        Bg,
        ClearImage,
        OverImage
    }

    private enum Texts
    {
        ClearDescText,
        OverDescText
    }

    private bool _isArrested;

    protected override void OnAwake()
    {
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TMP_Text>(typeof(Texts));
    }

    protected override void OnStart()
    {
        Get<Button>(Buttons.NextBtn).onClick.AddListener(ReturnToTycoon);
        
        Managers.Sound.StopBgm();
        Managers.Input.SetCursorLock(false);
    }

    protected override void OnUpdate()
    {
    }

    public override void OnInputKey()
    {
    }
    
    /// <summary> KDH : 게임 결과에 맞는 화면을 표시한다. </summary>
    public void ShowResult(bool isArrested)
    {
        _isArrested =  isArrested;
        
        Get<Image>(Images.Bg).gameObject.SetActive(true);
        Get<Image>(Images.ClearImage).gameObject.SetActive(false);
        Get<Image>(Images.OverImage).gameObject.SetActive(false);
        
        UpdateResultImage(isArrested);

        Settle();
    }

    /// <summary> KDH : 게임 결과에 맞는 이미지를 표시한다. </summary>
    private void UpdateResultImage(bool isArrested)
    {
        if (isArrested)
        {
            Get<Image>(Images.OverImage).gameObject.SetActive(true);
            return;
        }
        
        Get<Image>(Images.ClearImage).gameObject.SetActive(true);
    }

    /// <summary> KDH : 정산한다. </summary>
    private void Settle()
    {
        if (Managers.Data == null || Managers.UI == null)
        {
            return;
        }
        
        Managers.Data.SetNextDay();
        Managers.Data.ChangeSavedGameMode(GameMode.Stealth);

        Managers.Data.ClearConsumables();

        if (!_isArrested) // 체포 되지 않았다면 생선을 보관한다.
        {
            int settledCount = Managers.Data.SettleQuickSlotFish();
            
            string clearDescStr = $"사용하지 않은 도구는 버리고 훔친 생선 {settledCount}마리를 가게에 옮겼습니다.";
            Get<TMP_Text>(Texts.ClearDescText).SetText(clearDescStr);
        }
        else // 체포되면 훔친 생선을 전부 초기화하고 만족도를 감소시킨다.
        {
            int clearedCount = Managers.Data.ClearFishes();
            int satisfactionPenalty = 100 * clearedCount;

            Managers.Data.SubtractSatisfaction(satisfactionPenalty);

            string overDescStr = $"훔친 생선 {clearedCount}마리를 몰수당하고 가게 만족도가 {satisfactionPenalty} 감소했습니다.";
            Get<TMP_Text>(Texts.OverDescText).SetText(overDescStr);
        }

        Managers.Data.SaveData();

        Managers.UI.OpenToastMessage("저장되었습니다.");
    }

    /// <summary> KDH : 타이쿤 씬으로 돌아간다. </summary>
    private void ReturnToTycoon()
    {
        Managers.UI.OpenLoadingUI(0.5f, () =>
        {
            Managers.Scene.LoadSceneWithLoading(SceneType.TycoonScene);
        });
    }
}