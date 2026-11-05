using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishSpawnCtrl : MonoBehaviour
{
    #region ===== 생선 =====

    [Header("생선")]
    private FishPickup _fishPickupPrefab;

    /// <summary> KimYH : 생선이 놓일 수 있는 위치. 비워두면 씬의 FishSpawnPoint를 모두 찾는다. </summary>
    [SerializeField] private List<FishSpawnPoint> _fishSpawnPoints = new();

    [SerializeField, Min(0)] private int _minFishSpawnCount = 3;
    [SerializeField, Min(0)] private int _maxFishSpawnCount = 6;

    #endregion ===== 생선 =====

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
            CPrint.Warning("[SpawnManager] 생선 프리팹 없음");

            return;
        }

        List<FishSpawnPoint> candidatePoints = CollectFishSpawnPoints();

        if (candidatePoints.Count == 0)
        {
            CPrint.Warning("[SpawnManager] 생선 위치 없음");

            return;
        }

        if (!TryBuildFishWeights(out FishData[] fishList, out int[] cumulativeWeights))
        {
            return;
        }

        int spawnCount = Mathf.Min(UnityEngine.Random.Range(_minFishSpawnCount, _maxFishSpawnCount + 1), candidatePoints.Count);
        int totalWeight = cumulativeWeights[cumulativeWeights.Length - 1];

        for (int i = 0; i < spawnCount; i++)
        {
            int j = UnityEngine.Random.Range(i, candidatePoints.Count);
            (candidatePoints[i], candidatePoints[j]) = (candidatePoints[j], candidatePoints[i]);

            FishData fishData = fishList[SelectWeightedIndex(cumulativeWeights, UnityEngine.Random.Range(0, totalWeight))];
            SpawnFishAt(candidatePoints[i], fishData);
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

    /// <summary> KimYH : 생선 목록과 습득 확률 가중치 누적합을 만든다. 가중치가 0 이하인 생선은 뺀다 </summary>
    private static bool TryBuildFishWeights(out FishData[] fishList, out int[] cumulativeWeights)
    {
        fishList = null;
        cumulativeWeights = null;

        FishTable fishTable = Managers.Table?.Fish;

        if (fishTable == null || fishTable.Map.Count == 0)
        {
            CPrint.Warning("[SpawnManager] 생선 테이블 없음");

            return false;
        }

        List<FishData> validFish = new(fishTable.Map.Count);
        List<int> weights = new(fishTable.Map.Count);
        int totalWeight = 0;

        foreach (FishData fishData in fishTable.Map.Values)
        {
            if (fishData.AcquireWeight <= 0)
            {
                continue;
            }

            totalWeight += fishData.AcquireWeight;
            validFish.Add(fishData);
            weights.Add(totalWeight);
        }

        if (totalWeight <= 0)
        {
            CPrint.Warning("[SpawnManager] 습득 확률 가중치 합이 0");

            return false;
        }

        fishList = validFish.ToArray();
        cumulativeWeights = weights.ToArray();

        return true;
    }

    /// <summary> KimYH : 0 이상 총합 미만의 가중치 값이 누적합의 어느 사이로 들어갈지 고른다 </summary>
    private static int SelectWeightedIndex(int[] cumulativeWeights, int randomWeight)
    {
        int low = 0;
        int high = cumulativeWeights.Length - 1;

        while (low < high)
        {
            int mid = (low + high) / 2;

            if (randomWeight < cumulativeWeights[mid])
            {
                high = mid;
            }
            else
            {
                low = mid + 1;
            }
        }

        return low;
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
