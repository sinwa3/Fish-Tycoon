using System.Collections.Generic;
using UnityEngine;

/// <summary> KYH : 그날 준비한 메뉴를 손님 수만큼 섞어 두고, 주문 순서대로 하나씩 내준다 </summary>
public class DailyMenu
{
    // 메뉴 ID별 준비한 수량
    private readonly Dictionary<int, int> _menuCounts = new();

    // 손님 수 길이로 섞은 메뉴 ID
    private readonly List<int> _shuffledCandidates = new();

    // 지금까지 주문한 손님 수
    private int _orderedCount;

    public IReadOnlyList<int> ShuffledCandidates => _shuffledCandidates;
    public int OrderedCount => _orderedCount;

    /// <summary> KYH : 그날 팔 메뉴와 준비한 수량을 넣는다. 같은 메뉴면 수량을 더한다 </summary>
    public void AddMenu(int menuId, int count)
    {
        if (count == 0)
        {
            return;
        }

        if (count < 0)
        {
            CPrint.Warning($"[DailyMenu] Id : {menuId}의 개수가 0 미만");

            return;
        }

        _menuCounts.TryGetValue(menuId, out int menuCount);
        _menuCounts[menuId] = menuCount + count;
    }

    /// <summary> KYH : 메뉴를 손님 수 길이로 풀고 모자라면 김초밥으로 채우고 섞는다 </summary>
    public void ShuffleCandidates(int customerCount)
    {
        _shuffledCandidates.Clear();
        _orderedCount = 0;

        foreach (KeyValuePair<int, int> pair in _menuCounts)
        {
            for (int i = 0; i < pair.Value; i++)
            {
                _shuffledCandidates.Add(pair.Key);
            }
        }

        int gap = customerCount - _shuffledCandidates.Count;

        if (gap > 0)
        {
            for (int i = 0; i < gap; i++)
            {
                _shuffledCandidates.Add(CustomerTempValue.BasicSushiId);
            }
        }

        ShuffleList(_shuffledCandidates);

        if (gap < 0)
        {
            CPrint.Warning($"[DailyMenu] 음식 {_shuffledCandidates.Count}개 손님 {customerCount}명, 음식이 더 많음");

            _shuffledCandidates.RemoveRange(customerCount, -gap);
        }
    }

    /// <summary> KYH : 섞은 리스트에서 다음 메뉴를 꺼낸다. 다 꺼냈으면 김초밥 </summary>
    public int TakeNextMenu()
    {
        if (_orderedCount >= _shuffledCandidates.Count)
        {
            CPrint.Warning($"[DailyMenu] 주문 {_orderedCount}번째, 리스트 {_shuffledCandidates.Count}칸");

            return CustomerTempValue.BasicSushiId;
        }

        int menuId = _shuffledCandidates[_orderedCount];
        _orderedCount++;

        return menuId;
    }

    /// <summary> KYH : 리스트를 고르게 섞는다 </summary>
    private void ShuffleList(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }
}
