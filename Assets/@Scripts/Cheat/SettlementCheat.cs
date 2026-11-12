#if UNITY_EDITOR || DEVELOPMENT_BUILD

/// <summary> 영업이나 잠입을 끝내고 정산으로 넘긴다 </summary>
public class SettlementCheat : BaseCheat
{
    public override string Title => "정산";

    public override bool IsAvailable => IsDuringBusiness || IsStealthPlaying;

    private static bool IsDuringBusiness => IsTycoonPhase(TycoonScene.Phase.DuringBusiness);
    private static bool IsStealthPlaying => IsStealthPhase(StealthScene.Phase.Playing);

    protected override void OnInitialize()
    {
        if (!CanReadPhase)
        {
            CPrint.Warning("[SettlementCheat] 씬 페이즈 필드 없음");
        }
    }

    public override void DrawButtons()
    {
        if (IsDuringBusiness)
        {
            DrawButton("영업 끝내기", EndBusiness);

            return;
        }

        DrawButton("탈출 정산", Escape);
        DrawButton("체포 정산", Arrest);
    }

    /// <summary> 영업을 끝내고 정산 팝업을 띄운다 </summary>
    private void EndBusiness()
    {
        if (!IsDuringBusiness)
        {
            return;
        }

        Window.Close();
        ((TycoonScene)Managers.Scene.CurrentScene).ChangeCurrentPhase();
    }

    /// <summary> 탈출한 것으로 정산한다 </summary>
    private void Escape()
    {
        if (!IsStealthPlaying)
        {
            return;
        }

        Window.Close();
        Managers.Event?.RaisePlayerEscaped();
    }

    /// <summary> 잡힌 것으로 정산한다 </summary>
    private void Arrest()
    {
        if (!IsStealthPlaying)
        {
            return;
        }

        Window.Close();
        Managers.Event?.RaisePlayerArrested();
    }
}
#endif
