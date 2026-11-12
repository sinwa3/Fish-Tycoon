using UnityEngine;

/// <summary> KimYH : 생선이 놓일 수 있는 위치. 상호작용 가구의 자식으로 두고 위치를 맞춘다 </summary>
public class FishSpawnPoint : MonoBehaviour
{
    [SerializeField] private bool _attachToPoint = true;

    public bool AttachToPoint => _attachToPoint;

    /// <summary> KimYH : 이 위치를 막고 있는 가구 파츠 (서랍, 수납장 문). 없으면 null /// </summary>
    private InteractionFurniture _furniture;

    /// <summary> KimYH : 해당 가구 파츠에 자물쇠가 걸려 있는지 </summary>
    public bool IsLocked => _furniture != null && _furniture.IsLocked;

    private void Awake()
    {
        InitializeFurniture();
    }

    /// <summary> KimYH : 가구 파츠를 찾는다 </summary>
    private void InitializeFurniture()
    {
        // 서랍 안에 놓인 경우 (InteractiveFurniture_04, 05 등)
        _furniture = GetComponentInParent<InteractionFurniture>(true);

        if (_furniture != null || transform.parent == null)
        {
            return;
        }

        // 문이 하나뿐인 수납장 본체에 놓인 경우 (InteractiveFurniture_12, 15, 18)
        InteractionFurniture[] siblingParts = transform.parent.GetComponentsInChildren<InteractionFurniture>(true);

        if (siblingParts.Length == 1)
        {
            _furniture = siblingParts[0];
        }
    }

    // 확인용 기즈모
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.6f, 1f);
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.25f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 0.3f);
    }
}
