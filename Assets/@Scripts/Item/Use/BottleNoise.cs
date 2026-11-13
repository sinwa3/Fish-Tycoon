using UnityEngine;

/// <summary> KYH : 던진 병이 부딪힌 자리에 소음을 낸다 </summary>
[RequireComponent(typeof(Rigidbody))]
public class BottleNoise : MonoBehaviour
{
    private bool _hasMadeNoise;

    private void Start()
    {
        AutoDestroy();
    }

    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint contactPoint = collision.GetContact(0);
        GameObject hitObject = collision.collider.gameObject;

        bool isFloor = hitObject.layer == LayerKey.Idx.Wall && contactPoint.normal.y >= ConsumableTempValue.FloorNormalMinY;

        if (!isFloor)
        {
            return;
        }

        MakeNoise(contactPoint.point);
        Destroy(gameObject);
    }

    /// <summary> KYH : 일정 시간 뒤 병이 사라지게 한다</summary>
    private void AutoDestroy()
    {
        Destroy(gameObject, ConsumableTempValue.BottleAutoDestroyTime);
    }

    /// <summary> KYH : 지정한 위치에 병 소음을 한 번만 낸다 </summary>
    private void MakeNoise(Vector3 position)
    {
        if (_hasMadeNoise)
        {
            return;
        }

        StealthScene scene = Managers.Scene.CurrentScene as StealthScene;

        if (scene == null)
        {
            return;
        }

        scene.RaiseNoiseMade(position, ConsumableTempValue.BottleNoiseLoudness);
        _hasMadeNoise = true;
        Managers.Sound?.PlaySfx(SoundKey.Name.SfxType.ItemBottle);
    }
}
