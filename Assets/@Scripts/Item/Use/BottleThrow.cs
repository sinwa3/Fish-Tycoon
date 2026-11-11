using UnityEngine;

/// <summary> KYH : 병을 카메라 위치에 만들어 바라보는 방향으로 던진다 </summary>
public class BottleThrow : IConsumableEffect
{
    #region ===== 수치 =====

    // 던지는 속도
    private const float ThrowSpeed = 10.0f;

    #endregion ===== 수치 =====

    private readonly Transform _cameraTransform;
    private readonly CharacterController _characterController;

    public BottleThrow(Transform cameraTransform, CharacterController characterController)
    {
        _cameraTransform = cameraTransform;
        _characterController = characterController;
    }

    /// <summary> KYH : 병을 던지고 성공하면 true </summary>
    public bool Use(Item item)
    {
        if (_cameraTransform == null)
        {
            return false;
        }

        if (_characterController == null)
        {
            return false;
        }

        Vector3 position = _cameraTransform.position;

        GameObject bottleObject = Managers.Resource.Spawn(ResourceKey.Path.Item + item.ResourceName, position, Quaternion.identity);

        if (bottleObject == null)
        {
            return false;
        }

        Collider[] bottleColliders = bottleObject.GetComponentsInChildren<Collider>();

        foreach (Collider bottleCollider in bottleColliders)
        {
            Physics.IgnoreCollision(bottleCollider, _characterController);
        }

        Rigidbody bottleRigidbody = bottleObject.GetComponent<Rigidbody>();
        bottleRigidbody.velocity = _cameraTransform.forward * ThrowSpeed;

        return true;
    }
}
