using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> KDH : 비디오와 오디오 설정을 표시하고 변경을 즉시 적용한다. </summary>
public class OptionPopup : BasePopup
{
    private enum Buttons
    {
        VideoBtn,
        AudioBtn,
        ResetBtn,

        CloseBtn
    }

    private enum Panels
    {
        VideoPanel,
        AudioPanel
    }

    private enum Dropdowns
    {
        ResolutionDropdown,
        ScreenModeDropdown
    }

    private enum Sliders
    {
        MouseSlider,

        MasterSlider,
        BgmSlider,
        SfxSlider
    }

    private enum Texts
    {
        MouseSensitivityValue,

        MasterValue,
        BgmValue,
        SfxValue
    }

    private enum TabType
    {
        Video,
        Audio
    }

    #region ===== 상태 =====

    private TabType _currentTab = TabType.Video;

    private static readonly FullScreenMode[] ScreenModes =
    {
        FullScreenMode.FullScreenWindow,
        FullScreenMode.Windowed,
    };

    private readonly List<Vector2Int> _resolutions = new();

    private Vector2Int _displaySize;
    private FullScreenMode _displayMode;

    private int _customResolutionIndex = -1;
    
    #endregion ===== 상태 =====

    protected override void OnAwake()
    {
        Bind<Button>(typeof(Buttons));
        Bind<GameObject>(typeof(Panels));
        Bind<TMP_Dropdown>(typeof(Dropdowns));
        Bind<Slider>(typeof(Sliders));
        Bind<TMP_Text>(typeof(Texts));
    }

    protected override void OnStart()
    {
        Managers.Sound.PlaySfx(SoundKey.Name.SfxType.ChimePopup);

        Get<Button>(Buttons.VideoBtn).onClick.AddListener(() => SelectTab(TabType.Video));
        Get<Button>(Buttons.AudioBtn).onClick.AddListener(() => SelectTab(TabType.Audio));
        Get<Button>(Buttons.ResetBtn).onClick.AddListener(ResetCurrentTab);
        Get<Button>(Buttons.CloseBtn).onClick.AddListener(() => Close());

        InitializeDisplay();

        SetSliderListener(Sliders.MouseSlider, Texts.MouseSensitivityValue, Managers.Input.SaveMouseSpeed);
        SetSliderListener(Sliders.MasterSlider, Texts.MasterValue, Managers.Sound.SetMasterVolume);
        SetSliderListener(Sliders.BgmSlider, Texts.BgmValue, Managers.Sound.SetBgmVolume);
        SetSliderListener(Sliders.SfxSlider, Texts.SfxValue, Managers.Sound.SetSfxVolume);

        RefreshSliders();

        SelectTab(TabType.Video);
    }

    protected override void OnUpdate()
    {
        UpdateDisplaySelection();
    }

    public override void OnInputKey()
    {
        if (Managers.Input.KeyDown_Esc)
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
        PlayerPrefs.Save();
    }

    /// <summary> KDH : 지원하는 해상도와 화면 모드를 드롭다운에 설정하고 변경 이벤트를 등록한다. </summary>
    private void InitializeDisplay()
    {
        _resolutions.Clear();
        _customResolutionIndex = -1;
        
        foreach (Resolution resolution in Screen.resolutions)
        {
            AddResolution(resolution.width, resolution.height);
        }

        // 현재 게임 해상도 추가
        AddResolution(Screen.width, Screen.height);
        // 현재 모니터 해상도 추가
        AddResolution(Screen.currentResolution.width, Screen.currentResolution.height);

        // 해상도 목록을 가로 해상도 기준으로 내림차순 정렬하고, 가로 해상도가 같으면 세로 해상도 기준으로 내림차순 정렬
        _resolutions.Sort((left, right) => left.x != right.x
            ? right.x.CompareTo(left.x)
            : right.y.CompareTo(left.y));

        // 해상도 드롭다운
        TMP_Dropdown resolutionDropdown = Get<TMP_Dropdown>(Dropdowns.ResolutionDropdown);
        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(_resolutions.ConvertAll(size => $"{size.x} × {size.y}"));
        resolutionDropdown.onValueChanged.AddListener(_ => ApplyDisplay());

        // 화면모드 (전체, 창)
        TMP_Dropdown modeDropdown = Get<TMP_Dropdown>(Dropdowns.ScreenModeDropdown);
        modeDropdown.ClearOptions();
        modeDropdown.AddOptions(new List<string> { "전체 화면", "창 모드" });
        modeDropdown.onValueChanged.AddListener(_ => ApplyDisplay());

        UpdateDisplaySelection();
    }

    /// <summary> KDH : 팝업이 활성화 중에는 실제 화면 상태 UI를 업데이트 한다. </summary>
    private void UpdateDisplaySelection()
    {
        Vector2Int size = new(Screen.width, Screen.height);
        FullScreenMode mode = Screen.fullScreenMode;

        if (size.x <= 0 || size.y <= 0 || (size == _displaySize && mode == _displayMode))
        {
            return;
        }

        TMP_Dropdown resolutionDropdown = Get<TMP_Dropdown>(Dropdowns.ResolutionDropdown);
        TMP_Dropdown modeDropdown = Get<TMP_Dropdown>(Dropdowns.ScreenModeDropdown);
        int index = _resolutions.IndexOf(size);
        if (index < 0)
        {
            // 임시 커스텀 해상도 추가
            if (_customResolutionIndex < 0)
            {
                _customResolutionIndex = _resolutions.Count;
                _resolutions.Add(size);
                resolutionDropdown.AddOptions(new List<string> { $"{size.x} × {size.y}" });
            }
            else
            {
                _resolutions[_customResolutionIndex] = size;
                resolutionDropdown.options[_customResolutionIndex].text = $"{size.x} × {size.y}";
                resolutionDropdown.RefreshShownValue();
            }

            index = _customResolutionIndex;
        }

        // 화면 모드나 해상도가 바뀌었을 때 펼쳐진 드롭다운 목록을 접는다.
        if (resolutionDropdown.IsExpanded)
        {
            resolutionDropdown.Hide();
        }

        if (modeDropdown.IsExpanded)
        {
            modeDropdown.Hide();
        }

        resolutionDropdown.SetValueWithoutNotify(index);
        modeDropdown.SetValueWithoutNotify(mode == FullScreenMode.Windowed ? 1 : 0);

        _displaySize = size;
        _displayMode = mode;
    }

