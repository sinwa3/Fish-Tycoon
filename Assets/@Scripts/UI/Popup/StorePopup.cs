using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> KDH : 상점 카테고리 전환과 상품 목록 UI를 관리한다. </summary>
public class StorePopup : BasePopup
{
    public enum Buttons
    {
        PrevBtn,
        NextBtn,
        ExitBtn
    }

    public enum Texts
    {
        CategoryText,
    }

    #region ===== 참조 =====

    [SerializeField] private StoreEquipRecycleScroll _storeEquipRecycleScroll;
    [SerializeField] private StoreConsumableRecycleScroll _storeConsumableRecycleScroll;

    #endregion ===== 참조 =====
    
    #region ===== 상태 =====

    private ItemType _currentType = ItemType.Equipment;

    #endregion ===== 상태 =====

    protected override void OnAwake()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));
    }

    protected override void OnStart()
    {
        Managers.Sound.PlaySfx(SoundKey.Name.SfxType.ChimePopup);
        
        InitializeBtnEvent();
        
        UpdateCategoryUI();
    }

    protected override void OnUpdate()
    {
    }

    public override void OnInputKey()
    {
        if (Managers.Input.KeyDown_Esc)
        {
            OnClickExit();
        }
    }

    protected override void DestroyOverride()
    {
    }

    #region ===== 초기화 =====

    /// <summary> KDH : 카테고리 전환 버튼 이벤트를 초기화한다. </summary>
    private void InitializeBtnEvent()
    {
        Get<Button>(Buttons.PrevBtn)?.onClick.AddListener(ChangeCategory);
        Get<Button>(Buttons.NextBtn)?.onClick.AddListener(ChangeCategory);
        Get<Button>(Buttons.ExitBtn)?.onClick.AddListener(OnClickExit);
    }

    #endregion ===== 초기화 =====

    #region ===== 업데이트 =====

    /// <summary> KDH : 방향에 따라 상점 카테고리를 변경한다. </summary>
    private void ChangeCategory()
    {
        _currentType = _currentType switch
        {
            ItemType.Equipment => ItemType.Consumable,
            ItemType.Consumable => ItemType.Equipment,
            _ => ItemType.Equipment
        };

        UpdateCategoryUI();
    }

    /// <summary> KDH : 현재 카테고리 타입에 맞게 UI를 업데이트 한다. </summary>
    private void UpdateCategoryUI()
    {
        string categoryName = _currentType switch
        {
            ItemType.Equipment => "장비",
            ItemType.Consumable => "소비",
            _ => string.Empty
        };

        Get<TMP_Text>(Texts.CategoryText)?.SetText(categoryName);
        
        _storeEquipRecycleScroll?.gameObject.SetActive(_currentType == ItemType.Equipment);
        _storeConsumableRecycleScroll?.gameObject.SetActive(_currentType == ItemType.Consumable);
    }

    #endregion ===== 업데이트 =====
    
    #region ===== 이벤트 =====

    /// <summary> KDH : Exit 버튼을 선택했을 때 팝업을 닫고 지정된 동작을 실행한다. </summary>
    private void OnClickExit()
    {
        Managers.Sound.PlaySfx(SoundKey.Name.SfxType.ChimeConfirm);
        Close();
    }

    #endregion ===== 이벤트 =====
}