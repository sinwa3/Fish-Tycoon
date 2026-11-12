#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary> 잠입 중 퀵슬롯의 소비 아이템을 넣고 뺀다. 구현된 아이템만 넣는다 </summary>
public class QuickSlotItemCheat : BaseCountCheat
{
    // 소비 아이템을 더하는 공개 메서드가 없다
    private static readonly MethodInfo AddConsumableMethod = typeof(DataManager).GetMethod("TryAddConsumable", BindingFlags.Instance | BindingFlags.NonPublic);
    // 효과가 등록된 아이템이 구현된 아이템이다
    private static readonly FieldInfo EffectsField = typeof(PlayerItemUse).GetField("_effects", BindingFlags.Instance | BindingFlags.NonPublic);
    // 효과 없이 소지만 하는 아이템. 락픽은 자물쇠 미니게임이 퀵슬롯에서 세고 소모한다
    private static readonly HashSet<int> HoldOnlyIDs = new() { 11 };

    private PlayerItemUse _playerItemUse;

    public override string Title => "아이템 (잠입 퀵슬롯)";

    public override bool IsAvailable => Managers.Data != null && Managers.Table?.Item != null && IsCurrentScene<StealthScene>();

    protected override void OnInitialize()
    {
        if (AddConsumableMethod == null || EffectsField == null)
        {
            CPrint.Warning("[QuickSlotItemCheat] 리플렉션할 필드나 메서드 없음");
        }
    }

    /// <summary> 아이템 테이블에서 소비 아이템만 모은다 </summary>
    protected override void CollectIDs(List<int> ids)
    {
        foreach (int id in Managers.Table.Item.Map.Keys)
        {
            if (Managers.Table.Item.GetItem(id)?.Type == ItemType.Consumable)
            {
                ids.Add(id);
            }
        }
    }

    protected override string GetName(int id)
    {
        string name = $"{id} {Managers.Table.Item.GetItem(id)?.Name}";

        return IsImplemented(id) ? name : $"{name} (미구현)";
    }

    protected override int GetCount(int id)
    {
        return CheatQuickSlot.GetCount(QuickSlotItemType.Consumable, id);
    }

    /// <summary> 구현된 아이템만 최대 개수 안에서 넣고, 못 넣으면 토스트로 알린다 </summary>
    protected override void Add(int id, int count)
    {
        if (!IsImplemented(id))
        {
            Managers.UI?.OpenToastMessage("아직 구현되지 않은 아이템이라 추가하지 않았습니다.");

            return;
        }

        bool isAdded = AddConsumableMethod != null && (bool)AddConsumableMethod.Invoke(Managers.Data, new object[] { id, count });

        if (!isAdded)
        {
            Managers.UI?.OpenToastMessage("최대 개수를 넘거나 퀵슬롯이 가득 찼습니다.");
        }
    }

    protected override void Remove(int id, int count)
    {
        CheatQuickSlot.Remove(QuickSlotItemType.Consumable, id, count);
    }

    /// <summary> 소지형 아이템이거나 플레이어의 아이템 사용에 효과가 등록된 아이템인지 돌려준다 </summary>
    private bool IsImplemented(int id)
    {
        if (HoldOnlyIDs.Contains(id))
        {
            return true;
        }

        if (_playerItemUse == null)
        {
            _playerItemUse = Object.FindObjectOfType<PlayerItemUse>();
        }

        if (_playerItemUse == null || EffectsField == null)
        {
            return false;
        }

        return EffectsField.GetValue(_playerItemUse) is IDictionary effects && effects.Contains(id);
    }
}
#endif
