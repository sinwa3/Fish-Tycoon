using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>SJY : 당일 음식 ID 리스트와 현재 조리 순서를 관리</summary>
public class CookingOrder
{
    private IReadOnlyList<int> _foodIds;
    private int _currentIndex;

    public int CurrentIndex => _currentIndex;
    public int TotalCount => _foodIds?.Count ?? 0;
    public bool HasCurrentFood => _foodIds != null && _currentIndex < _foodIds.Count;


    public int? CurrentFoodID => HasCurrentFood ? _foodIds[CurrentIndex] : null;
    public int? NextFoodID => _foodIds != null && _currentIndex + 1 < _foodIds.Count ? _foodIds[_currentIndex + 1] : null;

    ///<summary>SJY : 당일 셔플된 음식 ID 리스트를 전달받아 초기화</summary>
    public void InitializeFoodList(IReadOnlyList<int> foodIds)
    {
        if (foodIds == null)
        {
            Clear();

            CPrint.Warning("[CookingOrder] 음식 리스트가 null입니다.");

            return;
        }
    }

    ///<summary>SJY : 현재 음식의 서빙이 완료되면 다음 음식으로 이동</summary>
    public bool MoveNext()
    {
        if (!HasCurrentFood)
        {
            return false;
        }

        _currentIndex++;

        return true;
    }

    ///<summary>SJY : 영업 종료 시 주문 데이터 초기화</summary>
    public void Clear()
    {
        _foodIds = null;
        _currentIndex = 0;
    }

}
