#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Reflection;
using UnityEngine;

/// <summary> KYH : 개발 빌드용 치트 </summary>
public class SettlementSkip : MonoBehaviour
{
    private const KeyCode SkipKey = KeyCode.F8;
    private const string PhaseFieldName = "_currentPhase";

    private static readonly FieldInfo TycoonPhaseField = typeof(TycoonScene).GetField(PhaseFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo StealthPhaseField = typeof(StealthScene).GetField(PhaseFieldName, BindingFlags.Instance | BindingFlags.NonPublic);

    /// <summary> KYH : 게임을 시작할 때 치트 오브젝트를 만든다 </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        GameObject cheatObject = new GameObject(nameof(SettlementSkip));
        DontDestroyOnLoad(cheatObject);
        cheatObject.AddComponent<SettlementSkip>();
    }

    private void Update()
    {
        UpdateSkipInput();
    }

    /// <summary> KYH : 키를 누르면 지금 씬에 맞는 정산으로 넘긴다 </summary>
    private void UpdateSkipInput()
    {
        if (!Input.GetKeyDown(SkipKey))
        {
            return;
        }

        BaseScene scene = Managers.Scene?.CurrentScene;

        if (scene is TycoonScene tycoonScene)
        {
            SkipTycoon(tycoonScene);
        }
        else if (scene is StealthScene stealthScene)
        {
            SkipStealth(stealthScene);
        }
    }

    /// <summary> KYH : 영업 중이면 영업을 끝내고 정산 팝업을 띄운다 </summary>
    private void SkipTycoon(TycoonScene scene)
    {
        if (TycoonPhaseField == null)
        {
            CPrint.Warning("[SettlementSkip] TycoonScene 페이즈 필드 없음");
            return;
        }

        TycoonScene.Phase phase = (TycoonScene.Phase)TycoonPhaseField.GetValue(scene);

        if (phase != TycoonScene.Phase.DuringBusiness)
        {
            return;
        }

        scene.ChangeCurrentPhase();
    }

    /// <summary> KYH : 잠입 중이면 탈출 처리한다 </summary>
    private void SkipStealth(StealthScene scene)
    {
        if (StealthPhaseField == null)
        {
            CPrint.Warning("[SettlementSkip] StealthScene 페이즈 필드 없음");
            return;
        }

        StealthScene.Phase phase = (StealthScene.Phase)StealthPhaseField.GetValue(scene);

        if (phase != StealthScene.Phase.Playing)
        {
            return;
        }

        Managers.Event?.RaisePlayerEscaped();
    }
}
#endif
