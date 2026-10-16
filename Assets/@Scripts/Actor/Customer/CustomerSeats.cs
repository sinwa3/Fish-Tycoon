using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary> KYH : 손님 좌석을 빈 자리 중 무작위로 배정하고 반납받으며 만석 여부를 알린다 </summary>
public class CustomerSeats : MonoBehaviour
{
    // 이 컴포넌트는 Seats 오브젝트에 붙이고, 자식은 Seat_0 ~ Seat_3 이름으로 둔다
    private const string SeatNamePrefix = "Seat_";
    private const int SeatCount = 4;

    #region ===== 좌석 =====

    private readonly List<Transform> _seats = new();
    private readonly List<CustomerController> _occupants = new();

    // 무작위 배정 때 빈 좌석 번호를 모으는 임시 목록 (매번 new 하지 않으려고 재사용)
    private readonly List<int> _emptySeatIndices = new();

    #endregion ===== 좌석 =====

    #region ===== 만석 =====

    private bool _isFull;

    public bool IsFull => _isFull;

    /// <summary> KYH : 만석 여부가 바뀔 때 발행 (true = 만석). 스폰의 입장 타이머 정지용 </summary>
    public event Action<bool> OnFullChanged;

    #endregion ===== 만석 =====

    private void Awake()
    {
        InitializeSeats();
    }

    #region ===== 초기화 =====

    /// <summary> KYH : 자식에서 Seat_0 ~ Seat_3를 찾아 좌석 목록을 만든다 </summary>
    private void InitializeSeats()
    {
        for (int i = 0; i < SeatCount; i++)
        {
            Transform seatTransform = gameObject.FindChild<Transform>(SeatNamePrefix + i);

            _seats.Add(seatTransform);
            _occupants.Add(null);
        }
    }

    #endregion ===== 초기화 =====

    #region ===== 배정과 반납 =====

    /// <summary> KYH : 빈 좌석 하나를 무작위로 골라 손님에게 배정한다. 만석이면 false </summary>
    public bool TryAssignSeat(CustomerController customer, out int seatIndex)
    {
        _emptySeatIndices.Clear();

        for (int i = 0; i < _occupants.Count; i++)
        {
            if (_occupants[i] == null)
            {
                _emptySeatIndices.Add(i);
            }
        }

        if (_emptySeatIndices.Count == 0)
        {
            seatIndex = -1;

            return false;
        }

        int randomIndex = UnityEngine.Random.Range(0, _emptySeatIndices.Count);
        seatIndex = _emptySeatIndices[randomIndex];

        _occupants[seatIndex] = customer;
        RefreshFull();

        return true;
    }

    /// <summary> KYH : 손님이 쓰던 좌석을 비운다 </summary>
    public void ReleaseSeat(int seatIndex)
    {
        if (seatIndex < 0 || seatIndex >= _seats.Count)
        {
            CPrint.Warning($"[CustomerSeats] [{seatIndex}] 인덱스 범위 벗어남");

            return;
        }

        _occupants[seatIndex] = null;
        RefreshFull();
    }

    /// <summary> KYH : 좌석 위치를 가져온다 </summary>
    public Transform GetSeat(int seatIndex)
    {
        if (seatIndex < 0 || seatIndex >= _seats.Count)
        {
            CPrint.Warning($"[CustomerSeats] [{seatIndex}] 인덱스 범위 벗어남");

            return null;
        }

        return _seats[seatIndex];
    }

    #endregion ===== 배정과 반납 =====

    #region ===== 만석 갱신 =====

    /// <summary> KYH : 빈 좌석이 있는지 다시 확인하고 만석 여부가 바뀌면 알린다 </summary>
    private void RefreshFull()
    {
        bool isFull = true;

        foreach (CustomerController occupant in _occupants)
        {
            if (occupant == null)
            {
                isFull = false;

                break;
            }
        }

        if (_isFull != isFull)
        {
            _isFull = isFull;
            OnFullChanged?.Invoke(_isFull);
        }
    }

    #endregion ===== 만석 갱신 =====
}
