#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;

/// <summary> 집주인 의심 게이지를 화면 위쪽에 막대와 숫자로 띄운다. 미니맵 없이 게이지만 볼 때 쓴다 </summary>
public class EnemyGaugeCheat : BaseCheat
{
    #region ===== 수치 =====

    private const float MaxGauge = 100.0f;

    private const float ReferenceHeight = 1080.0f;
    private const float TopMargin = 10.0f;
    private const float PanelWidth = 420.0f;
    private const float PanelHeight = 64.0f;
    private const float Padding = 8.0f;
    private const float BarHeight = 18.0f;
    private const float TickWidth = 2.0f;
    private const int TextFontSize = 16;

    private static readonly Color PanelColor = new Color(0.0f, 0.0f, 0.0f, 0.6f);
    private static readonly Color BarBackColor = new Color(0.25f, 0.25f, 0.25f, 1.0f);
    private static readonly Color TickColor = Color.white;

    #endregion ===== 수치 =====

    #region ===== 상태 =====

    private EnemyController _enemy;
    private GUIStyle _textStyle;

    private bool _isOn;

    private bool IsShowing => _isOn && IsAvailable;

    #endregion ===== 상태 =====

    public override string Title => "집주인 게이지";

    public override bool IsAvailable => IsCurrentScene<StealthScene>();

    public override void DrawButtons()
    {
        DrawButton(_isOn ? "게이지 끄기" : "게이지 켜기", () => _isOn = !_isOn);
        DrawLabel("흰 줄은 의심, 조사 시작 게이지. 끝까지 차면 발각");
    }

    public override void OnUpdate()
    {
        UpdateTargetEnemy();
    }

    public override void DrawOverlay()
    {
        if (!IsShowing)
        {
            return;
        }

        DrawGauge();
    }

    #region ===== 대상 =====

    /// <summary> 집주인을 찾는다 </summary>
    private void UpdateTargetEnemy()
    {
        if (!IsShowing || _enemy != null)
        {
            return;
        }

        _enemy = Object.FindObjectOfType<EnemyController>();
    }

    #endregion ===== 대상 =====

    #region ===== 그리기 =====

    /// <summary> 화면 왼쪽 반 가운데 위에 게이지 막대와 상태 글을 그린다 (오른쪽 반은 미니맵 자리) </summary>
    private void DrawGauge()
    {
        _textStyle ??= new GUIStyle(GUI.skin.label) { fontSize = TextFontSize, normal = { textColor = Color.white } };

        float scale = Screen.height / ReferenceHeight;
        GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1.0f));

        float screenWidth = Screen.width / scale;
        Rect panelRect = new Rect(screenWidth * 0.25f - PanelWidth * 0.5f, TopMargin, PanelWidth, PanelHeight);
        DrawRect(panelRect, PanelColor);

        Rect textRect = new Rect(panelRect.x + Padding, panelRect.y + Padding, panelRect.width - Padding * 2.0f, BarHeight + 4.0f);
        Rect barRect = new Rect(panelRect.x + Padding, panelRect.yMax - Padding - BarHeight, panelRect.width - Padding * 2.0f, BarHeight);

        if (_enemy == null)
        {
            GUI.Label(textRect, "집주인 없음", _textStyle);
            GUI.matrix = Matrix4x4.identity;

            return;
        }

        float gauge = _enemy.SuspicionGauge.Gauge;
        AlertLevel alertLevel = _enemy.Memory.AlertLevel;

        GUI.Label(textRect, $"게이지 {gauge:0.0} / {_enemy.CurrentStateName} / {alertLevel}", _textStyle);

        DrawRect(barRect, BarBackColor);
        DrawRect(new Rect(barRect.x, barRect.y, barRect.width * Mathf.Clamp01(gauge / MaxGauge), barRect.height), GetBarColor(alertLevel));

        EnemyDetectionData detection = _enemy.Detection;

        if (detection != null)
        {
            DrawTick(barRect, detection.SuspiciousEnterGauge);
            DrawTick(barRect, detection.AlertEnterGauge);
        }

        GUI.matrix = Matrix4x4.identity;
    }

    /// <summary> 막대 위 그 게이지 자리에 세로 줄을 긋는다 </summary>
    private void DrawTick(Rect barRect, float gauge)
    {
        float x = barRect.x + barRect.width * Mathf.Clamp01(gauge / MaxGauge);
        DrawRect(new Rect(x - TickWidth * 0.5f, barRect.y, TickWidth, barRect.height), TickColor);
    }

    /// <summary> 경계 단계마다 막대 색을 돌려준다 </summary>
    private Color GetBarColor(AlertLevel alertLevel)
    {
        switch (alertLevel)
        {
            case AlertLevel.Suspicious:
                return Color.yellow;
            case AlertLevel.Alert:
                return new Color(1.0f, 0.5f, 0.0f);
            case AlertLevel.Detected:
                return Color.red;
            default:
                return Color.green;
        }
    }

    private void DrawRect(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    #endregion ===== 그리기 =====
}
#endif
