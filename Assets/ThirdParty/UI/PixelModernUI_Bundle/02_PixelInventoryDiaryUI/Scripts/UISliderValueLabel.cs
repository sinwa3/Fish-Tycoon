using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PixelInventoryDiaryUI
{
    /// <summary>
    /// Mirrors a <see cref="Slider"/>'s value into a TextMeshPro label, remapped to
    /// a display range (default 1–100) and formatted. Updates live whenever the
    /// slider changes and previews in the editor. Perfect for stat bars that need a
    /// readable number next to the fill (e.g. Stamina 63 / 100).
    ///
    /// Usage:
    ///  - Add this to the stat row (or any object), then assign the <b>Slider</b>
    ///    and the <b>Label</b> (the TMP text that should show the number).
    ///  - Set the Display Range to the numbers you want at the slider's ends
    ///    (1 → 100 by default), and a Format like "{0}", "{0}%" or "{0}/100".
    ///  - The label follows the slider automatically, at runtime and in the editor.
    /// </summary>
    [AddComponentMenu("Pixel Inventory Diary UI/UI Slider Value Label")]
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class UISliderValueLabel : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The slider whose value is displayed. If left empty, one is searched on this object and its children.")]
        [SerializeField] private Slider slider;

        [Tooltip("The TMP text that shows the number. If left empty, one is searched on this object and its children.")]
        [SerializeField] private TMP_Text label;

        [Header("Display Range")]
        [Tooltip("Number shown when the slider sits at its minimum (0 = shows 0 when empty).")]
        [SerializeField] private float displayMin = 0f;

        [Tooltip("Number shown when the slider sits at its maximum.")]
        [SerializeField] private float displayMax = 100f;

        [Header("Format")]
        [Tooltip("Composite format string. {0} is the remapped value. Examples: \"{0}\", \"{0}%\", \"{0}/100\".")]
        [SerializeField] private string format = "{0}";

        [Tooltip("Decimal places to show (0 = whole numbers).")]
        [Min(0)] [SerializeField] private int decimals = 0;

        [Header("Animate")]
        [Tooltip("On enable, sweep from empty up to the value — the bar fills and the number counts up.")]
        [SerializeField] private bool animateOnEnable = false;

        [Tooltip("Sweep duration in seconds.")]
        [Min(0f)] [SerializeField] private float animateDuration = 0.5f;

        /// <summary>The slider being mirrored. Setting it re-subscribes and refreshes.</summary>
        public Slider Slider
        {
            get => slider;
            set
            {
                if (slider == value) return;
                Unsubscribe();
                slider = value;
                Subscribe();
                // Re-capture the animate target from the new slider on the next enable,
                // so a swapped-in slider animates to its own value, not the old one's.
                animateTargetCaptured = false;
                Refresh();
            }
        }

        /// <summary>The label being written to.</summary>
        public TMP_Text Label
        {
            get => label;
            set { label = value; Refresh(); }
        }

        private void Reset()
        {
            slider = GetComponentInChildren<Slider>();
            label = FindValueLabel();
        }

        /// <summary>
        /// Picks the most likely value label: a child TMP text that already holds a
        /// number (e.g. "80"), so the stat-name text next to it isn't chosen by
        /// mistake. Falls back to the first TMP text found.
        /// </summary>
        private TMP_Text FindValueLabel()
        {
            var texts = GetComponentsInChildren<TMP_Text>(includeInactive: true);
            foreach (var t in texts)
            {
                if (t == null) continue;
                string raw = t.text != null ? t.text.Replace(",", "").Trim() : string.Empty;
                if (float.TryParse(raw, out _))
                    return t;
            }
            return texts.Length > 0 ? texts[0] : null;
        }

        // The designed value, captured once before any sweep can overwrite it.
        private float animateTarget;
        private bool animateTargetCaptured;

        private void OnEnable()
        {
            Subscribe();

            // Capture the designed slider value the first time we're enabled, before any
            // sweep runs. The sweep animates slider.value from 0 up to the target, so if
            // the panel is toggled off mid-sweep (e.g. the profile tab hidden right after
            // scene start), slider.value is left at a low in-between value. Reading it
            // again on the next enable would make that low value the new target and
            // permanently collapse every stat toward ~9 — so we keep the real target here.
            if (slider != null && !animateTargetCaptured)
            {
                animateTarget = slider.value;
                animateTargetCaptured = true;
            }

            if (Application.isPlaying && animateOnEnable && slider != null && animateDuration > 0f)
                StartCoroutine(Sweep(slider.minValue, animateTarget));
            else
                Refresh();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        /// <summary>Animates the slider from its current value to a new one — the bar
        /// fills and the number counts up. Falls back to an instant set outside play mode.</summary>
        public void AnimateTo(float sliderValue)
        {
            if (slider == null)
                return;

            if (!Application.isPlaying || animateDuration <= 0f)
            {
                slider.value = sliderValue;
                Refresh();
                return;
            }

            // Keep the captured target in sync so a later re-enable animates to this value.
            animateTarget = sliderValue;
            animateTargetCaptured = true;
            StopAllCoroutines();
            StartCoroutine(Sweep(slider.value, sliderValue));
        }

        private IEnumerator Sweep(float from, float to)
        {
            float t = 0f;
            while (t < animateDuration)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / animateDuration);
                p = 1f - (1f - p) * (1f - p) * (1f - p); // OutCubic — eases to a smooth stop
                slider.SetValueWithoutNotify(Mathf.Lerp(from, to, p));
                Refresh();
                yield return null;
            }
            slider.SetValueWithoutNotify(to);
            Refresh();
        }

        private void Subscribe()
        {
            if (slider != null)
            {
                // Remove first so repeated Subscribe calls can never stack duplicate listeners.
                slider.onValueChanged.RemoveListener(OnSliderChanged);
                slider.onValueChanged.AddListener(OnSliderChanged);
            }
        }

        private void Unsubscribe()
        {
            if (slider != null)
                slider.onValueChanged.RemoveListener(OnSliderChanged);
        }

        private void OnSliderChanged(float _) => Refresh();

        // Cache the "F{n}" format string so Refresh() doesn't rebuild it every frame
        // during a count-up sweep (matters when several stat bars animate at once).
        private int cachedDecimals = -1;
        private string cachedNumberFormat = "F0";

        /// <summary>Recomputes and writes the label text from the current slider value.</summary>
        public void Refresh()
        {
            if (slider == null || label == null)
                return;

            float display = Mathf.LerpUnclamped(displayMin, displayMax, slider.normalizedValue);
            int places = Mathf.Max(0, decimals);
            if (places != cachedDecimals)
            {
                cachedDecimals = places;
                cachedNumberFormat = "F" + places;
            }
            float rounded = places == 0
                ? Mathf.Round(display)
                : (float)System.Math.Round(display, places);

            label.text = string.Format(format, rounded.ToString(cachedNumberFormat, System.Globalization.CultureInfo.InvariantCulture));
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            decimals = Mathf.Max(0, decimals);
            if (slider == null) slider = GetComponentInChildren<Slider>();
            if (label == null) label = FindValueLabel();

            // Reflect edits immediately in the editor without waiting for play mode.
            if (!Application.isPlaying)
                UnityEditor.EditorApplication.delayCall += DeferredEditorRefresh;
        }

        private void DeferredEditorRefresh()
        {
            UnityEditor.EditorApplication.delayCall -= DeferredEditorRefresh;
            if (this != null)
                Refresh();
        }
#endif
    }
}
