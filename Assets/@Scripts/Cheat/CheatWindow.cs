#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using UnityEngine;

/// <summary> F8로 여닫는 개발 빌드용 치트 창. 열려 있는 동안 게임을 멈추고 플레이어 입력을 막는다 </summary>
public class CheatWindow : MonoBehaviour
{
    #region ===== 수치 =====

    private const KeyCode ToggleKey = KeyCode.F8;
    private const int WindowID = 8008;
    private const string WindowTitle = "치트 (F8 닫기)";

    // 창 크기는 화면 높이 1080 기준이고 화면 높이에 맞춰 늘린다
    private const float ReferenceHeight = 1080.0f;
    private const float WindowMargin = 10.0f;
    private const float WindowWidth = 360.0f;
    private const float WindowHeight = 760.0f;
    private const int FontSize = 16;
    private const float ButtonHeight = 30.0f;

    #endregion ===== 수치 =====

    #region ===== 상태 =====

    private readonly List<BaseCheat> _cheats = new();

    private Rect _windowRect = new Rect(WindowMargin, WindowMargin, WindowWidth, WindowHeight);
    private Vector2 _scrollPosition;
    private GUIStyle _windowStyle;
    private GUIStyle _titleStyle;
    private bool _wasCursorLocked;

    public bool IsOpen { get; private set; }

    /// <summary> 창을 닫을 때 되돌릴 시간 배율 </summary>
    public float ResumeTimeScale { get; private set; } = 1.0f;

    public GUIStyle ButtonStyle { get; private set; }
    public GUIStyle LabelStyle { get; private set; }

    #endregion ===== 상태 =====

    /// <summary> 게임을 시작할 때 치트 창 오브젝트를 만든다 </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        GameObject cheatObject = new GameObject(nameof(CheatWindow));
        DontDestroyOnLoad(cheatObject);
        cheatObject.AddComponent<CheatWindow>();
    }

    private void Awake()
    {
        InitializeCheats();
    }

    private void Update()
    {
        UpdateToggleInput();
        UpdateCheats();
    }

    private void LateUpdate()
    {
        LateUpdateCursor();
        LateUpdateCheats();
    }

    private void OnGUI()
    {
        DrawOverlays();
        DrawWindow();
    }

    #region ===== 초기화 =====

    /// <summary> 창에 보일 순서대로 치트를 넣는다 </summary>
    private void InitializeCheats()
    {
        AddCheat(new SettlementCheat());
        AddCheat(new DetectionOffCheat());
        AddCheat(new MinimapCheat());
        AddCheat(new GoldCheat());
        AddCheat(new SatisfactionCheat());
        AddCheat(new QuickSlotItemCheat());
        AddCheat(new QuickSlotFishCheat());
        AddCheat(new InventoryFishCheat());
        AddCheat(new DayCheat());
        AddCheat(new SpeedCheat());
        AddCheat(new SceneCheat());
        AddCheat(new SaveResetCheat());
    }

    private void AddCheat(BaseCheat cheat)
    {
        cheat.Initialize(this);
        _cheats.Add(cheat);
    }

    /// <summary> 창 글씨 크기를 정한다 (GUI.skin은 OnGUI 안에서만 읽힌다) </summary>
    private void InitializeStyles()
    {
        if (ButtonStyle != null)
        {
            return;
        }

        _windowStyle = new GUIStyle(GUI.skin.window) { fontSize = FontSize };
        _titleStyle = new GUIStyle(GUI.skin.label) { fontSize = FontSize, fontStyle = FontStyle.Bold };
        ButtonStyle = new GUIStyle(GUI.skin.button) { fontSize = FontSize, fixedHeight = ButtonHeight };
        LabelStyle = new GUIStyle(GUI.skin.label) { fontSize = FontSize - 2, wordWrap = true };
    }

    #endregion ===== 초기화 =====

    #region ===== 열고 닫기 =====

    /// <summary> 키를 누르면 창을 열거나 닫는다 </summary>
    private void UpdateToggleInput()
    {
        if (!Input.GetKeyDown(ToggleKey))
        {
            return;
        }

        if (IsOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    /// <summary> 게임을 멈추고 입력을 막은 뒤 커서를 푼다 </summary>
    public void Open()
    {
        if (IsOpen)
        {
            return;
        }

        IsOpen = true;

        ResumeTimeScale = Time.timeScale;
        Time.timeScale = 0.0f;

        _wasCursorLocked = Cursor.lockState == CursorLockMode.Locked;
        Managers.Input?.SetInputBlocked(this, true);
        Managers.Input?.SetCursorLock(false);
    }

    /// <summary> 멈추기 전 시간 배율과 커서 잠금을 되돌린다 </summary>
    public void Close()
    {
        if (!IsOpen)
        {
            return;
        }

        IsOpen = false;

        Time.timeScale = ResumeTimeScale;

        Managers.Input?.SetInputBlocked(this, false);
        Managers.Input?.SetCursorLock(_wasCursorLocked);
    }

    /// <summary> 시간 배율을 바꾼다. 창이 열려 있으면 닫을 때 적용한다 </summary>
    public void SetResumeTimeScale(float timeScale)
    {
        ResumeTimeScale = timeScale;

        if (!IsOpen)
        {
            Time.timeScale = timeScale;
        }
    }

    /// <summary> 창이 열린 동안 HUD가 클릭으로 커서를 다시 잠가도 풀어 둔다 </summary>
    private void LateUpdateCursor()
    {
        if (!IsOpen || Cursor.lockState == CursorLockMode.None)
        {
            return;
        }

        Managers.Input?.SetCursorLock(false);
    }

    #endregion ===== 열고 닫기 =====

    #region ===== 치트 갱신 =====

    private void UpdateCheats()
    {
        foreach (BaseCheat cheat in _cheats)
        {
            cheat.RunPendingAction();
            cheat.OnUpdate();
        }
    }

    private void LateUpdateCheats()
    {
        foreach (BaseCheat cheat in _cheats)
        {
            cheat.OnLateUpdate();
        }
    }

    #endregion ===== 치트 갱신 =====

    #region ===== 그리기 =====

    /// <summary> 창과 상관없이 화면에 그릴 것을 그린다 (미니맵 점 등) </summary>
    private void DrawOverlays()
    {
        foreach (BaseCheat cheat in _cheats)
        {
            if (cheat.IsAvailable)
            {
                cheat.DrawOverlay();
            }
        }
    }

    /// <summary> 화면 높이에 맞춰 키운 창을 그린다 </summary>
    private void DrawWindow()
    {
        if (!IsOpen)
        {
            return;
        }

        InitializeStyles();

        float scale = Screen.height / ReferenceHeight;
        GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1.0f));

        _windowRect = GUI.Window(WindowID, _windowRect, DrawWindowContents, WindowTitle, _windowStyle);

        GUI.matrix = Matrix4x4.identity;
    }

    /// <summary> 지금 씬에서 쓸 수 있는 치트만 칸을 나눠 그린다 </summary>
    private void DrawWindowContents(int windowID)
    {
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

        foreach (BaseCheat cheat in _cheats)
        {
            if (!cheat.IsAvailable)
            {
                continue;
            }

            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label(cheat.Title, _titleStyle);
            cheat.DrawButtons();
            GUILayout.EndVertical();
        }

        GUILayout.EndScrollView();

        GUI.DragWindow();
    }

    #endregion ===== 그리기 =====
}
#endif