    /// <summary> KDH : 중복되지 않는 유효한 해상도를 목록에 추가한다. </summary>
    private void AddResolution(int width, int height)
    {
        Vector2Int size = new(width, height);
        if (width > 0 && height > 0 && !_resolutions.Contains(size))
        {
            _resolutions.Add(size);
        }
    }

    /// <summary> KDH : 선택한 해상도와 화면 모드를 실제 디스플레이에 적용한다. </summary>
    private void ApplyDisplay()
    {
        Vector2Int size = _resolutions[Get<TMP_Dropdown>(Dropdowns.ResolutionDropdown).value];
        FullScreenMode mode = ScreenModes[Get<TMP_Dropdown>(Dropdowns.ScreenModeDropdown).value];

        Screen.SetResolution(size.x, size.y, mode);
    }

    /// <summary> KDH : 슬라이더 변경 시 설정값을 적용하고 표시 텍스트를 갱신하도록 이벤트를 등록한다. </summary>
    private void SetSliderListener(Sliders slider, Texts label, Action<float> apply)
    {
        Get<Slider>(slider).onValueChanged.AddListener(value =>
        {
            apply(value);
            SetValueLabel(label, value);
        });
    }

    /// <summary> KDH : 현재 저장된 설정값을 각 슬라이더와 표시 텍스트에 반영한다. </summary>
    private void RefreshSliders()
    {
        SetSlider(Sliders.MouseSlider, Texts.MouseSensitivityValue, Managers.Input.MouseSpeed);
        SetSlider(Sliders.MasterSlider, Texts.MasterValue, Managers.Sound.GetMasterVolume());
        SetSlider(Sliders.BgmSlider, Texts.BgmValue, Managers.Sound.GetBgmVolume());
        SetSlider(Sliders.SfxSlider, Texts.SfxValue, Managers.Sound.GetSfxVolume());
    }

    /// <summary> KDH : 변경 이벤트를 발생시키지 않고 슬라이더 값과 표시 텍스트를 설정한다. </summary>
    private void SetSlider(Sliders slider, Texts label, float value)
    {
        Get<Slider>(slider).SetValueWithoutNotify(value);
        SetValueLabel(label, value);
    }

    /// <summary> KDH : 설정값을 백분율로 변환하여 표시 텍스트를 갱신한다. </summary>
    private void SetValueLabel(Texts label, float value)
    {
        int percent = Mathf.RoundToInt(value * 100f);
        Get<TMP_Text>(label).SetText(label == Texts.MouseSensitivityValue ? percent.ToString() : $"{percent}%");
    }

    /// <summary> KDH : 선택한 설정 탭을 활성화하고 탭 버튼의 색상을 변경한다. </summary>
    private void SelectTab(TabType tabType)
    {
        _currentTab = tabType;

        Get<GameObject>(Panels.VideoPanel).SetActive(false);
        Get<GameObject>(Panels.AudioPanel).SetActive(false);
        Get<Button>(Buttons.VideoBtn).image.color = Color.white;
        Get<Button>(Buttons.AudioBtn).image.color = Color.white;

        switch (_currentTab)
        {
            case TabType.Video:
                Get<GameObject>(Panels.VideoPanel).SetActive(true);
                Get<Button>(Buttons.VideoBtn).image.color = ColorKey.PastelYellow;
                break;
            case TabType.Audio:
                Get<GameObject>(Panels.AudioPanel).SetActive(true);
                Get<Button>(Buttons.AudioBtn).image.color = ColorKey.PastelYellow;
                break;
        }
    }

    /// <summary> KDH : 현재 선택한 탭의 설정을 기본값으로 복원하고 저장한다. </summary>
    private void ResetCurrentTab()
    {
        switch (_currentTab)
        {
            case TabType.Video:
                Managers.Input.SaveMouseSpeed(InputManager.DefaultMouseSpeed);
                int index = _resolutions.IndexOf(new Vector2Int(Screen.currentResolution.width,
                    Screen.currentResolution.height));
                Get<TMP_Dropdown>(Dropdowns.ResolutionDropdown).SetValueWithoutNotify(Mathf.Max(0, index));
                Get<TMP_Dropdown>(Dropdowns.ScreenModeDropdown).SetValueWithoutNotify(0);
                ApplyDisplay();
                break;
            case TabType.Audio:
                Managers.Sound.SetMasterVolume(SoundManager.DefaultMasterVolume);
                Managers.Sound.SetBgmVolume(SoundManager.DefaultVolume);
                Managers.Sound.SetSfxVolume(SoundManager.DefaultVolume);
                break;
        }

        RefreshSliders();
        PlayerPrefs.Save();
    }
}