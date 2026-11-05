using UnityEngine.UI;

/// <summary> KDH :  </summary>
public class EndScreen : BaseScreen
{
    private enum Buttons
    {
        TitleBtn,
    }

    private enum Images
    {
        Bg,
        ClearImage,
        OverImage
    }

    protected override void OnAwake()
    {
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
    }

    protected override void OnStart()
    {
        Get<Button>(Buttons.TitleBtn).onClick.AddListener(ReturnToTitle);
        
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
    public void ShowResult(bool isGameOver)
    {
        Get<Image>(Images.Bg).gameObject.SetActive(true);
        Get<Image>(Images.ClearImage).gameObject.SetActive(false);
        Get<Image>(Images.OverImage).gameObject.SetActive(false);
        
        UpdateResultImage(isGameOver);
    }

    /// <summary> KDH : 게임 결과에 맞는 이미지를 표시한다. </summary>
    private void UpdateResultImage(bool isGameOver)
    {
        if (isGameOver)
        {
            Get<Image>(Images.OverImage).gameObject.SetActive(true);
            return;
        }
        
        Get<Image>(Images.ClearImage).gameObject.SetActive(true);
    }

    /// <summary> KDH : 세이브 데이터를 삭제하고 타이틀 씬으로 돌아간다. </summary>
    private void ReturnToTitle()
    {
        Managers.UI.OpenLoadingUI(0.5f, () =>
        {
            Managers.Data.DeleteSaveData();
            Managers.Scene.LoadScene(SceneType.TitleScene);
        });
    }
}