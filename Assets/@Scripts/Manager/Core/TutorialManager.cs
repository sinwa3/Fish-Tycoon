using System.Collections.Generic;

/// <summary> KDH : 현재 일차와 진행 시점에 맞는 튜토리얼을 중복 없이 표시한다. </summary>
public class TutorialManager
{
    #region ===== 상태 =====

    // 이미 표시한 튜토리얼의 시점과 일차
    private readonly HashSet<(TutorialTrigger Trigger, int Day)> _shownGroups = new();

    #endregion ===== 상태 =====

    #region ===== 초기화 =====

    /// <summary> KDH : 튜토리얼 표시 기록을 초기화한다. </summary>
    public void Reset()
    {
        _shownGroups.Clear();
    }

    #endregion ===== 초기화 =====

    /// <summary> KDH : 현재 일차와 시점에 해당하는 튜토리얼을 최초 한 번만 표시한다. </summary>
    public void Show(TutorialTrigger trigger)
    {
        TutorialTable table = Managers.Table?.Tutorial;
        int day = Managers.Data?.GetCurrentDay() ?? 0;

        if (table == null || _shownGroups.Contains((trigger, day)))
        {
            return;
        }

        // 현재 시점과 일차에 해당하는 튜토리얼 페이지 조회
        TutorialData[] pages = table.GetPages(trigger, day);

        if (pages == null || pages.Length == 0)
        {
            return;
        }

        // 튜토리얼 팝업 생성
        TutorialPopup popup = Managers.UI.OpenPopup<TutorialPopup>();

        if (popup == null)
        {
            return;
        }

        // 중복 표시 방지
        _shownGroups.Add((trigger, day));

        // 영업 중 튜토리얼만 게임 일시정지
        popup.SetPages(pages, trigger == TutorialTrigger.DuringBusiness);
    }
}