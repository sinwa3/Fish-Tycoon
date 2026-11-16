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

    // 드롭다운 인덱스에 대응하는 화면 모드 목록
    private static readonly FullScreenMode[] ScreenModes =
    {
        FullScreenMode.FullScreenWindow,
        FullScreenMode.Windowed,
    };

    // 드롭다운에 표시할 해상도 목록
    private readonly List<Vector2Int> _resolutions = new();

    // 마지막으로 확인한 해상도와 화면 모드 (변경 감지용)
    private Vector2Int _displaySize;
    private FullScreenMode _displayMode;

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

        // 탭 전환 및 팝업 버튼 이벤트 등록
        Get<Button>(Buttons.VideoBtn).onClick.AddListener(() => SelectTab(TabType.Video));
        Get<Button>(Buttons.AudioBtn).onClick.AddListener(() => SelectTab(TabType.Audio));
        Get<Button>(Buttons.ResetBtn).onClick.AddListener(ResetCurrentTab);
        Get<Button>(Buttons.CloseBtn).onClick.AddListener(() => Close());

        // 해상도 및 화면 모드 드롭다운 초기화
        InitializeDisplay();

        // 슬라이더 변경 시 해당 설정값을 적용하도록 이벤트 등록
        SetSliderListener(Sliders.MouseSlider, Texts.MouseSensitivityValue, Managers.Input.SaveMouseSpeed);
        SetSliderListener(Sliders.MasterSlider, Texts.MasterValue, Managers.Sound.SetMasterVolume);
        SetSliderListener(Sliders.BgmSlider, Texts.BgmValue, Managers.Sound.SetBgmVolume);
        SetSliderListener(Sliders.SfxSlider, Texts.SfxValue, Managers.Sound.SetSfxVolume);

        // 현재 설정값을 슬라이더와 텍스트에 반영
        RefreshSliders();

        // 기본 탭을 비디오 설정으로 지정
        SelectTab(TabType.Video);
    }

    protected override void OnUpdate()
    {
        // 실제 해상도와 화면 모드 변경 여부 확인
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
        // 변경된 PlayerPrefs 데이터를 저장
        PlayerPrefs.Save();
    }

    #region ===== 디스플레이 =====

    /// <summary> KDH : 해상도와 화면 모드 드롭다운을 초기화하고 변경 이벤트를 등록한다. </summary>
    private void InitializeDisplay()
    {
        TMP_Dropdown resolutionDropdown = Get<TMP_Dropdown>(Dropdowns.ResolutionDropdown);

        // 해상도 변경 시 선택한 설정을 실제 화면에 적용
        resolutionDropdown.onValueChanged.AddListener(_ => ApplyDisplay());

        TMP_Dropdown modeDropdown = Get<TMP_Dropdown>(Dropdowns.ScreenModeDropdown);

        // 화면 모드 목록 구성 (ScreenModes 배열 순서와 일치)
        modeDropdown.ClearOptions();
        modeDropdown.AddOptions(new List<string> { "전체 화면", "창 모드" });

        // 화면 모드 변경 시 선택한 설정을 실제 화면에 적용
        modeDropdown.onValueChanged.AddListener(_ => ApplyDisplay());

        // 현재 화면 상태를 드롭다운에 반영
        UpdateDisplaySelection();
    }

    /// <summary> KDH : 실제 해상도나 화면 모드가 변경되면 드롭다운 목록과 선택값을 갱신한다. </summary>
    private void UpdateDisplaySelection()
    {
        // 현재 게임 화면의 해상도와 화면 모드 가져오기
        Vector2Int size = new(Screen.width, Screen.height);
        FullScreenMode mode = Screen.fullScreenMode;

        // 유효하지 않은 해상도이거나 이전 화면 상태와 같으면 갱신하지 않음
        if (size.x <= 0 || size.y <= 0 || (size == _displaySize && mode == _displayMode))
        {
            return;
        }

        TMP_Dropdown resolutionDropdown = Get<TMP_Dropdown>(Dropdowns.ResolutionDropdown);
        TMP_Dropdown modeDropdown = Get<TMP_Dropdown>(Dropdowns.ScreenModeDropdown);

        // 화면 상태 변경 시 펼쳐진 드롭다운 목록 닫기
        if (resolutionDropdown.IsExpanded)
        {
            resolutionDropdown.Hide();
        }

        if (modeDropdown.IsExpanded)
        {
            modeDropdown.Hide();
        }

        RebuildResolutionOptions(size, mode);
        // 변경 이벤트 없이 실제 화면 상태를 드롭다운에 반영
        resolutionDropdown.SetValueWithoutNotify(_resolutions.IndexOf(size));
        modeDropdown.SetValueWithoutNotify(mode == FullScreenMode.Windowed ? 1 : 0);

        // 마지막으로 확인한 화면 상태 저장
        _displaySize = size;
        _displayMode = mode;
    }

    /// <summary> KDH : 중복되지 않는 유효한 해상도를 목록에 추가한다. </summary>
    private void AddResolution(int width, int height)
    {
        Vector2Int size = new(width, height);

        // 가로와 세로가 유효하고 목록에 없는 해상도만 추가
        if (width > 0 && height > 0 && !_resolutions.Contains(size))
        {
            _resolutions.Add(size);
        }
    }

    /// <summary> KDH : 현재 화면 모드에서 허용되는 해상도를 수집하고 드롭다운 목록을 재구성한다. </summary>
    private void RebuildResolutionOptions(Vector2Int current, FullScreenMode mode)
    {
        _resolutions.Clear();

        // 시스템에서 지원하는 해상도 중 현재 화면 모드에서 허용되는 해상도 추가
        foreach (Resolution resolution in Screen.resolutions)
        {
            Vector2Int size = new(resolution.width, resolution.height);

            if (IsResolutionAllowed(size, mode))
            {
                AddResolution(size.x, size.y);
            }
        }

        // 실제 디스플레이의 해상도 추가
        Vector2Int desktop = GetNativeResolution();
        AddResolution(desktop.x, desktop.y);

        // 외부에서 바꾼 해상도도 현재 상태를 표시할 수 있도록 포함한다.
        AddResolution(current.x, current.y);

        // 가로 해상도 내림차순 정렬, 가로가 같으면 세로 해상도 내림차순 정렬
        _resolutions.Sort((left, right) => left.x != right.x
            ? right.x.CompareTo(left.x)
            : right.y.CompareTo(left.y));

        // 수집한 해상도 목록을 드롭다운에 표시
        TMP_Dropdown dropdown = Get<TMP_Dropdown>(Dropdowns.ResolutionDropdown);
        dropdown.ClearOptions();
        dropdown.AddOptions(_resolutions.ConvertAll(size => IsResolutionAllowed(size, mode)
            ? $"{size.x} × {size.y}"
            : $"{size.x} × {size.y} (현재)"));
    }

    /// <summary> KDH : 현재 화면 모드에서 지정한 해상도의 사용 가능 여부를 확인한다. </summary>
    private static bool IsResolutionAllowed(Vector2Int size, FullScreenMode mode)
    {
        // 창 모드에서는 화면 비율 제한 없이 허용
        if (mode != FullScreenMode.FullScreenWindow)
        {
            return true;
        }

        // 실제 디스플레이의 해상도를 기준으로 화면 비율 확인
        Vector2Int desktop = GetNativeResolution();

        // 디스플레이 해상도가 유효하지 않으면 제한하지 않음
        if (desktop.x <= 0 || desktop.y <= 0)
        {
            return true;
        }

        // 디스플레이의 화면 비율을 유지할 때 예상되는 가로 해상도 계산
        double expectedWidth = (double)size.y * desktop.x / desktop.y;

        // 1366×768처럼 픽셀 반올림으로 발생하는 최대 1픽셀 차이는 허용
        return Math.Abs(size.x - expectedWidth) <= 1.0;
    }

    /// <summary> KDH : 실제 디스플레이 해상도를 가져오고, 확인할 수 없으면 현재 화면 해상도를 반환한다. </summary>
    private static Vector2Int GetNativeResolution()
    {
        // 메인 디스플레이 정보 가져오기
        Display display = Display.main;

        // 디스플레이의 시스템 해상도가 유효하면 해당 값 반환
        if (display is { systemWidth: > 0, systemHeight: > 0 })
        {
            return new Vector2Int(display.systemWidth, display.systemHeight);
        }

        // 디스플레이 정보를 가져올 수 없으면 현재 화면 해상도로 대체
        Resolution fallback = Screen.currentResolution;
        return new Vector2Int(fallback.width, fallback.height);
    }

    /// <summary> KDH : 현재 화면 모드에서 허용되는 해상도 중 지정한 크기와 가장 가까운 항목을 찾는다. </summary>
    private int FindClosestResolution(Vector2Int size, FullScreenMode mode)
    {
        int closest = -1;
        float bestDistance = float.PositiveInfinity;

        for (int i = 0; i < _resolutions.Count; i++)
        {
            Vector2Int candidate = _resolutions[i];

            // 현재 화면 모드에서 허용되지 않는 해상도는 제외
            if (!IsResolutionAllowed(candidate, mode))
            {
                continue;
            }

            // 현재 해상도와 후보 해상도의 거리 제곱 계산
            float distance = (candidate - size).sqrMagnitude;

            // 기존 후보보다 가까운 해상도를 발견하면 갱신
            if (distance < bestDistance)
            {
                closest = i;
                bestDistance = distance;
            }
        }

        return closest;
    }

    /// <summary> KDH : 선택한 해상도와 화면 모드를 실제 디스플레이에 적용한다. </summary>
    private void ApplyDisplay()
    {
        // 드롭다운에서 선택한 해상도와 화면 모드 가져오기
        int index = Get<TMP_Dropdown>(Dropdowns.ResolutionDropdown).value;
        if (index < 0 || index >= _resolutions.Count)
        {
            return;
        }

        Vector2Int size = _resolutions[index];
        FullScreenMode mode = ScreenModes[Get<TMP_Dropdown>(Dropdowns.ScreenModeDropdown).value];

        // 선택한 해상도가 현재 화면 모드에서 허용되지 않으면 가장 가까운 해상도로 대체
        if (!IsResolutionAllowed(size, mode))
        {
            int closest = FindClosestResolution(size, mode);
            size = closest >= 0 ? _resolutions[closest] : GetNativeResolution();
        }

        if (size.x <= 0 || size.y <= 0)
        {
            return;
        }

        // 실제 게임 화면의 해상도와 화면 모드 변경
        Screen.SetResolution(size.x, size.y, mode);
    }

    #endregion ===== 디스플레이 =====

    #region ===== 슬라이더 =====

    /// <summary> KDH : 슬라이더 변경 시 설정값을 적용하고 표시 텍스트를 갱신하도록 이벤트를 등록한다. </summary>
    private void SetSliderListener(Sliders slider, Texts label, Action<float> apply)
    {
        Get<Slider>(slider).onValueChanged.AddListener(value =>
        {
            // 변경된 슬라이더 값을 실제 설정에 적용
            apply(value);

            // 변경된 값을 표시 텍스트에 반영
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
        // 값 변경 이벤트를 발생시키지 않고 슬라이더 값 설정
        Get<Slider>(slider).SetValueWithoutNotify(value);

        // 현재 값을 표시 텍스트에 반영
        SetValueLabel(label, value);
    }

    /// <summary> KDH : 설정값을 백분율로 변환하여 표시 텍스트를 갱신한다. </summary>
    private void SetValueLabel(Texts label, float value)
    {
        // 0~1 범위의 값을 0~100 정수로 변환
        int percent = Mathf.RoundToInt(value * 100f);

        // 마우스 감도는 숫자만, 볼륨은 백분율로 표시
        Get<TMP_Text>(label).SetText(label == Texts.MouseSensitivityValue ? percent.ToString() : $"{percent}%");
    }

    #endregion ===== 슬라이더 =====

    #region ===== 탭 =====

    /// <summary> KDH : 선택한 설정 탭을 활성화하고 탭 버튼의 색상을 변경한다. </summary>
    private void SelectTab(TabType tabType)
    {
        _currentTab = tabType;

        // 모든 패널 비활성화 및 탭 버튼 색상 초기화
        Get<GameObject>(Panels.VideoPanel).SetActive(false);
        Get<GameObject>(Panels.AudioPanel).SetActive(false);
        Get<Button>(Buttons.VideoBtn).image.color = Color.white;
        Get<Button>(Buttons.AudioBtn).image.color = Color.white;

        // 선택한 탭의 패널 활성화 및 버튼 색상 변경
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
                // 마우스 감도를 기본값으로 복원
                Managers.Input.SaveMouseSpeed(InputManager.DefaultMouseSpeed);

                // 실제 디스플레이 해상도 가져오기
                Vector2Int desktop = GetNativeResolution();

                // 유효한 해상도라면 해당 해상도로 전체 화면 모드 적용
                if (desktop is { x: > 0, y: > 0 })
                {
                    Screen.SetResolution(desktop.x, desktop.y, FullScreenMode.FullScreenWindow);
                }
                break;

            case TabType.Audio:
                // 마스터, BGM, SFX 볼륨을 기본값으로 복원
                Managers.Sound.SetMasterVolume(SoundManager.DefaultMasterVolume);
                Managers.Sound.SetBgmVolume(SoundManager.DefaultVolume);
                Managers.Sound.SetSfxVolume(SoundManager.DefaultVolume);
                break;
        }

        // 복원된 설정값을 슬라이더와 표시 텍스트에 반영
        RefreshSliders();

        // 변경된 설정 저장
        PlayerPrefs.Save();
    }

    #endregion ===== 탭 =====
}
