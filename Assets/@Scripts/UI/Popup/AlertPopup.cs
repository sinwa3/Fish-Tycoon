using System;
using TMPro;
using UnityEngine.UI;

/// <summary> KDH : 사용자에게 선택을 요청하고 결과에 따라 지정된 동작을 실행하는 알림 팝업이다. </summary>
public class AlertPopup : BasePopup
{
    private enum Buttons
    {
        YesBtn,
        NoBtn
    }

    private enum Texts
    {
        TitleText,
        ContentsText,
    }

    private Action _yesAction;

    protected override void OnAwake()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));
    }

    protected override void OnStart()
    {
        Get<Button>(Buttons.YesBtn).onClick.AddListener(OnClickYes);
        Get<Button>(Buttons.NoBtn).onClick.AddListener(OnClickNo);
    }

    protected override void OnUpdate()
    {
    }

    public override void OnInputKey()
    {
        if (Managers.Input.KeyDown_Enter)
        {
            OnClickYes();
        }
        else if (Managers.Input.KeyDown_Esc)
        {
            OnClickNo();
        }
    }

    protected override void DestroyOverride()
    {
        _yesAction = null;

        Get<Button>(Buttons.YesBtn)?.onClick.RemoveAllListeners();
        Get<Button>(Buttons.NoBtn)?.onClick.RemoveAllListeners();
    }

    /// <summary> 팝업의 내용과 선택 시 실행할 동작을 설정한다. </summary>
    public void Set(string title, string content, bool showCancelButton = false, Action yesAction = null)
    {
        _yesAction = yesAction;

        Get<TMP_Text>(Texts.TitleText).text = title;
        Get<TMP_Text>(Texts.ContentsText).text = content;
        Get<Button>(Buttons.NoBtn).gameObject.SetActive(showCancelButton);
    }

    /// <summary> Yes 버튼을 선택했을 때 팝업을 닫고 지정된 동작을 실행한다. </summary>
    private void OnClickYes()
    {
        Close(() => _yesAction?.Invoke());
        Managers.Sound.PlaySfx(SoundKey.Name.SfxType.ChimeConfirm);
    }

    /// <summary> No 버튼을 선택했을 때 팝업을 닫는다. </summary>
    private void OnClickNo()
    {
        Close();
        Managers.Sound.PlaySfx(SoundKey.Name.SfxType.ChimeCancel);
    }
}