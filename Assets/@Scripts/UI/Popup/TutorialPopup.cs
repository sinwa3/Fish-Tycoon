using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> KDH : 튜토리얼을 위한 팝업이다. </summary>
public class TutorialPopup : BasePopup
{
    private enum Buttons
    {
        NextBtn,
        PrevBtn,
        SkipBtn,
    }

    private enum Texts
    {
        TitleText,
        ContentsText,
        NextBtnText
    }

    private enum Images
    {
        Bg,
        Screenshot,
        UI_Dim,
    }

    #region ===== 참조 =====
    
    private Transform _hudRoot;
    
    #endregion ===== 참조 =====
    
    #region ===== 상태 =====
    
    private TutorialData[] _pages;
    private int _pageIndex;
    
    // 튜토리얼에서 생성한 하이라이트 컴포넌트 관리
    private readonly Dictionary<Transform, UIHighlightCanvas> _highlights = new();
    
    #endregion ===== 상태 =====

    protected override void OnAwake()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));
        Bind<Image>(typeof(Images));
    }

    protected override void OnStart()
    {
        Get<Button>(Buttons.NextBtn).onClick.AddListener(NextPage);
        Get<Button>(Buttons.SkipBtn).onClick.AddListener(() => Close());
        Get<Button>(Buttons.PrevBtn).onClick.AddListener(PreviousPage);
    }

    protected override void OnOpened()
    {
        base.OnOpened();
        
        Managers.Sound?.PlaySfx(SoundKey.Name.SfxType.TutorialOpen);
    }

    protected override void OnUpdate()
    {
    }
    
    public override void OnInputKey()
    {
        if (Managers.Input.KeyDown_Enter)
        {
            NextPage();
        }
        else if (Managers.Input.KeyDown_Esc)
        {
            Close();
        }
    }

    protected override void Close(Action closeAction = null)
    {
        base.Close(closeAction);
        
        Managers.Sound.PlaySfx(SoundKey.Name.SfxType.ChimeConfirm);
    }

    protected override void DestroyOverride()
    {
        RestoreHighlights();

        Time.timeScale = 1f;
    }

    /// <summary> KDH : 튜토리얼 페이지를 설정하고 게임 일시정지 및 UI 정렬을 초기화한다. </summary>
    public void SetPages(TutorialData[] pages, bool pauseGameplay)
    {
        _pages = pages;
        _hudRoot = Managers.UI.CurrentHUD?.transform;
        
        // HUD 강조 요소가 딤 배경보다 위에 표시되도록 정렬
        SetCanvasOrder(Get<Image>(Images.UI_Dim), SortingOrder - 1);
        SetCanvasOrder(Get<Image>(Images.Bg), SortingOrder + 1);
        
        // 튜토리얼 진행 중 게임 일시정지
        if (pauseGameplay)
        {
            Time.timeScale = 0f;
        }

        RefreshPage();
    }

    /// <summary> KDH : 다음 튜토리얼 페이지로 이동하거나 마지막 페이지에서 팝업을 닫는다. </summary>
    private void NextPage()
    {
        if (_pages == null)
        {
            return;
        }

        if (_pageIndex == _pages.Length - 1)
        {
            Close();
            return;
        }

        _pageIndex++;
        Managers.Sound?.PlaySfx(SoundKey.Name.SfxType.ButtonSound_1);
        RefreshPage();
    }

    /// <summary> KDH : 이전 튜토리얼 페이지로 이동한다. </summary>
    private void PreviousPage()
    {
        if (_pageIndex == 0)
        {
            return;
        }

        _pageIndex--;
        Managers.Sound?.PlaySfx(SoundKey.Name.SfxType.ButtonSound_2);
        RefreshPage();
    }

    /// <summary> KDH : 현재 페이지의 내용과 이미지, 버튼 상태 및 하이라이트를 갱신한다. </summary>
    private void RefreshPage()
    {
        TutorialData page = _pages[_pageIndex];
        Get<TMP_Text>(Texts.TitleText)?.SetText($"{page.Title} ({_pageIndex + 1}/{_pages.Length})");
        Get<TMP_Text>(Texts.ContentsText)?.SetText(page.Body);

        Image screenshot = Get<Image>(Images.Screenshot);
        if (screenshot != null)
        {
            screenshot.sprite = string.IsNullOrWhiteSpace(page.ImageName) || page.ImageName == "-" 
                ? null
                : Managers.Resource?.LoadSprite(ResourceKey.Path.TutorialSprite, page.ImageName);
            
            screenshot.gameObject.SetActive(screenshot.sprite != null);
        }
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(Get<Image>(Images.Bg).rectTransform);
        
        Get<Button>(Buttons.PrevBtn).interactable = _pageIndex > 0;
        Get<TMP_Text>(Texts.NextBtnText)?.SetText(_pageIndex == _pages.Length - 1 ? "확인" : "다음");
        
        HighlightPage(page.HighlightPaths);
    }

    /// <summary> KDH : 딤 배경과 팝업 본문 사이에 HUD 강조 요소를 표시할 수 있도록 Canvas 정렬을 설정한다. </summary>
    private static void SetCanvasOrder(Image image, int order)
    {
        Canvas canvas = image.gameObject.GetOrAddComponent<Canvas>();
        
        canvas.overrideSorting = true;
        canvas.sortingOrder = order;
        
        image.gameObject.GetOrAddComponent<GraphicRaycaster>();
    }

    /// <summary> KDH : 현재 페이지에 지정된 HUD 요소를 찾아 하이라이트를 적용한다. </summary>
    private void HighlightPage(string paths)
    {
        foreach (UIHighlightCanvas highlight in _highlights.Values)
        {
            if (highlight != null)
            {
                highlight.ResetSorting();
            }
        }

        if (_hudRoot == null || string.IsNullOrWhiteSpace(paths) || paths == "-")
        {
            return;
        }

        foreach (string path in paths.Split('|'))
        {
            Transform target = _hudRoot.Find(path.Trim());
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (!_highlights.TryGetValue(target, out UIHighlightCanvas highlight))
            {
                highlight = target.gameObject.AddComponent<UIHighlightCanvas>();
                _highlights.Add(target, highlight);
            }

            if (highlight != null)
            {
                highlight.Highlight(SortingOrder);
            }
        }
    }

    /// <summary> KDH : 튜토리얼에서 생성한 모든 하이라이트를 복원하고 관리 목록을 초기화한다. </summary>
    private void RestoreHighlights()
    {
        foreach (UIHighlightCanvas highlight in _highlights.Values)
        {
            if (highlight != null)
            {
                highlight.Restore();
            }
        }

        _highlights.Clear();
    }
}
