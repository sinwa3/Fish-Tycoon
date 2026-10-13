using UnityEditor;
using UnityEngine;
using PixelInventoryDiaryUI;

namespace PixelInventoryDiaryUI.Editor
{
    /// <summary>
    /// Custom inspector for <see cref="UITweener"/>.
    /// <para>
    /// • Groups fields into collapsible sections (Timing / Behaviour / Scale /
    ///   Move / Fade / Rotate / Presets / Events) so a fully-configured tween
    ///   stays compact. Each channel header carries its own enable toggle, and
    ///   fold states persist across selections/sessions via EditorPrefs.
    /// </para>
    /// <para>
    /// • Adds Edit-mode preview buttons (Play / Bounce / Pulse / Shake).
    ///   Previews drive the tween via EditorApplication.update
    ///   (coroutines don't run in Edit mode) and restore the original transform
    ///   when finished. In Play mode the buttons call the real runtime methods.
    /// </para>
    /// </summary>
    [CustomEditor(typeof(UITweener))]
    public class UITweenerEditor : UnityEditor.Editor
    {
        private enum PreviewMode { None, Tween, Bounce, Pulse, Shake }

        private PreviewMode mode = PreviewMode.None;
        private double startTime;
        private UITweener tw;

        // Original state captured before a preview, restored when it ends.
        private Vector3 savedScale;
        private Vector2 savedPos;
        private Quaternion savedRot;
        private CanvasGroup savedCanvasGroup;
        private float savedAlpha;

        // EditorPrefs key prefix for persisted fold states.
        private const string FoldPrefix = "PixelInventoryDiaryUI.UITweener.fold.";
        private GUIStyle sectionFoldout;

        private void OnEnable()
        {
            tw = target as UITweener;
        }

        private void OnDisable()
        {
            StopPreview();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EnsureStyles();

            DrawTimingAndBehaviour();
            DrawChannelSections();
            DrawPresetsAndEvents();

            serializedObject.ApplyModifiedProperties();

            DrawPreviewSection();
        }

        // ───────────────────────── Section drawing ────────────────────────
        private void DrawTimingAndBehaviour()
        {
            if (Section("timing", "Timing", defaultOpen: true))
            {
                EditorGUI.indentLevel++;
                var easeProp = serializedObject.FindProperty("ease");
                PropField("duration");
                PropField("delay");
                EditorGUILayout.PropertyField(easeProp);
                // Only show the custom curve when Ease = Custom (it's ignored otherwise).
                if (easeProp.enumValueIndex == (int)TweenEase.Custom)
                    PropField("customCurve");
                PropField("loop");
                EditorGUI.indentLevel--;
            }

            if (Section("behaviour", "Behaviour", defaultOpen: false))
            {
                EditorGUI.indentLevel++;
                PropField("playOnEnable");
                PropField("useUnscaledTime");
                PropField("snapToFromOnPlay");
                EditorGUI.indentLevel--;
            }
        }

        private void DrawChannelSections()
        {
            ChannelSection("scale", "Scale", "tweenScale", "fromScale", "toScale");
            ChannelSection("move", "Move", "tweenMove", "moveRelative", "fromPos", "toPos");
            ChannelSection("fade", "Fade", "tweenFade", "fromAlpha", "toAlpha", "controlRaycastByAlpha");
            ChannelSection("rotate", "Rotate", "tweenRotate", "fromAngle", "toAngle");
        }

        private void DrawPresetsAndEvents()
        {
            if (Section("presets", "Presets  (Button OnClick)", defaultOpen: false))
            {
                EditorGUI.indentLevel++;
                PropField("bounceScale");
                PropField("bounceDuration");
                PropField("pulseScale");
                PropField("pulseDuration");
                PropField("shakeStrength");
                PropField("shakeDuration");
                EditorGUI.indentLevel--;
            }

            if (Section("events", "Events", defaultOpen: false))
            {
                EditorGUI.indentLevel++;
                PropField("onComplete");
                EditorGUI.indentLevel--;
            }
        }

