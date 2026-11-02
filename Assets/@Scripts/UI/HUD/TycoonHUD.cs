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
        PhaseText,

        MainQuestValueText,
        SubQuestDescText,
    }

    public enum Sliders
    {
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

    #region ===== 애니메이션 =====

    // 다이어리
    private RectTransform _diaryRectTransform;
    private Vector2 _diaryOriginPos;
    private Tween _diaryTween;

    // 퀘스트
    private Coroutine _subQuestCoroutine;

    #endregion ===== 애니메이션 =====

    #region ==== 조리 커맨드 ====

    private Sprite[] _normalSprites = new Sprite[4];
    private Sprite[] _pressedSprites = new Sprite[4];
    private Sprite[] _failedSprites = new Sprite[4];

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
                _cookingController.ProcessCommand('W');
            }

            else if (Managers.Input.KeyDown_A)
            {
                _cookingController.ProcessCommand('A');
            }

            else if (Managers.Input.KeyDown_S)
            {
                _cookingController.ProcessCommand('S');
            }

            else if (Managers.Input.KeyDown_D)
            {
                _cookingController.ProcessCommand('D');
            }
        }
    }

    private void OnDestroy()
    {
        _diaryTween?.Kill();

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
        Get<Button>(Buttons.StoreBtn)?.onClick.AddListener(() => Managers.UI.OpenPopup<StorePopup>());
    }

    /// <summary> KDH : 타이쿤 씬 첫 진입시 현재 영업 날짜를 세팅한다. </summary>
    private void SetCurrentDayText()
    {
        Get<TMP_Text>(Texts.CurrentDayText)?.SetText($"{Managers.Data.GetCurrentDay()}일 차");
    }

    /// <summary> KDH : 타이쿤 씬 첫 진입시 목표 금액을 설정한다. </summary>
    private void SetMainQuestValueText()
    {
        Get<TMP_Text>(Texts.MainQuestValueText)?.SetText($"{_tycoonScene.TargetGold:N0} G");
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
        }

        Image servingImage = Get<Image>(Images.ServingImage);

        if (servingImage != null)
        {
            servingImage.sprite = Managers.Resource.LoadSprite(TycoonSpritePath + "2DMiniGame-Sheet", "serving_0");
        }
    }

    #endregion ===== 초기화 =====

    #region ===== 업데이트 =====

    /// <summary> KDH : 현재 골드 값을 UI에 업데이트 한다. </summary>
    private void UpdateGoldText()
    {
        Get<TMP_Text>(Texts.GoldValueText)?.SetText($"{Managers.Data.GetCurrentGold():N0} G");
    }

    /// <summary> KDH : 현재 영업 상태를 UI에 업데이트 한다. </summary>
    public void UpdatePhaseText(TycoonScene.Phase phase)
    {
        string message = phase switch
        {
            TycoonScene.Phase.BeforeBusiness => "영업 전",
            TycoonScene.Phase.DuringBusiness => "영업 중",
            TycoonScene.Phase.AfterBusiness => "영업 후",
            _ => string.Empty
        };

        Get<TMP_Text>(Texts.PhaseText)?.SetText(message);
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

        Get<Image>(Images.SubQuest)?.gameObject.SetActive(true);
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

    /// <summary> KDH : 다음 버튼[잠입전환] 표시/비표시한다. </summary>
    public void SetStoreBtnVisible(bool visible)
    {
        Get<Button>(Buttons.StoreBtn)?.gameObject.SetActive(visible);
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
    }

    /// <summary>SJY : 정답 커맨드 이미지 변경</summary>
    private void UpdateCommandPressed(int commandIndex)
    {
        UpdateCommandSprite(commandIndex, _pressedSprites);

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

    #endregion ==== 조리 커맨드 UI ====

    #region ===== 퀘스트 =====
    
    private void StartSubQuestCompletion(QuestProgressEvent progressEvent)
    {
        if (_subQuestCoroutine != null)
        {
            StopCoroutine(_subQuestCoroutine);
        }

        _subQuestCoroutine = StartCoroutine(Co_ShowSubQuestCompletion(progressEvent));
    }

    private IEnumerator Co_ShowSubQuestCompletion(QuestProgressEvent progressEvent)
    {
        // 완료 표시
        UpdateSubQuest(progressEvent.CompletedQuest, progressEvent.CompletedQuest.Goal, true);
        Managers.Quest.SetCompletionSequenceActive(true);
        yield return new WaitForSeconds(0.8f);

        // 보상 표시
        Get<TMP_Text>(Texts.SubQuestDescText)?.SetText($"{progressEvent.CompletedQuest.GoldReward:N0} G 획득!");
        yield return new WaitForSeconds(1.2f);

        // 다음 퀘스트 표시
        UpdateSubQuest(progressEvent.NextQuest, 0);
        Managers.Quest.SetCompletionSequenceActive(false);
        _subQuestCoroutine = null;
    }
    
    #endregion ===== 퀘스트 =====
    
    #region ===== 이벤트 =====

    /// <summary> KDH : 골드 변화시 UI 업데이트를 구독 </summary>
    private void SubscribeGoldChanged()
    {
        if (Managers.Data != null)
        {
            Managers.Data.OnGoldChanged += UpdateGoldText;
        }
    }

    /// <summary> KDH : 골드 변화시 UI 업데이트를 구독해제 </summary>
    private void UnsubscribeGoldChanged()
    {
        if (Managers.Data != null)
        {
            Managers.Data.OnGoldChanged -= UpdateGoldText;
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