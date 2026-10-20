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
    
    private TitleScene _titleScene;
    
    protected override void OnAwake()
    {
        Bind<Button>(typeof(Buttons));
        
        _titleScene = gameObject.FindParent<TitleScene>();
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
        Get<Button>(Buttons.StartBtn).onClick.AddListener(() => _titleScene?.PlayNewGame());

        if (Managers.Data.IsSaved)
        {
            Get<Button>(Buttons.LoadBtn).onClick.AddListener(() => _titleScene?.LoadGame());
        }
        else
        {
            Get<Button>(Buttons.LoadBtn).interactable = false;
        }
        
        Get<Button>(Buttons.OptionBtn).onClick.AddListener(() => Managers.UI.OpenToastMessage("준비 중"));
        Get<Button>(Buttons.ExitBtn).onClick.AddListener(() => Utils.QuitApp());
    }
}
