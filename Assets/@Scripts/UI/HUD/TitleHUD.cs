using UnityEngine.UI;

public class TitleHUD : BaseHUD
{
    public enum Buttons
    {
        StartBtn,
        LoadBtn,
        OptionBtn,
        ExitBtn,
    }
    
    protected override void OnAwake()
    {
        Bind<Button>(typeof(Buttons));
    }

    protected override void OnStart()
    {
        InitializeMenuButtons();
    }

    protected override void OnUpdate()
    {
    }
    
    /// <summary> KDH : 메뉴 리스트의 버튼 클릭 이벤트를 초기화한다. </summary>
    private void InitializeMenuButtons()
    {
        Get<Button>(Buttons.StartBtn).onClick.AddListener(() => Managers.Scene.LoadSceneWithLoading(SceneType.TycoonScene));
        Get<Button>(Buttons.LoadBtn).onClick.AddListener(() => Managers.Scene.LoadSceneWithLoading(SceneType.TycoonScene));
        Get<Button>(Buttons.OptionBtn).onClick.AddListener(() => Managers.UI.OpenToastMessage("준비 중"));
        Get<Button>(Buttons.ExitBtn).onClick.AddListener(() => Utils.QuitApp());
    }
}
