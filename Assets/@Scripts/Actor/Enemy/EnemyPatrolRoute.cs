using System.Collections.Generic;
using UnityEngine;

/// <summary> KYH : 순찰 웨이포인트를 모은다 </summary>
public class EnemyPatrolRoute : MonoBehaviour
{
    // 자식 이름은 Waypoint_0 ~ n
    private const string WaypointNamePrefix = "Waypoint_";

    #region ===== 웨이포인트 =====

    private readonly List<Transform> _waypoints = new();

    /// <summary> KYH : 웨이포인트 개수 </summary>
    public int WaypointCount => _waypoints.Count;

    #endregion ===== 웨이포인트 =====

    private void Awake()
    {
        InitializeWaypoints();
    }

    #region ===== 초기화 =====

    /// <summary> KYH : 자식 수만큼 Waypoint_0부터 찾아 웨이포인트 목록을 만든다 </summary>
    private void InitializeWaypoints()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform childTransform = gameObject.FindChild<Transform>(WaypointNamePrefix + i);

            if (childTransform == null)
            {
                continue;
            }

            _waypoints.Add(childTransform);
        }
    }

    #endregion ===== 초기화 =====

    #region ===== 웨이포인트 가져오기 =====

    /// <summary> KYH : 웨이포인트 위치를 가져온다 </summary>
    public Transform GetWaypoint(int waypointIndex)
    {
        if (waypointIndex < 0 || waypointIndex >= _waypoints.Count)
        {
            CPrint.Warning($"[EnemyPatrolRoute] {waypointIndex}번 웨이포인트 없음");

            return null;
        }

        return _waypoints[waypointIndex];
    }

    #endregion ===== 웨이포인트 가져오기 =====
}
