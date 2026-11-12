using System.Collections.Generic;

/// <summary> KDH : 튜토리얼을 표시할 게임 진행 시점이다. </summary>
public enum TutorialTrigger
{
    BeforeBusiness,
    DuringBusiness,
    AfterBusiness,
    StealthEntry,
}

/// <summary> KDH : 튜토리얼 한 페이지의 표시 정보와 노출 조건을 관리한다. </summary>
public class TutorialData : TableData
{
    [CsvField("시점")]
    public string Trigger;

    [CsvField("일차")]
    public int Day;

    [CsvField("페이지")]
    public int Page;

    [CsvField("제목")]
    public string Title;

    [CsvField("내용")]
    public string Body;

    [CsvField("이미지")]
    public string ImageName;

    [CsvField("하이라이트")]
    public string HighlightPaths;

    /// <summary> KDH : CSV에 저장된 튜토리얼 시점을 열거형으로 변환한다. </summary>
    public bool TryGetTrigger(out TutorialTrigger trigger)
    {
        return Utils.TryParseEnum(Trigger, out trigger);
    }
}

/// <summary> KDH : 튜토리얼 페이지를 시점과 일차별로 분류하고 순서대로 제공한다. </summary>
public class TutorialTable : TableLoader<TutorialData>
{
    protected override string TableName => "TutorialTable";

    #region ===== 상태 =====

    private readonly Dictionary<(TutorialTrigger Trigger, int Day), TutorialData[]> _pagesByGroup = new();

    #endregion ===== 상태 =====

    #region ===== 초기화 =====

    /// <summary> KDH : 테이블 로딩 후 튜토리얼 페이지를 시점과 일차별로 캐싱한다. </summary>
    protected override void OnLoaded()
    {
        base.OnLoaded();

        _pagesByGroup.Clear();

        // 시점과 일차별로 페이지 분류
        Dictionary<(TutorialTrigger Trigger, int Day), List<TutorialData>> groupedPages = new();

        foreach (KeyValuePair<int, TutorialData> pair in TableMap)
        {
            TutorialData page = pair.Value;

            if (!page.TryGetTrigger(out TutorialTrigger trigger) || page.Page <= 0)
            {
                continue;
            }

            var key = (trigger, page.Day);

            if (!groupedPages.TryGetValue(key, out List<TutorialData> pages))
            {
                pages = new List<TutorialData>();
                groupedPages.Add(key, pages);
            }

            pages.Add(page);
        }

        // 페이지 번호와 ID 순서로 정렬 후 배열 캐싱
        foreach (KeyValuePair<(TutorialTrigger Trigger, int Day), List<TutorialData>> pair in groupedPages)
        {
            List<TutorialData> pages = pair.Value;

            pages.Sort((a, b) =>
            {
                int result = a.Page.CompareTo(b.Page);

                return result != 0 ? result : a.ID.CompareTo(b.ID);
            });

            _pagesByGroup.Add(pair.Key, pages.ToArray());
        }
    }

    #endregion ===== 초기화 =====

    /// <summary> KDH : 지정한 시점과 일차에 해당하는 튜토리얼 페이지를 순서대로 가져온다. </summary>
    public TutorialData[] GetPages(TutorialTrigger trigger, int day)
    {
        return _pagesByGroup.GetValueOrDefault((trigger, day));
    }
}