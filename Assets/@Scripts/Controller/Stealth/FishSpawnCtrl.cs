using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary> KimYH : 생선 위치 중 랜덤하게 골라 생선을 놓는다. 자물쇠가 걸린 가구에는 Epic, Legendary만 놓는다 </summary>
[DefaultExecutionOrder(100)]
public class FishSpawnCtrl : MonoBehaviour
{
    #region ===== 생선 =====

    private FishPickup _fishPickupPrefab;

    /// <summary> KimYH : 생선이 놓일 수 있는 위치. 비워두면 씬의 FishSpawnPoint를 모두 찾는다. </summary>
    [Header("생선 스폰")]
    [SerializeField] private List<FishSpawnPoint> _fishSpawnPoints = new();

    [SerializeField, Min(0)] private int _minFishSpawnCount = 3;
    [SerializeField, Min(0)] private int _maxFishSpawnCount = 6;

    /// <summary> KimYH : 잠긴 가구에서 Legendary가 나올 확률 (나머지는 Epic) </summary>
    [Header("잠긴 가구 레전더리 비율")]
    [SerializeField, Range(0.0f, 1.0f)] private float _lockedLegendaryRatio = 0.5f;

    #endregion ===== 생선 =====

    #region ===== 생선 후보 =====

    /// <summary> KimYH : 습득 확률 가중치로 생선을 뽑는 후보 묶음 </summary>
    private class FishPool
    {
        private readonly List<FishData> _fishList = new();
        private readonly List<int> _cumulativeWeights = new();
        private int _totalWeight;

        public bool IsEmpty => _totalWeight <= 0;

        /// <summary> KimYH : 가중치가 0보다 큰 생선만 넣는다 </summary>
        public void Add(FishData fishData)
        {
            if (fishData.AcquireWeight <= 0)
            {
                return;
            }

            _totalWeight += fishData.AcquireWeight;
            _fishList.Add(fishData);
            _cumulativeWeights.Add(_totalWeight);
        }

        /// <summary> KimYH : 가중치 누적합 앞에서부터 랜덤 값이 들어가는 생선을 고른다 </summary>
        public FishData Pick()
        {
            if (IsEmpty)
            {
                return null;
            }

            int randomWeight = UnityEngine.Random.Range(0, _totalWeight);

            for (int i = 0; i < _cumulativeWeights.Count; i++)
            {
                if (randomWeight < _cumulativeWeights[i])
                {
                    return _fishList[i];
                }
            }

            return _fishList[_fishList.Count - 1];
        }
    }

    private readonly FishPool _allPool = new();
    private readonly FishPool _epicPool = new();
    private readonly FishPool _legendaryPool = new();

    #endregion ===== 생선 후보 =====

    private void Awake()
    {
        LoadPrefabs();
    }

    private void Start()
    {
        SpawnFish();
    }

    private void OnValidate()
    {
        if (_maxFishSpawnCount < _minFishSpawnCount)
        {
            _maxFishSpawnCount = _minFishSpawnCount;
        }
    }

    #region ===== 프리팹 로드 =====

    /// <summary> KimYH : 생선 프리팹을 불러온다 (ResourceManager가 캐싱) </summary>
    private void LoadPrefabs()
    {
        ResourceManager resource = Managers.Resource;

        if (resource == null)
        {
            return;
        }

        _fishPickupPrefab = resource.Load<FishPickup>(ResourceKey.Path.Item + ResourceKey.Name.Fish);
    }

    #endregion ===== 프리팹 로드 =====

    #region ===== 생선 생성 =====