        /// <summary>
        /// A collapsible channel section whose header row shows an enable toggle
        /// next to a bold foldout. While the channel is disabled its fields are
        /// drawn greyed-out so it's clear they have no effect.
        /// </summary>
        private void ChannelSection(string key, string label, string enableName, params string[] fieldNames)
        {
            var enableProp = serializedObject.FindProperty(enableName);

            // Foldout (arrow + label) fills the row except for a toggle parked at
            // the far right, so the enable checkbox never overlaps the arrow.
            Rect row = EditorGUILayout.GetControlRect();
            Rect foldRect = new Rect(row.x, row.y, row.width - 20f, row.height);
            Rect toggleRect = new Rect(row.xMax - 16f, row.y, 16f, row.height);

            string foldKey = FoldPrefix + key;
            bool cur = EditorPrefs.GetBool(foldKey, false);
            bool now = EditorGUI.Foldout(foldRect, cur, label, toggleOnLabelClick: true, sectionFoldout);
            if (now != cur)
                EditorPrefs.SetBool(foldKey, now);

            EditorGUI.BeginChangeCheck();
            bool enabled = EditorGUI.Toggle(toggleRect, enableProp.boolValue);
            if (EditorGUI.EndChangeCheck())
                enableProp.boolValue = enabled;

            if (now)
            {
                EditorGUI.indentLevel++;
                using (new EditorGUI.DisabledScope(!enableProp.boolValue))
                {
                    foreach (var f in fieldNames)
                        PropField(f);
                }
                EditorGUI.indentLevel--;
            }
        }

        /// <summary>A plain collapsible section header. Returns true when expanded.</summary>
        private bool Section(string key, string label, bool defaultOpen)
        {
            string foldKey = FoldPrefix + key;
            bool cur = EditorPrefs.GetBool(foldKey, defaultOpen);
            bool now = EditorGUILayout.Foldout(cur, label, toggleOnLabelClick: true, sectionFoldout);
            if (now != cur)
                EditorPrefs.SetBool(foldKey, now);
            return now;
        }

        private void PropField(string propertyName)
        {
            var p = serializedObject.FindProperty(propertyName);
            if (p != null)
                EditorGUILayout.PropertyField(p, includeChildren: true);
        }

        private void EnsureStyles()
        {
            if (sectionFoldout == null)
                sectionFoldout = new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold };
        }

