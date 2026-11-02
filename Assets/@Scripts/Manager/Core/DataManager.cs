using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary> KDH : 게임의 로컬 세이브 데이터를 저장하고 관리한다. </summary>
public class DataManager
{
    private const string SaveFileName = "SaveData.json";

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    private SaveData _saveData;

    public bool IsSaved => File.Exists(SaveFilePath);

    #region ===== 퀵슬롯 =====

    private const int QuickSlotBaseCount = 4;

    #endregion ===== 퀵슬롯 =====

    #region ===== 이벤트 =====

    public event Action OnGoldChanged;
    public event Action OnSatisfactionChanged;
    public event Action OnQuickSlotChanged;
    public event Action OnStoreChanged;

    #endregion ===== 이벤트 =====

    /// <summary> KDH : 현재 게임 데이터를 로컬 파일에 저장한다. </summary>
    public void SaveData()
    {
        string json = JsonUtility.ToJson(_saveData, true);

        CPrint.Log($"[DataManager] Save\n{json}");

        File.WriteAllText(SaveFilePath, json);
    }

    /// <summary> KDH : 로컬 파일에서 게임 데이터를 불러오거나 새로 만든다. </summary>
    public void LoadData()
    {
        // 새로하기
        if (!IsSaved)
        {
            _saveData = new SaveData();
            GrantInitialFish();
            return;
        }

        string json = File.ReadAllText(SaveFilePath);

        CPrint.Log($"[DataManager] Load\n{json}");

        if (string.IsNullOrEmpty(json))
        {
            _saveData = new SaveData();
            return;
        }

        _saveData = JsonUtility.FromJson<SaveData>(json);

        if (_saveData == null)
        {
            _saveData = new SaveData();
        }

        EnsureQuickSlotCount();
    }

    /// <summary> KDH : 세이버 데이터를 삭제한다. </summary>
    public void DeleteSaveData()
    {
        if (!IsSaved)
        {
            CPrint.Warning("[DataManager] 삭제할 데이터가 존재하지 않습니다.");
            return;
        }

        File.Delete(SaveFilePath);
    }

    #region ===== 골드 =====

    /// <summary> KDH : 골드를 추가한다. </summary>
    public void AddGold(int amount)
    {
        if (_saveData == null)
        {
            return;
        }

        if (amount <= 0)
        {
            return;
        }

        _saveData.Gold += amount;
        OnGoldChanged?.Invoke();
    }

    /// <summary> KDH : 골드를 차감할 수 있는지 확인하고 차감한다. </summary>
    public bool TrySpendGold(int amount)
    {
        if (_saveData == null)
        {
            return false;
        }

        if (amount < 0)
        {
            return false;
        }

        if (_saveData.Gold < amount)
        {
            return false;
        }

        _saveData.Gold -= amount;
        OnGoldChanged?.Invoke();

        return true;
    }

    /// <summary> KDH : 현재 소지금을 가져온다. </summary>
    public int GetCurrentGold()
    {
        return _saveData?.Gold ?? 0;
    }

    #endregion ===== 골드 =====

    #region ===== 만족도 =====

    /// <summary> KDH : 만족도를 추가한다. </summary>
    public void AddSatisfaction(int amount)
    {
        if (_saveData == null)
        {
            return;
        }

        if (amount <= 0)
        {
            return;
        }

        int currentGrade = Managers.Table.Reputation.GetReputationGrade(GetSatisfaction());
        if (Managers.Table.Reputation.IsMaxReputationGrade(currentGrade))
        {
            return;
        }

        _saveData.Satisfaction += amount;
        OnSatisfactionChanged?.Invoke();
    }

    /// <summary> KDH : 만족도를 차감한다. </summary>
    public void SubtractSatisfaction(int amount)
    {
        if (_saveData == null)
        {
            return;
        }

        if (amount <= 0)
        {
            return;
        }

        _saveData.Satisfaction -= amount;
        if (GetSatisfaction() < 0)
        {
            _saveData.Satisfaction = 0;
        }

        OnSatisfactionChanged?.Invoke();
    }

    /// <summary> KDH : 현재 만족도를 가져온다. </summary>
    public int GetSatisfaction()
    {
        return _saveData?.Satisfaction ?? 0;
    }

