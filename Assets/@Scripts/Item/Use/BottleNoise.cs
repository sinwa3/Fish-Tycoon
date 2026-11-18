using UnityEngine;

/// <summary> KYH : 던진 병이 부딪힌 자리에 소음을 낸다 </summary>
[RequireComponent(typeof(Rigidbody))]
public class BottleNoise : MonoBehaviour
{
    // 병이 깨질 때 소음 크기 (들리는 반경 = 크기 x 소리 범위)
    private const float NoiseLoudness = 3.0f;
    // 법선 벡터 최소 y
    private const float FloorNormalMinY = 0.9f;
    // 바닥에 못 닿은 병이 사라지기까지 시간
    private const float AutoDestroyTime = 10.0f;

    private bool _hasMadeNoise;

    private void Start()
    {
        AutoDestroy();
    }

    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint contactPoint = collision.GetContact(0);
        GameObject hitObject = collision.collider.gameObject;

        bool isFloor = hitObject.layer == LayerKey.Idx.Wall && contactPoint.normal.y >= FloorNormalMinY;

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
        Destroy(gameObject, AutoDestroyTime);
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

        scene.RaiseNoiseMade(position, NoiseLoudness);
        _hasMadeNoise = true;
        Managers.Sound?.PlaySfx(SoundKey.Name.SfxType.ItemBottle);
    }
}
