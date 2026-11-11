using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.UI;

/// <summary>SJY : 만능툴 최대 소지 개수에 따라 잠입씬의 자물쇠 오브젝트를 활성화</summary>
public class LockSpawnCtrl : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private Transform _interactionRoot;

    #region ==== 설정 ====

    private const int MasterToolItemID = 11;
    private const int FullSpawnStartDay = 5;
    private const float EarlyDaySpawnRatio = 0.5f;
    private const string LockObjectName = "RotationLock_Interactable";

    #endregion #region ==== 설정 ====

    private readonly List<GameObject> _locks = new();

    private void Start()
    {
        InitializeLocks();
    }

    /// <summary>SJY : 현재 일차에 따른 자물쇠 활성화 개수 계산 및 배치</summary>
    private void InitializeLocks()
    {
        if (_interactionRoot == null || Managers.Table == null || Managers.Table.Item == null || Managers.Data == null)
        {
            return;
        }

        CollectLocks();

        int currentDay = Managers.Data.GetCurrentDay();
        int maxToolCount = GetMasterToolLimitCount();
        int spawnCount = GetSpawnCount(currentDay, maxToolCount);

        ActivateRandomLocks(spawnCount);

        CPrint.Log($"[LockSpawnCtrl] {currentDay}일차 / 최대 소지 개수: {maxToolCount} / 생성 목표: {spawnCount} / 자물쇠 후보: {_locks.Count}");
    }

    /// <summary>SJY : 집 안에 존재하는 모든 자물쇠 수집</summary>
    private void CollectLocks()
    {
        _locks.Clear();

        Transform[] children = _interactionRoot.GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.name != LockObjectName)
            {
                continue;
            }

            _locks.Add(child.gameObject);
        }
    }

    /// <summary>SJY : 아이템 테이블에서 만능툴의 최대 소지 개수 조회</summary>
    private int GetMasterToolLimitCount()
    {
        Item item = Managers.Table.Item.GetItem(MasterToolItemID);

        if (item == null)
        {
            CPrint.Warning($"[LockSpawnCtrl] 아이템 ID {MasterToolItemID}를 찾을 수 없습니다");

            return 0;
        }

        return Mathf.Max(0, item.LimitCount);
    }

    private int GetSpawnCount(int currentDay, int maxToolCount)
    {
        if (currentDay < FullSpawnStartDay)
        {
            return Mathf.FloorToInt(maxToolCount * EarlyDaySpawnRatio);
        }

        return maxToolCount;
    }

    private void ActivateRandomLocks(int count)
    {
        foreach (GameObject lockObj in _locks)
        {
            lockObj.SetActive(false);
        }

        for (int i = _locks.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            (_locks[i], _locks[randomIndex]) = (_locks[randomIndex], _locks[i]);
        }

        int activeCount = Mathf.Min(Mathf.Max(0, count), _locks.Count);

        for (int i = 0; i < activeCount; i++)
        {
            _locks[i].SetActive(true);
        }

        CPrint.Log($"[LockSpawnCtrl] 전체 자물쇠 : {_locks.Count}개 / 활성화 자물쇠 : {activeCount}개");
    }


}