    /// <summary> KDH : 현재 평판 등급과 만족도 진행 정보를 반환한다. </summary>
    /// <returns>(평판 등급, 현재 구간 만족도, 다음 등급까지 필요한 만족도)</returns>
    public (int reputationGrade, int currentSatisfaction, int requiredSatisfaction) GetReputationInfo()
    {
        if (_saveData == null)
        {
            CPrint.Error("[DataManager] SaveData is null.");
            return (0, 0, 0);
        }

        int totalSatisfaction = GetSatisfaction();

        int reputationGrade = Managers.Table.Reputation.GetReputationGrade(totalSatisfaction);

        ReputationData currentReputation = Managers.Table.Reputation.GetReputationByGrade(reputationGrade);
        ReputationData nextReputation = Managers.Table.Reputation.GetNextReputation(reputationGrade);

        if (currentReputation == null || nextReputation == null)
        {
            return (reputationGrade, -1, -1);
        }

        int currentSatisfaction = totalSatisfaction - currentReputation.RequiredSatisfaction;
        int requiredSatisfaction = nextReputation.RequiredSatisfaction - currentReputation.RequiredSatisfaction;

        return (reputationGrade, currentSatisfaction, requiredSatisfaction);
    }

    #endregion ===== 만족도 =====

    #region ===== 게임 상태 =====

    /// <summary> KDH : 현재 몇일 차 인지 반환 </summary>
    public int GetCurrentDay()
    {
        if (_saveData == null)
        {
            return 0;
        }

        return _saveData.CurrentDay;
    }

    /// <summary> KDH : 저장했을 당시의 게임모드를 반환한다. </summary>
    public GameMode GetSavedGameMode()
    {
        if (_saveData == null)
        {
            return 0;
        }

        return _saveData.SavedGameMode;
    }

    /// <summary> KDH : 저장된 게임모드를 변경한다. </summary>
    public void ChangeSavedGameMode(GameMode mode)
    {
        if (_saveData == null)
        {
            CPrint.Warning("[DataManager] SaveData is null.");
            return;
        }

        _saveData.SavedGameMode = mode;
    }

    /// <summary> KDH : 저장된 현재 날짜를 다음 날짜로 변경한다. </summary>
    public void SetNextDay()
    {
        if (_saveData == null)
        {
            CPrint.Warning("[DataManager] SaveData is null.");
            return;
        }

        _saveData.CurrentDay++;
    }

    #endregion ===== 게임 상태 =====

    #region ===== 상점 =====

    /// <summary> KDH : 아이템 구매 가능 상태를 반환한다. </summary>
    public PurchaseStatus GetPurchaseStatus(Item item)
    {
        if (_saveData == null || item == null || !Managers.Table.Shop.IsSelling(item.ID))
        {
            return PurchaseStatus.NotForSale;
        }

        // 장비 및 소비품의 보유 조건을 먼저 검사한다.
        PurchaseStatus status = item.Type switch
        {
            ItemType.Equipment => GetEquipmentPurchaseStatus(item),
            ItemType.Consumable => GetConsumablePurchaseStatus(item),
            _ => PurchaseStatus.NotForSale
        };

        if (status != PurchaseStatus.Available)
        {
            return status;
        }

        int price = Managers.Table.Shop.GetItemSellPrice(item.ID);

        return GetCurrentGold() < price ? PurchaseStatus.InsufficientGold : PurchaseStatus.Available;
    }

    /// <summary> KDH : 구매 할 장비티어와 현재 장착한 장비티어끼리 비교하여 구매 가능 상태를 반환한다. </summary>
    private PurchaseStatus GetEquipmentPurchaseStatus(Item item)
    {
        if (_saveData.Equipment == null || item.EquipType == EquipmentType.None)
        {
            return PurchaseStatus.NotForSale;
        }

        int equippedItemID = GetEquippedItemID(item.EquipType);
        Item equippedItem = equippedItemID >= 0 ? Managers.Table.Item.GetItem(equippedItemID) : null;
        int equippedItemTier = equippedItem?.Tier ?? 0;

        if (item.Tier <= equippedItemTier)
        {
            return PurchaseStatus.AlreadyPurchased;
        }

        if (item.Tier != equippedItemTier + 1)
        {
            return PurchaseStatus.PreviousTierRequired;
        }

        return PurchaseStatus.Available;
    }

