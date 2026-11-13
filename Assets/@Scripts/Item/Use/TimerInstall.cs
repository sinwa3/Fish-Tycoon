using UnityEngine;

/// <summary> KYH : 타이머 장치를 플레이어 발밑에 설치한다 </summary>
public class TimerInstall : IConsumableEffect
{
    private readonly CharacterController _characterController;

    public TimerInstall(CharacterController characterController)
    {
        _characterController = characterController;
    }

    /// <summary> KYH : 타이머 장치를 설치하고 성공하면 true </summary>
    public bool Use(Item item)
    {
        if (_characterController == null)
        {
            return false;
        }

        Vector3 position = _characterController.transform.position;
        position.y += _characterController.center.y - _characterController.height * 0.5f;

        GameObject timerObject = Managers.Resource.Spawn(ResourceKey.Path.Item + item.ResourceName, position, Quaternion.identity);

        if (timerObject == null)
        {
            return false;
        }

        TimerNoise noise = timerObject.GetComponent<TimerNoise>();

        if (noise == null)
        {
            timerObject.DestroyGO();

            return false;
        }

        noise.StartTimer(item.BuffValue);

        return true;
    }
}
