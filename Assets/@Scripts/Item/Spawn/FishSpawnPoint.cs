using UnityEngine;

/// <summary> KimYH : 생선이 놓일 수 있는 위치. 상호작용 가구의 자식으로 두고 위치를 맞춘다 </summary>
public class FishSpawnPoint : MonoBehaviour
{
    [SerializeField] private bool _attachToPoint = true;

    public bool AttachToPoint => _attachToPoint;

    // 확인용 기즈모
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.6f, 1f);
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.25f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 0.3f);
    }
}
