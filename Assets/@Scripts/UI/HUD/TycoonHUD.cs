using System;
using TMPro;

public class TycoonHUD : BaseHUD
{
    public enum Texts
    {
        GoldValueText
    }
    
    protected override void OnAwake()
    {
        Bind<TMP_Text>(typeof(Texts));
    }

    protected override void OnStart()
    {
        UpdateGoldText();

        SubscribeGoldChanged();
    }

    protected override void OnUpdate()
    {
    }

    public override void OnInputKey()
    {
        base.OnInputKey();

        if (Managers.Input.KeyDown_Space)
        {
            Managers.Data.AddGold(500);
        }
    }

    private void OnDestroy()
    {
        UnsubscribeGoldChanged();
    }

    /// <summary> KDH : 현재 골드 값을 UI에 업데이트 한다. </summary>
    private void UpdateGoldText()
    {
        Get<TMP_Text>(Texts.GoldValueText)?.SetText(Managers.Data.GetCurrentGold().ToString("N0"));
    }

    #region ===== 이벤트 =====

    /// <summary> KDH : 골드 변화시 UI 업데이트를 구독 </summary>
    private void SubscribeGoldChanged()
    {
        if (Managers.Data != null)
        {
            Managers.Data.OnGoldChanged += UpdateGoldText;
        }
    }
    
    /// <summary> KDH : 골드 변화시 UI 업데이트를 구독해제 </summary>
    private void UnsubscribeGoldChanged()
    {
        if (Managers.Data != null)
        {
            Managers.Data.OnGoldChanged -= UpdateGoldText;
        }
    }

    #endregion ===== 이벤트 =====
    
    
}