    /// <summary> KDH : 소비 아이템의 보유 한도와 퀵슬롯의 빈 슬롯 여부를 확인하여 구매 가능 상태를 반환한다. </summary>
    private PurchaseStatus GetConsumablePurchaseStatus(Item item)
    {
        EnsureQuickSlotCount();

        bool hasEmptySlot = false;

        foreach (QuickSlotData slot in _saveData.QuickSlots)
        {
            if (slot.Type == QuickSlotItemType.Consumable && slot.ItemID == item.ID)
            {
                return slot.Count >= item.LimitCount ? PurchaseStatus.QuantityLimit : PurchaseStatus.Available;
            }

            if (slot.Type == QuickSlotItemType.None)
            {
                hasEmptySlot = true;
            }
        }

        return hasEmptySlot ? PurchaseStatus.Available : PurchaseStatus.NoEmptySlot;
    }

    /// <summary> KDH : 구매 가능 여부를 확인하고 골드를 차감한 뒤 아이템을 지급한다. </summary>
    public PurchaseStatus TryPurchaseItem(int itemID)
    {
        Item item = Managers.Table.Item.GetItem(itemID);
        PurchaseStatus status = GetPurchaseStatus(item);

        if (status != PurchaseStatus.Available)
        {
            return status;
        }

        int price = Managers.Table.Shop.GetItemSellPrice(itemID);

        // 골드 차감
        if (!TrySpendGold(price))
        {
            return PurchaseStatus.InsufficientGold;
        }

        bool isAdded = item.Type switch
        {
            ItemType.Equipment => TryEquipItem(item),
            ItemType.Consumable => TryAddConsumable(itemID),
            _ => false
        };

        // 아이템 지급 실패시
        if (!isAdded)
        {
            AddGold(price);
            return PurchaseStatus.NotForSale;
        }

        OnStoreChanged?.Invoke();

        return PurchaseStatus.Available;
    }

    #endregion ===== 상점 =====

    #region ===== 장비 =====

    /// <summary> KDH : 구매한 장비를 해당 장비 슬롯에 적용한다. </summary>
    private bool TryEquipItem(Item item)
    {
        if (_saveData?.Equipment == null || item == null)
        {
            return false;
        }

        switch (item.EquipType)
        {
            case EquipmentType.HipSack:
                _saveData.Equipment.HipSackID = item.ID;

                EnsureQuickSlotCount();
                OnQuickSlotChanged?.Invoke();
                return true;

            case EquipmentType.Socks:
                _saveData.Equipment.SocksID = item.ID;
                return true;

            case EquipmentType.Glove:
                _saveData.Equipment.GloveID = item.ID;
                return true;

            default:
                return false;
        }
    }

    /// <summary> KDH : 지정된 장비 종류에 현재 장착된 아이템 ID를 반환한다. </summary>
    public int GetEquippedItemID(EquipmentType equipType)
    {
        if (_saveData?.Equipment == null)
        {
            return -1;
        }

        return equipType switch
        {
            EquipmentType.HipSack => _saveData.Equipment.HipSackID,
            EquipmentType.Socks => _saveData.Equipment.SocksID,
            EquipmentType.Glove => _saveData.Equipment.GloveID,
            _ => -1
        };
    }

    #endregion ===== 장비 =====

    #region ===== 퀵슬롯 =====

    /// <summary> KDH : 현재 장비를 기준으로 퀵슬롯 개수를 반환한다. </summary>
    public int GetQuickSlotCount()
    {
        int slotCount = QuickSlotBaseCount;

        if (_saveData?.Equipment == null)
        {
            return slotCount;
        }

        Item hipSack = Managers.Table.Item.GetItem(_saveData.Equipment.HipSackID);

        if (hipSack != null)
        {
            slotCount += hipSack.BuffValue;
        }

        return slotCount;
    }

    /// <summary> KDH : 현재 장비에 맞춰 퀵슬롯 개수를 보정한다. </summary>
    private void EnsureQuickSlotCount()
    {
        if (_saveData == null)
        {
            return;
        }

        _saveData.QuickSlots ??= new List<QuickSlotData>();

        int slotCount = GetQuickSlotCount();

        while (_saveData.QuickSlots.Count < slotCount)
        {
            _saveData.QuickSlots.Add(new QuickSlotData());
        }
    }

