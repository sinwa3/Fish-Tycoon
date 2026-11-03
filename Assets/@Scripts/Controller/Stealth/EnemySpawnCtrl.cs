using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary> KimYH : 잠입 씬 시작 시 집주인(순찰 경로 포함)과 랜덤 생선을 맵에 배치한다 </summary>
public class EnemySpawnCtrl : MonoBehaviour
{
    #region ===== 집주인 =====

    /// <summary> KimYH : 집주인 한 명을 어디에 놓고 어떤 경로로 순찰시킬지 </summary>
    [Serializable]
    private class EnemySpawnInfo
    {
        public Transform SpawnPoint;

        public EnemyPatrolRoute PatrolRoute;
    }

    [Header("집주인")]
    [SerializeField] private List<EnemySpawnInfo> _enemySpawnInfos = new();

    private EnemyController _enemyPrefab;

    private readonly List<EnemyController> _spawnedEnemies = new();

    /// <summary> KimYH : 생성된 집주인 목록 </summary>
    public IReadOnlyList<EnemyController> SpawnedEnemies => _spawnedEnemies;

    #endregion ===== 집주인 =====

    private void Awake()
    {
        LoadPrefabs();
    }

    private void Start()
    {
        SpawnEnemies();
    }

    #region ===== 프리팹 로드 =====

    /// <summary> KimYH : 집주인 프리팹을 불러온다 (ResourceManager가 캐싱) </summary>
    private void LoadPrefabs()
    {
        ResourceManager resource = Managers.Resource;

        if (resource == null)
        {
            return;
        }

        _enemyPrefab = resource.Load<EnemyController>(ResourceKey.Path.Enemy + ResourceKey.Name.Enemy);
    }

    #endregion ===== 프리팹 로드 =====

    #region ===== 집주인 생성 =====

    /// <summary> KimYH : 등록된 위치마다 집주인을 만들고 순찰 경로를 넘긴다 </summary>
    private void SpawnEnemies()
    {
        if (_enemyPrefab == null)
        {
            CPrint.Warning("[SpawnManager] 집주인 프리팹 없음");

            return;
        }

        if (Managers.Data == null || Managers.Data.GetCurrentDay() == 1)
        {
            return;
        }
        
        foreach (EnemySpawnInfo spawnInfo in _enemySpawnInfos)
        {
            SpawnEnemy(spawnInfo);
        }
    }

    /// <summary> KimYH : 집주인 하나를 만든다. </summary>
    private void SpawnEnemy(EnemySpawnInfo spawnInfo)
    {
        if (spawnInfo == null || spawnInfo.SpawnPoint == null)
        {
            CPrint.Warning("[SpawnManager] 집주인 생성 위치 없음");

            return;
        }

        Transform spawnPoint = spawnInfo.SpawnPoint;
        EnemyController enemy = Instantiate(_enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        if (spawnInfo.PatrolRoute != null)
        {
            enemy.SetPatrolRoute(spawnInfo.PatrolRoute);
        }
        else
        {
            CPrint.Warning($"[SpawnManager] [{spawnPoint.name}] 순찰 경로 없음");
        }

        _spawnedEnemies.Add(enemy);
    }

    #endregion ===== 집주인 생성 =====
}
