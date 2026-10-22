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
    
    #region ===== 이벤트 =====
    
    public event Action OnGoldChanged;
    public event Action OnSatisfactionChanged;

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
    
    #region ===== 인벤토리(생선) =====

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
    public bool TryAddFish(int fishID, int count)
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
    
    #endregion ===== 인벤토리(생선) =====
}