    /// <summary> KDH : 지정된 퀵슬롯 데이터를 반환한다. </summary>
    public QuickSlotData GetQuickSlot(int slotIndex)
    {
        EnsureQuickSlotCount();

        if (_saveData?.QuickSlots == null)
        {
            return null;
        }

        if (slotIndex < 0 || slotIndex >= _saveData.QuickSlots.Count)
        {
            return null;
        }

        return _saveData.QuickSlots[slotIndex];
    }

    /// <summary> KDH : 잠입 중 획득한 생선을 퀵슬롯에 추가한다. </summary>
    public bool TryAddFishToQuickSlot(int fishID, int count = 1)
    {
        if (_saveData == null || fishID < 0 || count <= 0)
        {
            return false;
        }

        EnsureQuickSlotCount();

        // 동일한 생선이 있다면 기존 슬롯에 추가한다.
        foreach (QuickSlotData slot in _saveData.QuickSlots)
        {
            if (slot.Type != QuickSlotItemType.Fish || slot.ItemID != fishID)
            {
                continue;
            }

            if (slot.Count > int.MaxValue - count)
            {
                return false;
            }

            slot.Count += count;

            OnQuickSlotChanged?.Invoke();
            return true;
        }

        // 새로운 종류의 생선은 빈 슬롯이 있어야 획득할 수 있다.
        foreach (QuickSlotData slot in _saveData.QuickSlots)
        {
            if (slot.Type != QuickSlotItemType.None)
            {
                continue;
            }

            slot.Type = QuickSlotItemType.Fish;
            slot.ItemID = fishID;
            slot.Count = count;

            OnQuickSlotChanged?.Invoke();
            return true;
        }

        return false;
    }

    /// <summary> KDH : 지정된 퀵슬롯의 소비 아이템을 사용한다. </summary>
    public bool TryUseConsumable(int slotIndex)
    {
        QuickSlotData slot = GetQuickSlot(slotIndex);

        if (slot == null ||
            slot.Type != QuickSlotItemType.Consumable ||
            slot.ItemID < 0 ||
            slot.Count <= 0)
        {
            return false;
        }

        slot.Count--;

        if (slot.Count == 0)
        {
            ClearQuickSlot(slot);
        }

        OnQuickSlotChanged?.Invoke();
        OnStoreChanged?.Invoke();

        return true;
    }

    /// <summary> KDH : 소비 아이템을 퀵슬롯에 추가한다. </summary>
    private bool TryAddConsumable(int itemID, int count = 1)
    {
        if (_saveData == null || count <= 0)
        {
            return false;
        }

        Item item = Managers.Table.Item.GetItem(itemID);

        if (item == null || item.Type != ItemType.Consumable)
        {
            return false;
        }

        EnsureQuickSlotCount();

        // 동일한 소비 아이템이 있다면 기존 슬롯에 추가한다.
        foreach (QuickSlotData slot in _saveData.QuickSlots)
        {
            if (slot.Type != QuickSlotItemType.Consumable || slot.ItemID != itemID)
            {
                continue;
            }

            if (count > item.LimitCount - slot.Count)
            {
                return false;
            }

            slot.Count += count;

            OnQuickSlotChanged?.Invoke();
            return true;
        }

        // 동일한 소비 아이템이 없다면 빈 슬롯을 사용한다.
        foreach (QuickSlotData slot in _saveData.QuickSlots)
        {
            if (slot.Type != QuickSlotItemType.None)
            {
                continue;
            }

            if (count > item.LimitCount)
            {
                return false;
            }

            slot.Type = QuickSlotItemType.Consumable;
            slot.ItemID = itemID;
            slot.Count = count;

            OnQuickSlotChanged?.Invoke();
            return true;
        }

        return false;
    }

    /// <summary> KDH : 잠입 중 획득한 생선을 인벤토리로 이동하고 해당 퀵슬롯을 비운다. </summary>
    public void SettleQuickSlotFish()
    {
        if (_saveData?.QuickSlots == null)
        {
            return;
        }

        foreach (QuickSlotData slot in _saveData.QuickSlots)
        {
            if (slot.Type != QuickSlotItemType.Fish || slot.ItemID < 0 || slot.Count <= 0)
            {
                continue;
            }

            if (!TryAddFish(slot.ItemID, slot.Count))
            {
                CPrint.Warning($"[DataManager] 생선 정산 실패. FishID: {slot.ItemID}");
                continue;
            }

            ClearQuickSlot(slot);
        }

        OnQuickSlotChanged?.Invoke();
    }

