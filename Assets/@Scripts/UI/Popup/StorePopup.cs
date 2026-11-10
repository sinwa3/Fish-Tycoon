using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> KDH : 상점 카테고리 전환과 상품 목록 UI를 관리한다. </summary>
public class StorePopup : BasePopup
{
    public enum Buttons
    {
        EquipmentTabBtn,
        ConsumableTabBtn,
        ExitBtn
    }

    public enum Texts
    {
        EquipmentTabText,
        ConsumableTabText,
    }

    #region ===== 참조 =====

    [Header("스크롤")]
    [SerializeField] private StoreEquipRecycleScroll _storeEquipRecycleScroll;
    [SerializeField] private StoreConsumableRecycleScroll _storeConsumableRecycleScroll;
    
    [Header("버튼")]
    [SerializeField] private Sprite _selectedTabSprite;
    [SerializeField] private Sprite _unselectedTabSprite;

    private UIHighlightCanvas _goldFrameHighlight;

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
        _goldFrameHighlight?.Restore();

        if (Managers.UI?.CurrentHUD is TycoonHUD tycoonHUD)
        {
            tycoonHUD.RestoreSubQuestHighlight();
        }
    }

    #region ===== 초기화 =====

    /// <summary> KDH : 하이라이트를 위해 골드 프레임 이미지를 설정한다. </summary>
    public void SetGoldFrameImage(Image goldFrameImage)
    {
        if (goldFrameImage == null)
        {
            return;
        }

        _goldFrameHighlight = goldFrameImage.gameObject.GetOrAddComponent<UIHighlightCanvas>();
        _goldFrameHighlight.Highlight(SortingOrder + 1);
    }

    /// <summary> KDH : 카테고리 전환 버튼 이벤트를 초기화한다. </summary>
    private void InitializeBtnEvent()
    {
        Get<Button>(Buttons.EquipmentTabBtn)?.onClick.AddListener(() => ChangeCategory(ItemType.Equipment));
        Get<Button>(Buttons.ConsumableTabBtn)?.onClick.AddListener(() => ChangeCategory(ItemType.Consumable));
        Get<Button>(Buttons.ExitBtn)?.onClick.AddListener(OnClickExit);
    }

    #endregion ===== 초기화 =====

    #region ===== 업데이트 =====

    /// <summary> KDH : 선택한 탭에 해당하는 상점 카테고리로 변경한다. </summary>
    private void ChangeCategory(ItemType itemType)
    {
        if (_currentType == itemType)
        {
            return;
        }

        _currentType = itemType;

        UpdateCategoryUI();
    }

    /// <summary> KDH : 현재 카테고리 타입에 맞게 UI를 업데이트 한다. </summary>
    private void UpdateCategoryUI()
    {
        UpdateTabUI(Buttons.EquipmentTabBtn, Texts.EquipmentTabText, _currentType == ItemType.Equipment);
        UpdateTabUI(Buttons.ConsumableTabBtn, Texts.ConsumableTabText, _currentType == ItemType.Consumable);

        _storeEquipRecycleScroll?.gameObject.SetActive(_currentType == ItemType.Equipment);
        _storeConsumableRecycleScroll?.gameObject.SetActive(_currentType == ItemType.Consumable);
    }

    /// <summary> KDH : 탭의 선택 상태에 맞게 배경 스프라이트와 글자 색상을 갱신한다. </summary>
    private void UpdateTabUI(Buttons buttonType, Texts textType, bool isSelected)
    {
        Button button = Get<Button>(buttonType);
        TMP_Text text = Get<TMP_Text>(textType);

        if (button != null && button.image != null)
        {
            button.image.sprite = isSelected ? _selectedTabSprite : _unselectedTabSprite;
        }

        if (text != null)
        {
            text.color = isSelected ? ColorKey.DarkBrown : ColorKey.DarkGray;
        }
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
