using System.Collections.Generic;
using UnityEngine;

/// <summary> KYH : 퀵슬롯에서 사용한 소비 아이템의 효과를 낸다 </summary>
public class PlayerItemUse : MonoBehaviour
{
    // ItemTable의 병 ID
    private const int BottleItemID = 12;

    // 던지는 방향의 기준이 될 카메라 (비우면 Camera.main)
    [SerializeField] private Transform _cameraTransform;

    // 아이템 ID별 효과
    private readonly Dictionary<int, IConsumableEffect> _effects = new();

    private void Awake()
    {
        InitializeComponents();
        InitializeEffects();
    }

    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    #region ===== 초기화 =====

    /// <summary> KYH : 던지는 방향의 기준이 될 카메라를 가져온다 </summary>
    private void InitializeComponents()
    {
        if (_cameraTransform == null && Camera.main != null)
        {
            _cameraTransform = Camera.main.transform;
        }

        if (_cameraTransform == null)
        {
            CPrint.Warning("[PlayerItemUse] 카메라 없음");
        }
    }

    /// <summary> KYH : 아이템마다 효과를 만들어 등록한다 </summary>
    private void InitializeEffects()
    {
        _effects.Add(BottleItemID, new BottleThrow(_cameraTransform, GetComponent<CharacterController>()));
        // TODO : 타이머 장치, 빨간 버튼 효과 등록
    }

    #endregion ===== 초기화 =====

    #region ===== 이벤트 =====

    /// <summary> KYH : 아이템 사용 이벤트를 구독한다 </summary>
    private void SubscribeEvents()
    {
        if (Managers.Data == null)
        {
            return;
        }

        Managers.Data.OnItemUsed += HandleItemUsed;
    }

    /// <summary> KYH : 아이템 사용 이벤트 구독을 해제한다 </summary>
    private void UnsubscribeEvents()
    {
        if (Managers.Data == null)
        {
            return;
        }

        Managers.Data.OnItemUsed -= HandleItemUsed;
    }

    #endregion ===== 이벤트 =====

    #region ===== 사용 =====

    /// <summary> KYH : 퀵슬롯에서 아이템을 쓰면 그 아이템의 효과를 낸다 </summary>
    private void HandleItemUsed(int itemID)
    {
        Item item = Managers.Table.Item.GetItem(itemID);

        UseItem(item);
    }

    /// <summary> KYH : 등록된 효과를 찾아 쓰고 성공하면 true </summary>
    private bool UseItem(Item item)
    {
        if (item == null)
        {
            return false;
        }

        if (!_effects.TryGetValue(item.ID, out IConsumableEffect effect))
        {
            return false;
        }

        return effect.Use(item);
    }

    #endregion ===== 사용 =====
}
