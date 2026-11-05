using UnityEngine;
using UnityEngine.UI;

/// <summary> KDH : 타이쿤 씬, 잠입 씬에서 Esc 누를 시 활성화되는 일시정지 팝업이다. </summary>
public class PausePopup : BasePopup
{
    public enum Buttons
    {
        CloseBtn,
        ResumeBtn,
        ToTitleBtn,
        OptionBtn,
        ExitBtn
    }
    
    protected override void OnAwake()
    {
        Bind<Button>(typeof(Buttons));
    }

    protected override void OnStart()
    {
        Managers.Sound.PlaySfx(SoundKey.Name.SfxType.ChimePopup);
        
        Get<Button>(Buttons.CloseBtn).onClick.AddListener(OnClickExit);
        Get<Button>(Buttons.ResumeBtn).onClick.AddListener(OnClickExit);
        Get<Button>(Buttons.ToTitleBtn).onClick.AddListener(OnClickToTitle);
        Get<Button>(Buttons.OptionBtn).onClick.AddListener(() => Managers.UI.OpenToastMessage("준비 중 입니다."));
        Get<Button>(Buttons.ExitBtn).onClick.AddListener(Utils.QuitApp);
    }

    protected override void OnUpdate()
    {
    }

    public override void OnInputKey()
    {
        if (Managers.Input.KeyDown_Esc)
        {
            OnClickExit();
        }
    }

    protected override void OnOpened()
    {
        base.OnOpened();
        
        Time.timeScale = 0f;
        Managers.Event.RaisePause();
    }

    protected override void DestroyOverride()
    {
        Time.timeScale = 1f;
    }

    /// <summary> KDH : 타이틀 이동 버튼 클릭했을 시 페이드 전환 후 전환한다. </summary>
    private void OnClickToTitle()
    {
        Managers.UI.OpenLoadingUI(1f, () =>
        {
            Managers.Scene.LoadScene(SceneType.TitleScene);
        });
    }
    
    /// <summary> KDH : 계속하기/닫기 버튼을 선택했을 때 팝업을 닫는다. </summary>
    private void OnClickExit()
    {
        Close();
        
        Managers.Sound.PlaySfx(SoundKey.Name.SfxType.ChimeConfirm);
    }
}