        // ───────────────────────────── Preview ────────────────────────────
        private void DrawPreviewSection()
        {
            EditorGUILayout.Space(8f);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

                if (mode == PreviewMode.None)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("▶ Play")) BeginPreview(PreviewMode.Tween);
                        if (GUILayout.Button("Bounce")) BeginPreview(PreviewMode.Bounce);
                        if (GUILayout.Button("Pulse")) BeginPreview(PreviewMode.Pulse);
                        if (GUILayout.Button("Shake")) BeginPreview(PreviewMode.Shake);
                    }

                    if (!Application.isPlaying)
                    {
                        EditorGUILayout.HelpBox(
                            "Play runs the configured channels From → To. " +
                            "Edit-mode preview restores the original transform when it ends. " +
                            "Fade is only previewed if a CanvasGroup already exists on this object.",
                            MessageType.None);
                    }
                }
                else
                {
                    if (GUILayout.Button("■ Stop Preview"))
                        StopPreview();
                    EditorGUILayout.LabelField("Previewing", mode.ToString());
                }
            }
        }

        private void BeginPreview(PreviewMode m)
        {
            if (tw == null)
                return;

            // In Play mode, use the real runtime methods (coroutines run there).
            // There is no runtime round-trip API, so it falls back to a forward Play.
            if (Application.isPlaying)
            {
                switch (m)
                {
                    case PreviewMode.Tween: tw.Play(); break;
                    case PreviewMode.Bounce: tw.Bounce(); break;
                    case PreviewMode.Pulse: tw.Pulse(); break;
                    case PreviewMode.Shake: tw.Shake(); break;
                }
                return;
            }

            SaveState();
            tw.EditorPrepare();

            mode = m;
            startTime = EditorApplication.timeSinceStartup;
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
        }

        private void StopPreview()
        {
            if (mode == PreviewMode.None)
                return;

            EditorApplication.update -= OnEditorUpdate;
            RestoreState();
            mode = PreviewMode.None;
            RepaintViews();
        }

        private void OnEditorUpdate()
        {
            if (tw == null)
            {
                StopPreview();
                return;
            }

            float elapsed = (float)(EditorApplication.timeSinceStartup - startTime);

            switch (mode)
            {
                case PreviewMode.Tween:
                {
                    float delay = tw.PreviewDelay;
                    float dur = tw.PreviewDuration;
                    float t = elapsed <= delay ? 0f : (elapsed - delay) / dur;
                    tw.EditorSampleTween(Mathf.Clamp01(t), reverse: false);
                    if (elapsed >= delay + dur)
                    {
                        tw.EditorSampleTween(1f, reverse: false);
                        StopPreview();
                        return;
                    }
                    break;
                }

                case PreviewMode.Bounce:
                case PreviewMode.Pulse:
                {
                    bool isBounce = mode == PreviewMode.Bounce;
                    float mult = isBounce ? tw.PreviewBounceScale : tw.PreviewPulseScale;
                    float dur = isBounce ? tw.PreviewBounceDuration : tw.PreviewPulseDuration;
                    if (elapsed >= dur)
                    {
                        tw.EditorSetLocalScale(savedScale);
                        StopPreview();
                        return;
                    }
                    float half = Mathf.Max(0.0001f, dur * 0.5f);
                    float s = elapsed < half
                        ? Mathf.LerpUnclamped(1f, mult, tw.EditorEase(TweenEase.OutQuad, elapsed / half))
                        : Mathf.LerpUnclamped(mult, 1f, tw.EditorEase(TweenEase.OutQuad, (elapsed - half) / half));
                    tw.EditorSetLocalScale(savedScale * s);
                    break;
                }

                case PreviewMode.Shake:
                {
                    float dur = tw.PreviewShakeDuration;
                    float strength = tw.PreviewShakeStrength;
                    if (elapsed >= dur)
                    {
                        tw.EditorSetAnchoredPos(savedPos);
                        StopPreview();
                        return;
                    }
                    float damper = 1f - Mathf.Clamp01(elapsed / dur);
                    float x = (Mathf.PerlinNoise(elapsed * 35f, 0.3f) * 2f - 1f) * strength * damper;
                    float y = (Mathf.PerlinNoise(0.7f, elapsed * 35f) * 2f - 1f) * strength * damper;
                    tw.EditorSetAnchoredPos(savedPos + new Vector2(x, y));
                    break;
                }
            }

            RepaintViews();
        }

        private void SaveState()
        {
            var rt = tw.transform as RectTransform;
            if (rt == null)
                return;

            savedScale = rt.localScale;
            savedPos = rt.anchoredPosition;
            savedRot = rt.localRotation;
            savedCanvasGroup = tw.GetComponent<CanvasGroup>();
            savedAlpha = savedCanvasGroup != null ? savedCanvasGroup.alpha : 1f;
        }

        private void RestoreState()
        {
            var rt = tw.transform as RectTransform;
            if (rt == null)
                return;

            rt.localScale = savedScale;
            rt.anchoredPosition = savedPos;
            rt.localRotation = savedRot;
            if (savedCanvasGroup != null)
                savedCanvasGroup.alpha = savedAlpha;
        }

        private static void RepaintViews()
        {
            SceneView.RepaintAll();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
    }
}
