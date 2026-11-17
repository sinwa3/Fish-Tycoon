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

    #region ===== 층 =====

    // 같은 층으로 보는 웨이포인트 높이 차이
    private const float FloorHeightRange = 1.0f;

    #endregion ===== 층 =====

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

    /// <summary> KYH : 가까운 층의 웨이포인트 번호 담기 </summary>
    public void GetFloorWaypointIndices(float height, List<int> waypointIndices)
    {
        waypointIndices.Clear();

        // 높이가 가장 가까운 웨이포인트의 높이를 층 높이로 본다
        float floorHeight = height;
        float minHeightGap = float.MaxValue;

        foreach (Transform waypoint in _waypoints)
        {
            float heightGap = Mathf.Abs(waypoint.position.y - height);

            if (heightGap < minHeightGap)
            {
                minHeightGap = heightGap;
                floorHeight = waypoint.position.y;
            }
        }

        for (int i = 0; i < _waypoints.Count; i++)
        {
            if (Mathf.Abs(_waypoints[i].position.y - floorHeight) <= FloorHeightRange)
            {
                waypointIndices.Add(i);
            }
        }
    }

    #endregion ===== 웨이포인트 가져오기 =====
}