    /// <summary> KimYH : 생선 위치 중 랜덤하게 골라 랜덤 생선을 놓는다 </summary>
    private void SpawnFish()
    {
        if (_fishPickupPrefab == null)
        {
            CPrint.Warning("[FishSpawnCtrl] 생선 프리팹 없음");

            return;
        }

        List<FishSpawnPoint> candidatePoints = CollectFishSpawnPoints();

        if (candidatePoints.Count == 0)
        {
            CPrint.Warning("[FishSpawnCtrl] 생선 위치 없음");

            return;
        }

        if (!TryBuildFishPools())
        {
            return;
        }

        int spawnCount = Mathf.Min(UnityEngine.Random.Range(_minFishSpawnCount, _maxFishSpawnCount + 1), candidatePoints.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            int j = UnityEngine.Random.Range(i, candidatePoints.Count);
            (candidatePoints[i], candidatePoints[j]) = (candidatePoints[j], candidatePoints[i]);

            FishSpawnPoint spawnPoint = candidatePoints[i];
            FishData fishData = spawnPoint.IsLocked ? PickLockedFish() : _allPool.Pick();

            SpawnFishAt(spawnPoint, fishData);
        }
    }

    /// <summary> KimYH : 인스펙터에 넣었으면 그걸 쓰고, 없으면 씬의 FishSpawnPoint를 모두 쓴다 </summary>
    private List<FishSpawnPoint> CollectFishSpawnPoints()
    {
        IList<FishSpawnPoint> source = _fishSpawnPoints.Count > 0
            ? (IList<FishSpawnPoint>)_fishSpawnPoints
            : FindObjectsOfType<FishSpawnPoint>();

        List<FishSpawnPoint> points = new(source.Count);

        for (int i = 0; i < source.Count; i++)
        {
            FishSpawnPoint point = source[i];

            if (point != null && point.isActiveAndEnabled)
            {
                points.Add(point);
            }
        }

        return points;
    }

    /// <summary> KimYH : 생선 테이블을 한 번 돌며 전체, Epic, Legendary 후보를 만든다 </summary>
    private bool TryBuildFishPools()
    {
        FishTable fishTable = Managers.Table?.Fish;

        if (fishTable == null || fishTable.Map.Count == 0)
        {
            CPrint.Warning("[FishSpawnCtrl] 생선 테이블 없음");

            return false;
        }

        foreach (FishData fishData in fishTable.Map.Values)
        {
            _allPool.Add(fishData);

            switch (fishData.GradeType)
            {
                case GradeType.Epic:
                    _epicPool.Add(fishData);
                    break;
                case GradeType.Legendary:
                    _legendaryPool.Add(fishData);
                    break;
            }
        }

        if (_allPool.IsEmpty)
        {
            CPrint.Warning("[FishSpawnCtrl] 습득 확률 가중치 합이 0");

            return false;
        }

        return true;
    }

    /// <summary> KimYH : 잠긴 가구용. 설정한 비율로 등급을 먼저 정하고, 그 등급 안에서 가중치로 고른다 </summary>
    private FishData PickLockedFish()
    {
        FishPool gradePool = UnityEngine.Random.value < _lockedLegendaryRatio ? _legendaryPool : _epicPool;

        if (gradePool.IsEmpty)
        {
            gradePool = gradePool == _legendaryPool ? _epicPool : _legendaryPool;
        }

        return gradePool.IsEmpty ? _allPool.Pick() : gradePool.Pick();
    }

    /// <summary> KimYH : 지정한 위치에 생선을 만들고 종류와 개수를 넣는다 </summary>
    private void SpawnFishAt(FishSpawnPoint spawnPoint, FishData fishData)
    {
        Transform point = spawnPoint.transform;
        Transform parent = spawnPoint.AttachToPoint ? point : null;

        Quaternion randomRot = Quaternion.Euler(0.0f, UnityEngine.Random.Range(0.0f, 360.0f), 0.0f);
        Quaternion rotation = point.rotation * randomRot * _fishPickupPrefab.transform.rotation;

        FishPickup fish = Instantiate(_fishPickupPrefab, point.position, rotation, parent);

        int minCount = Mathf.Max(1, fishData.MinRandomAcquireCount);
        int maxCount = Mathf.Max(minCount, fishData.MaxRandomAcquireCount);

        fish.Setup(fishData.ID, UnityEngine.Random.Range(minCount, maxCount + 1));
    }

    #endregion ===== 생선 생성 =====
}