    /// <summary> KDH : 퀵슬롯에서 소비 아이템만 초기화한다. </summary>
    public void ClearConsumables()
    {
        if (_saveData?.QuickSlots == null)
        {
            return;
        }

        foreach (QuickSlotData slot in _saveData.QuickSlots)
        {
            if (slot.Type != QuickSlotItemType.Consumable)
            {
                continue;
            }

            ClearQuickSlot(slot);
        }

        OnQuickSlotChanged?.Invoke();
        OnStoreChanged?.Invoke();
    }

    /// <summary> KDH : 퀵슬롯 데이터를 빈 상태로 초기화한다. </summary>
    private void ClearQuickSlot(QuickSlotData slot)
    {
        slot.Type = QuickSlotItemType.None;
        slot.ItemID = -1;
        slot.Count = 0;
    }

    #endregion ===== 퀵슬롯 =====

    #region ===== 인벤토리(생선) =====

    /// <summary> KDH : 보유한 생선을 전부 초기화한다. </summary>
    public void ClearFishInventory()
    {
        _saveData.InvenFish.Clear();
    }

    /// <summary> KDH : 보유한 생선 목록을 읽기 전용으로 반환한다. </summary>
    public IReadOnlyList<InventoryFishData> GetInventoryFishList()
    {
        return _saveData?.InvenFish;
    }

    /// <summary> KDH : ID에 해당하는 생선의 보유 개수를 반환한다. </summary>
    public int GetFishCount(int fishID)
    {
        if (_saveData?.InvenFish == null)
        {
            return 0;
        }

        InventoryFishData fishData = _saveData.InvenFish.Find(fish => fish.FishID == fishID);

        return fishData?.Count ?? 0;
    }

    /// <summary> KDH : 최초 게임 시작 시 기본 생선을 지급한다. </summary>
    private void GrantInitialFish()
    {
        for (int fishID = 0; fishID < 4; fishID++)
        {
            if (!TryAddFish(fishID, 5))
            {
                CPrint.Warning($"[DataManager] 튜토리얼 생선 지급 실패. FishID: {fishID}");
                return;
            }
        }
    }

    /// <summary> KDH : ID에 해당하는 생선을 개수만큼 추가한다. </summary>
    private bool TryAddFish(int fishID, int count)
    {
        if (fishID < 0 || count <= 0)
        {
            return false;
        }

        InventoryFishData fishData = _saveData.InvenFish.Find(fish => fish.FishID == fishID);

        if (fishData != null)
        {
            fishData.Count += count;
            return true;
        }

        _saveData.InvenFish.Add(new InventoryFishData
        {
            FishID = fishID,
            Count = count
        });

        return true;
    }

    #endregion ===== 인벤토리(생선) =====
    
    #region ===== 퀘스트 =====
    
    /// <summary> KDH : 씬별 서브 퀘스트 상태를 반환한다. </summary>
    public (int questID, int progress) GetSubQuestState(SceneType scene)
    {
        if (_saveData == null)
        {
            return (0, 0);
        }

        return scene switch
        {
            SceneType.TycoonScene => (_saveData.TycoonSubQuestID, _saveData.TycoonSubQuestProgress),
            SceneType.StealthScene => (_saveData.StealthSubQuestID, _saveData.StealthSubQuestProgress),
            _ => (0, 0),
        };
    }

    /// <summary> KDH : 씬별 서브 퀘스트의 ID와 진행도를 데이터에 설정한다. </summary>
    public void SetSubQuestState(SceneType scene, int questID, int progress)
    {
        if (_saveData == null)
        {
            return;
        }

        switch (scene)
        {
            case SceneType.TycoonScene:
                _saveData.TycoonSubQuestID = questID;
                _saveData.TycoonSubQuestProgress = Mathf.Max(0, progress);
                break;
            case SceneType.StealthScene:
                _saveData.StealthSubQuestID = questID;
                _saveData.StealthSubQuestProgress = Mathf.Max(0, progress);
                break;
        }
    }
    
    #endregion ===== 퀘스트 =====
}