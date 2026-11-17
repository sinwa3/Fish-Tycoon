using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TycoonHUD : BaseHUD
{
    public enum Texts
    {
        GoldValueText,
        ReputationValueText,
        SatisfactionValueText,
        CurrentDayText,

        MainQuestValueText,
        SubQuestDescText,
        FoodSaleLimitText,
    }

    public enum Sliders
    {
        MainQuestSlider,
        SatisfactionSlider,
        BusinessProgressSlider,
    }

    public enum Buttons
    {
        NextBtn,
        StartBusinessBtn,
        StoreBtn,
    }

    public enum Images
    {
        Command01,
        Command02,
        Command03,
        Command04,
        Command05,
        Command06,
        Command07,
        Command08,
        Command09,
        Command10,
        Command11,
        Command12,
        ServingImage,
        CookingImage,
        CurrentFoodImage,
        NextFoodImage,
        ServingFoodImage,

        GoldFrameImage,
        ReputationIcon,

        QuickSlotBar,

        MainQuest,
        SubQuest
    }

    public enum Toggles
    {
        SubQuestToggle
    }

    #region ===== 참조 =====

    private TycoonScene _tycoonScene;

    private CanvasGroup _foodDiaryCG;

    private ReputationTooltip _reputationTooltip;

    private CookingController _cookingController;

    #endregion ===== 참조 =====

    #region ===== 상태 =====

    // 영업 진행 여부 (영업 중에는 판매 금액을 메인 퀘스트 게이지에 반영)
    private bool _salesInProgress;
    
    // 골드 아이콘이 도착하여 메인 퀘스트 게이지에 반영된 누적 판매 금액
    private int _salesProgressGold;
    // 아직 골드 아이콘이 도착하지 않아 메인 퀘스트 게이지에 반영하지 않은 퀘스트 보상 금액
    private int _pendingQuestGold;
    // 서브 퀘스트 완료 연출 후 골드 아이콘으로 이동시킬 예정인 보상 금액
    private int _questGoldWaitingToFly;
    
    // 현재 메인 퀘스트 게이지로 이동 중인 골드 아이콘 개수
    private int _flyingGoldIcons;
    // 이동 중인 골드 아이콘이 있는지 확인
    public bool HasFlyingGold => _flyingGoldIcons > 0;

    #endregion ===== 상태 =====

    #region ===== 애니메이션 =====

    // 다이어리
    private RectTransform _diaryRectTransform;
    private Vector2 _diaryOriginPos;
    private Tween _diaryTween;

    // 상점 버튼
    private Tween _storeBtnTween;

    // 퀘스트
    private Tween _mainQuestTween;
    private Tween _subQuestTween;
    private Coroutine _subQuestCoroutine;
    private UIHighlightCanvas _subQuestHighlight;

    // 골드
    private Tween _goldTween;
    private int _displayedGold;

    #endregion ===== 애니메이션 =====

    #region ==== 조리 커맨드 ====

    private Sprite[] _normalSprites = new Sprite[4];
    private Sprite[] _pressedSprites = new Sprite[4];
    private Sprite[] _failedSprites = new Sprite[4];
    private Sprite[] _highlightSprites = new Sprite[4];

    private Image[] _commandImages;

    private const string TycoonSpritePath = "Sprite/Tycoon/";

    #endregion ==== 조리 커맨드 ====

    protected override void OnAwake()
    {
        Bind<TMP_Text>(typeof(Texts));
        Bind<Slider>(typeof(Sliders));
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<Toggle>(typeof(Toggles));

        _tycoonScene = gameObject.FindParent<TycoonScene>();

        // SJY : TycoonScene에서 CookingController를 가져옴
        if (_tycoonScene?.CookingController != null)
        {
            _cookingController = _tycoonScene?.CookingController;
        }

        ResolveFoodDiary();

        InitializeReputationTooltip();
        InitializeCookingSprites();
        InitializeCommandImages();

        Get<Image>(Images.ServingImage)?.gameObject.SetActive(false);
        Get<Image>(Images.CookingImage)?.gameObject.SetActive(false);
        Get<Image>(Images.ServingFoodImage)?.gameObject.SetActive(false);
    }

    protected override void OnStart()
    {
        InitializeBtnEvent();
        InitializeMainQuest();

        SetCurrentDayText();
        SetMainQuestValueText();
        SetReputationIconHover();

        UpdateGoldText();
        UpdateSatisfactionInfo();

        SubscribeGoldChanged();
        SubscribeSatisfactionChanged();

        SubscribeCookingEvents();

        SubscribeSubQuestEvents();

        if (Managers.Quest != null)
        {
            UpdateSubQuest(Managers.Quest.GetCurrentQuest(SceneType.TycoonScene, out var progress), progress);
        }
    }

    protected override void OnUpdate()
    {
    }

    public override void OnInputKey()
    {
        base.OnInputKey();

        if (Managers.Input.KeyDown_Esc)
        {
            Managers.UI.OpenPopup<PausePopup>();
        }

        // SJY : 조리 중 WASD 커맨드 입력
        if (_cookingController != null)
        {
            if (_cookingController.State != CookingState.Playing || !Managers.Input.CanReceivePlayer)
            {
                return;
            }

            if (Managers.Input.KeyDown_W)
            {
                Managers.Sound?.PlaySfx(SoundKey.Name.SfxType.ButtonSound_1);
                _cookingController.ProcessCommand('W');
            }

            else if (Managers.Input.KeyDown_A)
            {
                Managers.Sound?.PlaySfx(SoundKey.Name.SfxType.ButtonSound_2);
                _cookingController.ProcessCommand('A');
            }

            else if (Managers.Input.KeyDown_S)
            {
                Managers.Sound?.PlaySfx(SoundKey.Name.SfxType.ButtonSound_3);
                _cookingController.ProcessCommand('S');
            }

            else if (Managers.Input.KeyDown_D)
            {
                Managers.Sound?.PlaySfx(SoundKey.Name.SfxType.ButtonSound_4);
                _cookingController.ProcessCommand('D');
            }
        }
    }

    private void OnDestroy()
    {
        _mainQuestTween?.Kill();
        _subQuestTween?.Kill();
        _diaryTween?.Kill();
        _storeBtnTween?.Kill();

        UnsubscribeGoldChanged();
        UnsubscribeSatisfactionChanged();

        UnsubscribeCookingEvents();

        UnsubscribeSubQuestEvents();
    }

    #region ===== 초기화 =====

    /// <summary> KDH : 버튼 이벤트를 초기화한다. </summary>
    private void InitializeBtnEvent()
    {
        Get<Button>(Buttons.NextBtn)?.onClick.AddListener(() => _tycoonScene.ChangeCurrentPhase());
        Get<Button>(Buttons.StartBusinessBtn)?.onClick.AddListener(CloseFoodDiary);
        Get<Button>(Buttons.StoreBtn)?.onClick.AddListener(() =>
        {
            StorePopup storePopup = Managers.UI.OpenPopup<StorePopup>();
            storePopup.SetGoldFrameImage(Get<Image>(Images.GoldFrameImage));
        });
    }

    /// <summary> KDH : 타이쿤 씬 첫 진입시 현재 영업 날짜를 세팅한다. </summary>
    private void SetCurrentDayText()
    {
        Get<TMP_Text>(Texts.CurrentDayText)?.SetText(Managers.Data.GetCurrentDay().ToString());
    }

    /// <summary> KDH : 타이쿤 씬 첫 진입시 목표 금액을 설정한다. </summary>
    private void SetMainQuestValueText(int? currentGold = null)
    {
        int gold = currentGold ?? Managers.Data.GetCurrentGold();
        Get<TMP_Text>(Texts.MainQuestValueText)?.SetText(
            gold >= TycoonScene.TargetGold ? $"{gold:N0} G" : $"({gold:N0}/{TycoonScene.TargetGold:N0})");
    }

    /// <summary> KDH : 평판 아이콘에 호버시 평판 정보를 보여주도록 한다. </summary>
    private void SetReputationIconHover()
    {
        Get<Image>(Images.ReputationIcon)?.gameObject.BindEvent(UIEventType.PointerEnter, OnPointerEnterReputation);
        Get<Image>(Images.ReputationIcon)?.gameObject.BindEvent(UIEventType.PointerExit, OnPointerExitReputation);
    }

    /// <summary> KDH : 음식 메뉴 리스트를 참조한다. </summary>
    private void ResolveFoodDiary()
    {
        _foodDiaryCG = gameObject.FindChild<CanvasGroup>("FoodDiaryCG", true);
        if (_foodDiaryCG != null)
        {
            _diaryRectTransform = _foodDiaryCG.GetComponent<RectTransform>();
            _diaryOriginPos = _diaryRectTransform.anchoredPosition;
        }
    }

    /// <summary> KDH : 영업 진행도를 초기화한다. </summary>
    public void InitializeBusinessProgress(int maxCustomerCount)
    {
        Slider businessProgressSlider = Get<Slider>(Sliders.BusinessProgressSlider);

        if (businessProgressSlider != null)
        {
            businessProgressSlider.minValue = 0;
            businessProgressSlider.maxValue = Mathf.Max(1, maxCustomerCount);
            businessProgressSlider.SetValueWithoutNotify(0);
        }
    }

    /// <summary> KDH : 메인퀘스트에서 영업 중에는 누적 판매액이 반영되는 목표 게이지를 표시한다. </summary>
    public void InitializeMainQuest()
    {
        Slider mainQuestSlider = Get<Slider>(Sliders.MainQuestSlider);

        int startingGold = Managers.Data?.GetCurrentDay() ?? 0;
        _salesProgressGold = 0;

        if (mainQuestSlider != null)
        {
            mainQuestSlider.maxValue = TycoonScene.TargetGold;
            mainQuestSlider.SetValueWithoutNotify(startingGold);
        }
    }

    /// <summary> KDH : 메인퀘스트의 진행도를 설정한다. </summary>
    public void SetMainQuestInProgress(bool inProgress)
    {
        _salesInProgress = inProgress;
        RefreshMainQuestProgress();
    }

    /// <summary> KDH : 평판 툴팁을 초기화한다. </summary>
    private void InitializeReputationTooltip()
    {
        if (_reputationTooltip == null)
        {
            _reputationTooltip = Managers.Resource.Spawn(ResourceKey.Path.TooltipUI + nameof(ReputationTooltip),
                transform).gameObject.GetOrAddComponent<ReputationTooltip>();
        }

        _reputationTooltip.gameObject.SetActive(false);
    }

    /// <summary>SJY : 커맨드 UI 이미지 배열 초기화</summary>
    private void InitializeCommandImages()
    {
        _commandImages = new Image[12];

        for (int i = 0; i < _commandImages.Length; i++)
        {
            _commandImages[i] = Get<Image>((Images)i);

            if (_commandImages[i] != null)
            {
                _commandImages[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>SJY : 조리 커맨드 및 서빙 스프라이트 초기화</summary>
    private void InitializeCookingSprites()
    {
        string[] commands = { "W", "A", "S", "D" };

        for (int i = 0; i < commands.Length; i++)
        {
            string sheetName = $"Command_{commands[i]}-Sheet";
            string path = TycoonSpritePath + sheetName;

            _normalSprites[i] = Managers.Resource.LoadSprite(path, sheetName + "_0");
            _pressedSprites[i] = Managers.Resource.LoadSprite(path, sheetName + "_1");
            _failedSprites[i] = Managers.Resource.LoadSprite(path, sheetName + "_2");
            _highlightSprites[i] = Managers.Resource.LoadSprite(path, sheetName + "_3");
        }

        Image servingImage = Get<Image>(Images.ServingImage);

        if (servingImage != null)
        {
            servingImage.sprite = Managers.Resource.LoadSprite(TycoonSpritePath + "2DMiniGame-Sheet", "serving_0");
        }
    }

    #endregion ===== 초기화 =====

    #region ===== 업데이트 =====

    /// <summary> KDH : 현재 골드 값을 UI에 즉시 적용한다. </summary>
    private void UpdateGoldText()
    {
        _goldTween?.Kill();
        _goldTween = null;

        _displayedGold = Managers.Data.GetCurrentGold();

        Get<TMP_Text>(Texts.GoldValueText)?.SetText($"{_displayedGold:N0} G");
    }

    /// <summary> KDH : 현재 만족도와 평판 등급을 업데이트한다. </summary>
    private void UpdateSatisfactionInfo()
    {
        if (Managers.Table.Reputation == null)
        {
            CPrint.Error("[TycoonHUD] : 현재 테이블 데이터가 존재하지 않습니다.");
            return;
        }

        var info = Managers.Data.GetReputationInfo();

        Get<TMP_Text>(Texts.ReputationValueText)?.SetText(info.reputationGrade.ToString("N0"));
        Get<TMP_Text>(Texts.SatisfactionValueText)
            ?.SetText($"{info.currentSatisfaction:N0} / {info.requiredSatisfaction:N0}");

        bool isMaxGrade = Managers.Table.Reputation.IsMaxReputationGrade(info.reputationGrade);

        if (isMaxGrade) // 만렙인경우
        {
            Get<Slider>(Sliders.SatisfactionSlider)?.SetValueWithoutNotify(1f);
            Get<TMP_Text>(Texts.SatisfactionValueText)?.SetText("MAX");

            return;
        }

        float progress = info.requiredSatisfaction > 0
            ? (float)info.currentSatisfaction / info.requiredSatisfaction
            : 0f;
        Get<Slider>(Sliders.SatisfactionSlider)?.SetValueWithoutNotify(progress);
    }

    /// <summary> KDH : 음식 서빙이 완료되면 영업 진행도를 갱신한다. </summary>
    public void UpdateBusinessProgress()
    {
        if (_tycoonScene == null)
        {
            return;
        }

        Slider businessProgressSlider = Get<Slider>(Sliders.BusinessProgressSlider);

        if (businessProgressSlider != null)
        {
            _tycoonScene.ServedCount = Mathf.Min(_tycoonScene.ServedCount + 1, (int)businessProgressSlider.maxValue);
            businessProgressSlider.SetValueWithoutNotify(_tycoonScene.ServedCount);
        }
    }

    /// <summary> KDH : 해당 퀘스트 정보로 UI를 업데이트한다. </summary>
    private void UpdateSubQuest(QuestData quest, int progress, bool isCompleted = false)
    {
        if (quest == null)
        {
            Get<Image>(Images.SubQuest)?.gameObject.SetActive(false);
            return;
        }

        Get<TMP_Text>(Texts.SubQuestDescText)
            ?.SetText($"{quest.Description}\n({Mathf.Clamp(progress, 0, quest.Goal)} / {quest.Goal})\n");
        Get<Toggle>(Toggles.SubQuestToggle)?.SetIsOnWithoutNotify(isCompleted);
    }

    #endregion ===== 업데이트 =====

    #region ===== 활성화 =====

    /// <summary> KDH : 다음 버튼[잠입전환] 표시/비표시한다. </summary>
    public void SetNextBtnVisible(bool visible)
    {
        Get<Button>(Buttons.NextBtn)?.gameObject.SetActive(visible);
    }

    /// <summary> KDH : 상점 버튼을 표시하고 등장 애니메이션을 재생한다. </summary>
    public void SetStoreBtnVisible(bool isVisible)
    {
        Button storeBtn = Get<Button>(Buttons.StoreBtn);

        if (storeBtn == null)
        {
            return;
        }

        _storeBtnTween?.Kill();

        if (!isVisible)
        {
            storeBtn.gameObject.SetActive(false);
            storeBtn.transform.localScale = Vector3.one;
            return;
        }

        storeBtn.transform.localScale = Vector3.one * 0.6f;
        storeBtn.gameObject.SetActive(true);

        _storeBtnTween = storeBtn.transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutBounce);
    }

    /// <summary> KDH : 가게 평판/만족도/소지금을 표시/비표시한다. </summary>
    public void SetStoreInfoVisible(bool isVisible)
    {
        Get<Slider>(Sliders.SatisfactionSlider)?.gameObject.SetActive(isVisible);
        Get<Image>(Images.GoldFrameImage)?.gameObject.SetActive(isVisible);
    }

    /// <summary> KDH : 영업 진행도를 표시/비표시한다. </summary>
    public void SetBusinessProgressVisible(bool isVisible)
    {
        Get<Slider>(Sliders.BusinessProgressSlider)?.gameObject.SetActive(isVisible);
    }

    /// <summary> KDH : 퀵슬롯을 표시/비표시한다. </summary>
    public void SetQuickSlotVisible(bool isVisible)
    {
        Get<Image>(Images.QuickSlotBar)?.gameObject.SetActive(isVisible);
    }

    /// <summary> KDH : 퀘스트를 표시/비표시한다. </summary>
    public void SetQuestVisible(bool isMainVisible, bool isSubVisible)
    {
        Get<Image>(Images.MainQuest)?.gameObject.SetActive(isMainVisible);
        Get<Image>(Images.SubQuest)?.gameObject.SetActive(isSubVisible);
    }

    /// <summary> KDH : 음식 다이어리에 판매 가능 개수를 표시한다. </summary>
    public void SetFoodSaleLimit(int count)
    {
        Get<TMP_Text>(Texts.FoodSaleLimitText)?.SetText("판매 가능 개수 : {0}", count);
    }

    /// <summary> KDH : 음식 다이어리를 오픈한다. </summary>
    public void OpenFoodDiary()
    {
        if (_foodDiaryCG == null || _diaryRectTransform == null)
        {
            return;
        }

        _diaryTween?.Kill();

        _diaryRectTransform.anchoredPosition = _diaryOriginPos + Vector2.down * 100f;

        _foodDiaryCG.alpha = 0f;
        _foodDiaryCG.interactable = false;
        _foodDiaryCG.blocksRaycasts = true;

        Sequence sequence = DOTween.Sequence();

        sequence.Append(_diaryRectTransform.DOAnchorPos(_diaryOriginPos, 0.45f).SetEase(Ease.OutCubic));
        sequence.Join(_foodDiaryCG.DOFade(1f, 0.45f));
        sequence.OnComplete(() => { _foodDiaryCG.interactable = true; });

        _diaryTween = sequence;
    }

    /// <summary> KDH : 음식 다이어리를 닫고 영업 중으로 전환한다. </summary>
    private void CloseFoodDiary()
    {
        if (_foodDiaryCG == null || _diaryRectTransform == null)
        {
            return;
        }

        _diaryTween?.Kill();

        _foodDiaryCG.interactable = false;
        _foodDiaryCG.blocksRaycasts = true;

        Sequence sequence = DOTween.Sequence();

        sequence.Append(_diaryRectTransform.DOAnchorPos(_diaryOriginPos + Vector2.down * 100f, 0.45f)
            .SetEase(Ease.InCubic));
        sequence.Join(_foodDiaryCG.DOFade(0f, 0.45f));
        sequence.OnComplete(() =>
        {
            _foodDiaryCG.blocksRaycasts = false;
            _tycoonScene.ChangeCurrentPhase();
            DeactivateFoodDiary();
        });

        _diaryTween = sequence;
    }

    /// <summary> KDH : 다이어리를 비활성화한다. </summary>
    public void DeactivateFoodDiary()
    {
        if (_foodDiaryCG == null)
        {
            return;
        }

        _diaryTween?.Kill();
        _foodDiaryCG.transform.parent.gameObject.SetActive(false);
    }

    #endregion ===== 활성화 =====

    #region ===== 애니메이션 =====

    /// <summary> KDH : 현재 표시 중인 골드에서 최신 골드까지 숫자 변경 애니메이션을 재생한다. </summary>
    private void AnimateGoldText()
    {
        TMP_Text goldText = Get<TMP_Text>(Texts.GoldValueText);

        if (goldText == null)
        {
            return;
        }

        int targetGold = Managers.Data.GetCurrentGold();
        RefreshMainQuestProgress();
        
        _goldTween?.Kill();
        _goldTween = null;

        if (_displayedGold == targetGold)
        {
            return;
        }

        _goldTween = DOVirtual.Int(_displayedGold, targetGold, 0.5f, value =>
        {
            _displayedGold = value;
            goldText.SetText($"{value:N0} G");
        }).SetEase(Ease.OutCubic).SetLink(gameObject).OnComplete(() =>
        {
            _displayedGold = targetGold;
            goldText.SetText($"{targetGold:N0} G");
            _goldTween = null;
        });
    }

    /// <summary> KDH : 상점 팝업 위에 서브 퀘스트를 강조한다. </summary>
    private void HighlightSubQuest()
    {
        if (Managers.UI == null || Managers.UI.CurrentPopup is not StorePopup storePopup)
        {
            return;
        }

        Image subQuestImage = Get<Image>(Images.SubQuest);

        if (subQuestImage == null)
        {
            return;
        }

        _subQuestHighlight = subQuestImage.gameObject.GetOrAddComponent<UIHighlightCanvas>();
        _subQuestHighlight.Highlight(storePopup.SortingOrder + 1);
    }

    /// <summary> KDH : 서브 퀘스트 하이라이트를 복원한다. </summary>
    public void RestoreSubQuestHighlight()
    {
        if (_subQuestHighlight == null)
        {
            return;
        }

        _subQuestHighlight.Restore();
        _subQuestHighlight = null;
    }

    #endregion ===== 애니메이션 =====

    #region ==== 조리 커맨드 UI ====

    /// <summary>SJY : 커맨드 문자에 해당하는 스프라이트 인덱스 반환</summary>
    private int GetCommandSpriteIndex(char command)
    {
        return "WASD".IndexOf(command);
    }

    /// <summary>SJY : 현재 음식의 커맨드 이미지 표시 및 초기화</summary>
    private void ResetCommandImages()
    {
        if (_cookingController == null || _commandImages == null)
        {
            return;
        }

        int commandCount = _cookingController.TotalCommandCount;

        for (int i = 0; i < _commandImages.Length; i++)
        {
            Image image = _commandImages[i];

            if (image == null)
            {
                continue;
            }

            bool isActive = i < commandCount;
            image.gameObject.SetActive(isActive);

            if (!isActive)
            {
                continue;
            }

            char command = _cookingController.GetCommand(i);
            int spriteIndex = GetCommandSpriteIndex(command);

            if (spriteIndex < 0 || spriteIndex >= _normalSprites.Length)
            {
                continue;
            }

            image.sprite = _normalSprites[spriteIndex];
        }

        UpdateCommandHighlight(0);
    }

    /// <summary>SJY : 정답 커맨드 이미지 변경</summary>
    private void UpdateCommandPressed(int commandIndex)
    {
        UpdateCommandSprite(commandIndex, _pressedSprites);

        UpdateCommandHighlight(commandIndex + 1);

        UpdateCookingImage(commandIndex);
    }

    /// <summary>SJY : 오답 커맨드 이미지 변경</summary>
    private void UpdateCommandFailed(int commandIndex)
    {
        UpdateCommandSprite(commandIndex, _failedSprites);

        ShowCookingFailed();
    }

    /// <summary>SJY : 지정된 커맨드의 스프라이트 변경</summary>
    private void UpdateCommandSprite(int commandIndex, Sprite[] sprites)
    {
        if (_cookingController == null || _commandImages == null || commandIndex < 0 ||
            commandIndex >= _commandImages.Length)
        {
            return;
        }

        Image image = _commandImages[commandIndex];

        if (image == null)
        {
            return;
        }

        char command = _cookingController.GetCommand(commandIndex);
        int spriteIndex = GetCommandSpriteIndex(command);

        if (spriteIndex < 0 || spriteIndex >= sprites.Length)
        {
            return;
        }

        image.sprite = sprites[spriteIndex];
    }

    /// <summary>SJY : 조리 완료 시 커맨드 UI 비활성화 및 서빙 이미지 출력</summary>
    private void HandleCookingCompleted(int foodId)
    {
        for (int i = 0; i < _commandImages.Length; i++)
        {
            if (_commandImages[i] != null)
            {
                _commandImages[i].gameObject.SetActive(false);
            }
        }

        Get<Image>(Images.CookingImage)?.gameObject.SetActive(false);

        Image servingImage = Get<Image>(Images.ServingImage);

        if (servingImage != null && servingImage.sprite != null)
        {
            servingImage.gameObject.SetActive(true);
        }

        UpdateServingFoodImage(foodId);
    }

    /// <summary>SJY : 서빙 연출 종료 시 이미지 비활성화</summary>
    private void HandleServingFinished()
    {
        Get<Image>(Images.ServingImage)?.gameObject.SetActive(false);
        Get<Image>(Images.ServingFoodImage)?.gameObject.SetActive(false);
    }

    /// <summary>SJY : 커맨드 개수와 진행도에 따라 조리 이미지 변경</summary>
    private void UpdateCookingImage(int commandIndex)
    {
        if (_cookingController == null)
        {
            return;
        }

        int commandCount = _cookingController.TotalCommandCount;

        if (commandCount < 4 || commandIndex < 0 || commandIndex >= commandCount)
        {
            return;
        }

        string[] stageNames = { "cutting", "rice", "wasabi", "finish" };

        int baseCount = commandCount / stageNames.Length;
        int remainder = commandCount % stageNames.Length;

        int startIndex = 0;

        for (int i = 0; i < stageNames.Length; i++)
        {
            int stageCount = baseCount + (i < remainder ? 1 : 0);

            if (commandIndex < startIndex + stageCount)
            {
                int stageIndex = commandIndex - startIndex;

                // SJY : 커맨드 수가 6일때 wasabi, finish에서 1번만 사용
                int spriteIndex = commandCount == 6 && i >= 2 ? 1 : stageIndex % 2;

                string spriteName = $"{stageNames[i]}_{spriteIndex}";

                Image cookingImage = Get<Image>(Images.CookingImage);

                if (cookingImage == null)
                {
                    return;
                }

                cookingImage.sprite = Managers.Resource.LoadSprite(TycoonSpritePath + "2DMiniGame-Sheet", spriteName);
                cookingImage.gameObject.SetActive(cookingImage.sprite != null);

                return;
            }

            startIndex += stageCount;
        }
    }

    /// <summary>SJY : 커맨드 입력 실패 시 조리 실패 이미지 표시</summary>
    private void ShowCookingFailed()
    {
        Image cookingImage = Get<Image>(Images.CookingImage);

        if (cookingImage == null)
        {
            return;
        }

        cookingImage.sprite = Managers.Resource.LoadSprite(TycoonSpritePath + "2DMiniGame-Sheet", "wrongaction");
        cookingImage.gameObject.SetActive(cookingImage.sprite != null);
    }

    /// <summary>SJY : 현재 음식의 커맨드 이미지 표시 및 초기화</summary>
    private void ResetCookingImage()
    {
        Image cookingImage = Get<Image>(Images.CookingImage);

        if (cookingImage == null)
        {
            return;
        }

        if (_cookingController == null || _cookingController.TotalCommandCount == 0)
        {
            cookingImage.gameObject.SetActive(false);

            return;
        }

        cookingImage.sprite = Managers.Resource.LoadSprite(TycoonSpritePath + "2DMiniGame-Sheet", "idle");
        cookingImage.gameObject.SetActive(cookingImage.sprite != null);
    }

    /// <summary>SJY : 현재 음식과 다음 음식 아이콘 갱신</summary>
    private void UpdateFoodImages()
    {
        if (_cookingController == null)
        {
            return;
        }

        UpdateFoodImage(Get<Image>(Images.CurrentFoodImage), _cookingController.CurrentFoodID);
        UpdateFoodImage(Get<Image>(Images.NextFoodImage), _cookingController.NextFoodID);
    }

    /// <summary>SJY : 음식 ID로 테이블을 조회하여 아이콘 표시</summary>
    private void UpdateFoodImage(Image image, int? foodId)
    {
        if (image == null)
        {
            return;
        }

        if (!foodId.HasValue || !Managers.Table.Food.Map.TryGetValue(foodId.Value, out FoodData foodData))
        {
            image.gameObject.SetActive(false);

            return;
        }

        image.sprite = Managers.Table.Food.GetSpriteByFoodID(foodData.ID);
        image.gameObject.SetActive(image.sprite != null);
    }

    /// <summary>SJY : 서빙할 음식을 접시위에 표시</summary>
    private void UpdateServingFoodImage(int foodId)
    {
        Image servingFoodImage = Get<Image>(Images.ServingFoodImage);

        if (servingFoodImage == null)
        {
            return;
        }

        if (!Managers.Table.Food.Map.TryGetValue(foodId, out FoodData foodData))
        {
            servingFoodImage.gameObject.SetActive(false);

            return;
        }

        servingFoodImage.sprite = Managers.Table.Food.GetSpriteByFoodID(foodData.ID);
        servingFoodImage.gameObject.SetActive(servingFoodImage.sprite != null);
    }

    /// <summary>SJY : 현재 입력해야 하는 커맨드를 테두리 스프라이트로 변경</summary>
    private void UpdateCommandHighlight(int commandIndex)
    {
        if (_cookingController == null || _commandImages == null || commandIndex < 0 ||
            commandIndex >= _cookingController.TotalCommandCount)
        {
            return;
        }

        UpdateCommandSprite(commandIndex, _highlightSprites);
    }

    #endregion ==== 조리 커맨드 UI ====

    #region ===== 퀘스트 =====

    /// <summary> KDH : 메인 퀘스트 관련 정보를 업데이트 한다. </summary>
    private void RefreshMainQuestProgress()
    {
        int gold = Managers.Data.GetCurrentGold() - _pendingQuestGold;
        if (_salesInProgress)
        {
            gold += _salesProgressGold;
        }

        Get<Slider>(Sliders.MainQuestSlider)?.SetValueWithoutNotify(gold);
        SetMainQuestValueText(gold);
    }

    /// <summary> 손님의 음식값을 화폐 아이콘으로 표시하고 도착할 때 목표 게이지에 반영한다. </summary>
    public void PlayCustomerPayAnimation(Vector2 screenPosition, int price)
    {
        Slider mainQuestSlider = Get<Slider>(Sliders.MainQuestSlider);

        if (mainQuestSlider == null || price <= 0)
        {
            return;
        }

        Canvas canvas = GetComponent<Canvas>();
        RectTransform rect = transform as RectTransform;

        if (canvas != null)
        {
            // 손님의 화면 좌표를 UI 좌표로 변환
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPosition,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out Vector2 start);

            PlayMovingGoldAnimation(start, price, false);
        }
    }

    /// <summary> KDH : 골드 아이콘이 메인퀘스트 창으로 이동하는 애니메이션을 재생한다. </summary>
    private void PlayMovingGoldAnimation(Vector2 start, int price, bool isQuestReward)
    {
        // 고가 메뉴에서도 화면을 가리지 않도록 한 건당 최대 10장으로 표시한다.
        int count = Mathf.Clamp(price / 100, 1, 10);
        for (int i = 0; i < count; i++)
        {
            int amount = price / count + (i < price % count ? 1 : 0);

            // 골드 아이콘 생성
            Image icon = new GameObject("GoldIcon").GetOrAddComponent<Image>();
            icon.gameObject.layer = transform.gameObject.layer;
            icon.transform.SetParent(transform, false);
            icon.color = Color.white;
            icon.raycastTarget = false;
            icon.sprite = Managers.Resource.LoadSprite(ResourceKey.Path.ItemSprite, ResourceKey.Name.Gold);
            icon.preserveAspect = true;
            icon.rectTransform.sizeDelta = new Vector2(120f, 84f);

            // 골드 아이콘을 기준 위치 주변에 겹치도록 배치
            float x = (i % 5 - 2) * 22f; // 가로: -44, -22, 0, 22, 44 반복
            float y = 36f + (i % 3) * 20f; // 세로: 36, 56, 76 반복

            // 기준 위치에서 계산한 만큼 이동
            Vector2 origin = start + new Vector2(x, y);
            icon.rectTransform.localPosition = origin;

            // 목적지 설정
            RectTransform destination = Get<Slider>(Sliders.MainQuestSlider).GetComponent<RectTransform>();
            _flyingGoldIcons++;
            // 0.45초 동안 진행률을 0에서 1까지 증가시킨다.
            DOVirtual.Float(0f, 1f, 0.45f, progress =>
                {
                    // 메인 퀘스트 UI의 월드 위치를 TycoonHUD 기준 로컬 위치로 변환
                    Vector2 target = transform.InverseTransformPoint(destination.position);
                    // 출발점에서 도착점까지 진행률에 따라 위치 계산
                    Vector2 point = Vector2.Lerp(origin, target, progress);
                    // 이동 경로 중간에서 최대 48만큼 위로 솟아오르는 효과
                    point.y += Mathf.Sin(progress * Mathf.PI) * 48f;
                    // 계산된 위치를 골드 아이콘에 적용
                    icon.rectTransform.localPosition = point;
                })
                .SetDelay(0.18f * i / count) // // 골드 아이콘마다 이동 시작 시간을 조금씩 다르게 설정
                .SetLink(icon.gameObject) // // 골드 아이콘이 파괴되면 Tween도 자동 종료
                .OnComplete(() => // // 골드 아이콘이 목적지에 도착했을 때 실행
                {
                    _flyingGoldIcons--;
                    if (isQuestReward)
                    {
                        _pendingQuestGold -= amount;
                    }
                    else
                    {
                        _salesProgressGold += amount;
                    }

                    RefreshMainQuestProgress();
                    PlayMainQuestBounce();
                    Destroy(icon.gameObject);
                });
        }
    }

    /// <summary> KDH : 메인퀘스트 스케일 바운스 애니메이션을 재생한다. </summary>
    private void PlayMainQuestBounce()
    {
        Image image = Get<Image>(Images.MainQuest);

        if (image == null)
        {
            return;
        }

        if (_mainQuestTween != null && _mainQuestTween.IsActive())
        {
            return;
        }

        Transform mainQuest = image.transform;
        Vector3 restingScale = mainQuest.localScale;
        _mainQuestTween = mainQuest.DOScale(restingScale * 1.1f, 0.17f)
            .SetEase(Ease.OutBack)
            .SetLoops(2, LoopType.Yoyo)
            .OnComplete(() => mainQuest.localScale = restingScale);
    }

    /// <summary> KDH : 서브퀘스트 완료됐을 때 연출을 재생한다. </summary>
    private void StartSubQuestCompletion(QuestProgressEvent progressEvent)
    {
        if (_subQuestCoroutine != null)
        {
            StopCoroutine(_subQuestCoroutine);
            _pendingQuestGold -= _questGoldWaitingToFly;
            _questGoldWaitingToFly = 0;
            RefreshMainQuestProgress();
        }

        _subQuestCoroutine = StartCoroutine(Co_ShowSubQuestCompletion(progressEvent));
    }

    /// <summary> KDH : 서브 퀘스트 완료 연출을 재생한다. </summary>
    private IEnumerator Co_ShowSubQuestCompletion(QuestProgressEvent progressEvent)
    {
        Managers.Sound?.PlaySfx(SoundKey.Name.SfxType.QuestClear);

        // 골드 보상시 메인퀘스트로 골드아이콘을 이동시킬 준비를 한다.
        int reward = progressEvent.CompletedQuest.GoldReward;
        if (reward > 0)
        {
            _pendingQuestGold += reward;
            _questGoldWaitingToFly = reward;
            RefreshMainQuestProgress();
        }
        
        // 완료 표시
        UpdateSubQuest(progressEvent.CompletedQuest, progressEvent.CompletedQuest.Goal, true);
        Managers.Quest.SetCompletionSequenceActive(true);

        // 상점 팝업이 열려 있다면 하이라이트
        HighlightSubQuest();

        // 스케일 바운스
        PlaySubQuestBounce();

        if (_subQuestTween != null)
        {
            yield return _subQuestTween.WaitForCompletion();
        }

        // 보상 표시
        Get<TMP_Text>(Texts.SubQuestDescText)?.SetText($"{progressEvent.CompletedQuest.GoldReward:N0} G 획득!");

        if (reward > 0)
        {
            _questGoldWaitingToFly = 0;
            
            Image SubQuestImage = Get<Image>(Images.SubQuest);
            if (SubQuestImage != null)
            {
                PlayMovingGoldAnimation(transform.InverseTransformPoint(SubQuestImage.rectTransform.position), reward, true);
            }
        }
        
        yield return new WaitForSeconds(1f);

        // 바운스 종료 후 하이라이트 복원
        RestoreSubQuestHighlight();

        // 다음 퀘스트 표시
        UpdateSubQuest(progressEvent.NextQuest, 0);
        Managers.Quest.SetCompletionSequenceActive(false);

        _subQuestCoroutine = null;
    }

    /// <summary> KDH : 서브 퀘스트 완료 시 스케일 바운스 애니메이션을 재생한다. </summary>
    private void PlaySubQuestBounce()
    {
        Image subQuestImage = Get<Image>(Images.SubQuest);

        if (subQuestImage == null)
        {
            return;
        }

        RectTransform rectTransform = subQuestImage.rectTransform;

        _subQuestTween?.Kill();

        rectTransform.localScale = Vector3.one;

        _subQuestTween = rectTransform.DOScale(1.15f, 0.4f).SetEase(Ease.OutBack).SetLoops(2, LoopType.Yoyo)
            .OnComplete(() =>
            {
                rectTransform.localScale = Vector3.one;
                _subQuestTween = null;
            });
    }

    #endregion ===== 퀘스트 =====

    #region ===== 이벤트 =====

    /// <summary> KDH : 골드 변화시 UI 업데이트를 구독 </summary>
    private void SubscribeGoldChanged()
    {
        if (Managers.Data != null)
        {
            Managers.Data.OnGoldChanged += AnimateGoldText;
        }
    }

    /// <summary> KDH : 골드 변화시 UI 업데이트를 구독해제 </summary>
    private void UnsubscribeGoldChanged()
    {
        if (Managers.Data != null)
        {
            Managers.Data.OnGoldChanged -= AnimateGoldText;
        }
    }

    /// <summary> KDH : 만족도 변화시 UI 업데이트를 구독 </summary>
    private void SubscribeSatisfactionChanged()
    {
        if (Managers.Data != null)
        {
            Managers.Data.OnSatisfactionChanged += UpdateSatisfactionInfo;
        }
    }

    /// <summary> KDH : 만족도 변화시 UI 업데이트를 구독해제 </summary>
    private void UnsubscribeSatisfactionChanged()
    {
        if (Managers.Data != null)
        {
            Managers.Data.OnSatisfactionChanged -= UpdateSatisfactionInfo;
        }
    }

    /// <summary> KDH : 퀘스트 진행도 변화시 UI 업데이트를 구독 </summary>
    private void SubscribeSubQuestEvents()
    {
        if (Managers.Quest != null)
        {
            Managers.Quest.OnQuestProgressed += OnSubQuestProgressed;
        }
    }

    /// <summary> KDH : 퀘스트 진행도 변화시 UI 업데이트를 구독해제 </summary>
    private void UnsubscribeSubQuestEvents()
    {
        if (Managers.Quest != null)
        {
            Managers.Quest.OnQuestProgressed -= OnSubQuestProgressed;
        }
    }

    /// <summary>SJY : 조리 커맨드 이벤트 구독</summary>
    private void SubscribeCookingEvents()
    {
        if (_cookingController == null)
        {
            CPrint.Error("[TycoonHUD] CookingController 참조가 없습니다.");

            return;
        }

        _cookingController.OnCommandPressed += UpdateCommandPressed;
        _cookingController.OnCommandFailed += UpdateCommandFailed;
        _cookingController.OnCommandReset += ResetCommandImages;
        _cookingController.OnCommandReset += ResetCookingImage;
        _cookingController.OnCookingCompleted += HandleCookingCompleted;
        _cookingController.OnServingFinished += HandleServingFinished;
        _cookingController.OnServingFinished += UpdateBusinessProgress;
        _cookingController.OnFoodChanged += UpdateFoodImages;

        UpdateFoodImages();
    }

    /// <summary>SJY : 조리 커맨드 이벤트 구독 해제</summary>
    private void UnsubscribeCookingEvents()
    {
        if (_cookingController == null)
        {
            return;
        }

        _cookingController.OnCommandPressed -= UpdateCommandPressed;
        _cookingController.OnCommandFailed -= UpdateCommandFailed;
        _cookingController.OnCommandReset -= ResetCommandImages;
        _cookingController.OnCommandReset -= ResetCookingImage;
        _cookingController.OnCookingCompleted -= HandleCookingCompleted;
        _cookingController.OnServingFinished -= HandleServingFinished;
        _cookingController.OnServingFinished -= UpdateBusinessProgress;
        _cookingController.OnFoodChanged -= UpdateFoodImages;
    }

    /// <summary> KDH : 마우스가 평판 아이콘에 올라오면 평판 정보를 표시한다. </summary>
    private void OnPointerEnterReputation()
    {
        _reputationTooltip?.Show(Get<TMP_Text>(Texts.ReputationValueText).text,
            Get<Image>(Images.ReputationIcon).rectTransform);
    }

    /// <summary> KDH : 마우스가 평판 아이콘에서 벗어나면 평판 정보를 숨긴다. </summary>
    private void OnPointerExitReputation()
    {
        _reputationTooltip?.Hide();
    }

    /// <summary> KDH : 퀘스트가 진행됐을 때 서브 퀘스트의 완료 여부에 따라 표시를 달리한다. </summary>
    private void OnSubQuestProgressed(QuestProgressEvent progressEvent)
    {
        if (progressEvent.Scene != SceneType.TycoonScene)
        {
            return;
        }

        if (progressEvent.IsCompleted)
        {
            StartSubQuestCompletion(progressEvent);
            return;
        }

        UpdateSubQuest(progressEvent.CurrentQuest, progressEvent.Progress);
    }

    #endregion ===== 이벤트 =====
}