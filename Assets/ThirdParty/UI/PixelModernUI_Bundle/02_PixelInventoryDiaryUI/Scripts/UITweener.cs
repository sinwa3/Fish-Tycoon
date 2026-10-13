using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace PixelInventoryDiaryUI
{
    /// <summary>Easing functions supported by <see cref="UITweener"/>.</summary>
    public enum TweenEase
    {
        Linear,
        InQuad, OutQuad, InOutQuad,
        InCubic, OutCubic, InOutCubic,
        InBack, OutBack,
        OutElastic,
        OutBounce,
        Custom
    }

    /// <summary>How the tween repeats.</summary>
    public enum TweenLoop { Once, Loop, PingPong }

    /// <summary>
    /// A lightweight, dependency-free UI tween for UGUI. Animate scale, move
    /// (anchoredPosition), fade (CanvasGroup) and rotation on a single component,
    /// with easing, delay and looping. Great for "appear" pop-ins and button feedback.
    ///
    /// Usage:
    ///  - Enable the channels you want (Scale / Move / Fade / Rotate) and set From/To.
    ///  - Turn on "Play On Enable" for an entrance animation whenever the object is shown.
    ///  - Wire a Button's OnClick to <see cref="Bounce"/>, <see cref="Pulse"/> or
    ///    <see cref="Shake"/> for instant tactile feedback.
    ///  - In Play mode, right-click the component header to try Play / Bounce / Shake.
    /// </summary>
    [AddComponentMenu("Pixel Inventory Diary UI/UI Tweener")]
    [DisallowMultipleComponent]
    public class UITweener : MonoBehaviour
    {
        // ─────────────────────────── Timing ───────────────────────────
        [Header("Timing")]
        [Tooltip("Duration of one pass, in seconds.")]
        [Min(0f)] [SerializeField] private float duration = 0.3f;

        [Tooltip("Delay before the tween starts, in seconds.")]
        [Min(0f)] [SerializeField] private float delay = 0f;

        [Tooltip("Easing curve applied to progress.")]
        [SerializeField] private TweenEase ease = TweenEase.OutBack;

        [Tooltip("Used only when Ease = Custom. X: 0..1 time, Y: eased progress.")]
        [SerializeField] private AnimationCurve customCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Tooltip("Once: play a single pass. Loop: restart forever. PingPong: bounce back and forth.")]
        [SerializeField] private TweenLoop loop = TweenLoop.Once;

        // ────────────────────────── Behaviour ─────────────────────────
        [Header("Behaviour")]
        [Tooltip("Play automatically every time this object is enabled (entrance animation).")]
        [SerializeField] private bool playOnEnable = true;

        [Tooltip("Use unscaled time so tweens keep running while the game is paused (Time.timeScale = 0). Recommended for UI.")]
        [SerializeField] private bool useUnscaledTime = true;

        [Tooltip("Snap to the From values the moment playback starts, so entrances always begin cleanly.")]
        [SerializeField] private bool snapToFromOnPlay = true;

        // ──────────────────────────── Scale ───────────────────────────
        [Header("Scale")]
        [SerializeField] private bool tweenScale = false;
        [SerializeField] private Vector3 fromScale = Vector3.one * 0.8f;
        [SerializeField] private Vector3 toScale = Vector3.one;

        // ──────────────────── Move (anchoredPosition) ─────────────────
        [Header("Move  (anchoredPosition)")]
        [SerializeField] private bool tweenMove = false;
        [Tooltip("Treat From/To as offsets from the position captured on Awake, instead of absolute anchored positions.")]
        [SerializeField] private bool moveRelative = true;
        [SerializeField] private Vector2 fromPos = new Vector2(0f, -40f);
        [SerializeField] private Vector2 toPos = Vector2.zero;

        // ──────────────────────── Fade (CanvasGroup) ──────────────────
        [Header("Fade  (CanvasGroup)")]
        [SerializeField] private bool tweenFade = false;
        [Range(0f, 1f)] [SerializeField] private float fromAlpha = 0f;
        [Range(0f, 1f)] [SerializeField] private float toAlpha = 1f;
        [Tooltip("Disable raycasts and interaction while the group is (nearly) transparent, so invisible UI can't be clicked.")]
        [SerializeField] private bool controlRaycastByAlpha = true;

        // ──────────────────────────── Rotate ──────────────────────────
        [Header("Rotate  (Z axis, degrees)")]
        [SerializeField] private bool tweenRotate = false;
        [SerializeField] private float fromAngle = 0f;
        [SerializeField] private float toAngle = 0f;

        // ─────────────────── Presets (Button OnClick) ─────────────────
        [Header("Presets  (call from Button OnClick)")]
        [Tooltip("Bounce(): quick press-in. Scale multiplier at the low point.")]
        [SerializeField] private float bounceScale = 0.9f;
        [Min(0.01f)] [SerializeField] private float bounceDuration = 0.12f;

        [Tooltip("Pulse(): quick emphasis. Scale multiplier at the peak.")]
        [SerializeField] private float pulseScale = 1.12f;
        [Min(0.01f)] [SerializeField] private float pulseDuration = 0.16f;

        [Tooltip("Shake(): positional shake strength, in pixels.")]
        [SerializeField] private float shakeStrength = 10f;
        [Min(0.01f)] [SerializeField] private float shakeDuration = 0.3f;

        // ──────────────────────────── Events ──────────────────────────
        [Header("Events")]
        [Tooltip("Invoked when a non-looping tween (or a preset) finishes.")]
        [SerializeField] private UnityEvent onComplete = new UnityEvent();

        // ──────────────────────────── State ───────────────────────────
        private RectTransform rt;
        private CanvasGroup canvasGroup;
        private Coroutine routine;
        private Vector2 basePos;
        private bool baseCaptured;

        /// <summary>True while a tween or preset is running.</summary>
        public bool IsPlaying { get; private set; }

        /// <summary>Fires when a non-looping tween or a preset finishes.</summary>
        public UnityEvent OnComplete => onComplete;

        /// <summary>Playback duration of one pass, in seconds (clamped to a small minimum).</summary>
        public float Duration
        {
            get => duration;
            set => duration = Mathf.Max(0f, value);
        }

        /// <summary>Delay before the tween starts, in seconds. Used by staggered playback.</summary>
        public float Delay
        {
            get => delay;
            set => delay = Mathf.Max(0f, value);
        }

        private float DeltaTime => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        // ─────────────────────────── Lifecycle ────────────────────────
        private void Awake()
        {
            rt = transform as RectTransform;
            if (tweenFade) EnsureCanvasGroup();
            CaptureBase();
        }

        private void OnEnable()
        {
            if (playOnEnable)
                Play();
        }

        private void OnDisable()
        {
            // Stop silently: don't fire OnComplete, so re-enabling starts fresh.
            StopInternal(invokeComplete: false);
        }

        // ────────────────────────── Public API ────────────────────────
        /// <summary>Plays the configured channels From → To.</summary>
        [ContextMenu("Play")]
        public void Play() => PlayDirectional(reverse: false);

        /// <summary>Alias for <see cref="Play"/> (From → To).</summary>
        public void PlayForward() => PlayDirectional(reverse: false);

        /// <summary>Plays To → From, e.g. to close/hide a panel that was opened with Play.</summary>
        [ContextMenu("Play Reverse")]
        public void PlayReverse() => PlayDirectional(reverse: true);

        /// <summary>Restarts the forward tween from the beginning.</summary>
        public void Restart() => Play();

        /// <summary>Stops the current tween/preset immediately (fires OnComplete).</summary>
        public void Stop() => StopInternal(invokeComplete: true);

        /// <summary>Quick press-in feedback (scale down and back). Ignores channel setup.</summary>
        [ContextMenu("Bounce")]
        public void Bounce() => StartPreset(ScalePunchRoutine(bounceScale, bounceDuration));

        /// <summary>Quick emphasis pop (scale up and back). Ignores channel setup.</summary>
        [ContextMenu("Pulse")]
        public void Pulse() => StartPreset(ScalePunchRoutine(pulseScale, pulseDuration));

        /// <summary>Positional shake (e.g. invalid action feedback). Ignores channel setup.</summary>
        [ContextMenu("Shake")]
        public void Shake() => StartPreset(ShakeRoutine(shakeStrength, shakeDuration));

        // ──────────────────────── Core playback ───────────────────────
        private void PlayDirectional(bool reverse)
        {
            if (!isActiveAndEnabled) return; // coroutines can't run on a disabled object
            if (rt == null) rt = transform as RectTransform;
            if (tweenFade) EnsureCanvasGroup();
            CaptureBase();

            StopActive();
            routine = StartCoroutine(TweenRoutine(reverse));
        }

        private IEnumerator TweenRoutine(bool reverse)
        {
            IsPlaying = true;

            if (snapToFromOnPlay)
                Apply(reverse ? 1f : 0f);

            // Let the From pose render for a frame first.
            yield return null;

            // Delay — use a REAL-TIME wait so a heavy frame (scene load, tab switch)
            // can't skip it. Counting it down with per-frame deltaTime lets an
            // inflated frame swallow the whole delay, which made every staggered item
            // appear at once. WaitForSecondsRealtime is unscaled (matches useUnscaledTime).
            if (delay > 0f)
            {
                if (useUnscaledTime)
                    yield return new WaitForSecondsRealtime(delay);
                else
                    yield return new WaitForSeconds(delay);
            }

            float dur = Mathf.Max(0.0001f, duration);
            float t = 0f;
            int dir = 1;
            bool firstStep = true;

            while (true)
            {
                // The frames right after an object is enabled often have an inflated
                // deltaTime (scene load, layout rebuild, activation work). We ignore it
                // on the first step (so the "From" pose renders), then cap later steps.
                // The cap is min(dur*0.5, ~34ms): the dur*0.5 term keeps even a very
                // short tween to 2+ steps, and the ~34ms (~30fps) term stops a stall
                // right after SetActive from collapsing a normal-length tween into 2-3
                // frames — which made it look like the tween "barely played" when a
                // panel (e.g. the diary) was toggled on.
                float cap = Mathf.Min(dur * 0.5f, 0.034f);
                float dt = firstStep ? 0f : Mathf.Min(DeltaTime, cap);
                firstStep = false;

                t += (dt / dur) * dir;

                if (loop == TweenLoop.Once)
                {
                    if (t >= 1f)
                    {
                        Apply(Coeff(reverse, 1f));
                        break;
                    }
                }
                else if (loop == TweenLoop.PingPong)
                {
                    if (dir > 0 && t >= 1f) { t = 1f; dir = -1; }
                    else if (dir < 0 && t <= 0f) { t = 0f; dir = 1; }
                }
                else // Loop
                {
                    if (t >= 1f) t -= 1f;
                }

                Apply(Coeff(reverse, Mathf.Clamp01(t)));
                yield return null;
            }

            IsPlaying = false;
            routine = null;
            onComplete.Invoke();
        }

        /// <summary>Turns raw 0..1 time into the final lerp factor (easing + direction).</summary>
        private float Coeff(bool reverse, float t)
        {
            float e = Evaluate(ease, t);
            return reverse ? 1f - e : e;
        }

        /// <summary>Applies a 0..1 lerp factor to every enabled channel.</summary>
        private void Apply(float p)
        {
            if (rt == null) return;

            if (tweenScale)
                rt.localScale = Vector3.LerpUnclamped(fromScale, toScale, p);

            if (tweenMove)
            {
                Vector2 offset = Vector2.LerpUnclamped(fromPos, toPos, p);
                rt.anchoredPosition = moveRelative ? basePos + offset : offset;
            }

            if (tweenRotate)
                rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpUnclamped(fromAngle, toAngle, p));

            if (tweenFade && canvasGroup != null)
            {
                float a = Mathf.Clamp01(Mathf.LerpUnclamped(fromAlpha, toAlpha, p));
                canvasGroup.alpha = a;
                if (controlRaycastByAlpha)
                {
                    bool visible = a > 0.001f;
                    canvasGroup.blocksRaycasts = visible;
                    canvasGroup.interactable = visible;
                }
            }
        }

        // ───────────────────────────── Presets ────────────────────────
        private void StartPreset(IEnumerator preset)
        {
            if (!isActiveAndEnabled) return;
            if (rt == null) rt = transform as RectTransform;
            StopActive();
            routine = StartCoroutine(preset);
        }

        private IEnumerator ScalePunchRoutine(float multiplier, float dur)
        {
            IsPlaying = true;

            Vector3 baseScale = rt.localScale;
            Vector3 peak = baseScale * multiplier;
            float half = Mathf.Max(0.0001f, dur * 0.5f);

            float t = 0f;
            while (t < 1f)
            {
                t += DeltaTime / half;
                rt.localScale = Vector3.LerpUnclamped(baseScale, peak, Evaluate(TweenEase.OutQuad, Mathf.Clamp01(t)));
                yield return null;
            }

            t = 0f;
            while (t < 1f)
            {
                t += DeltaTime / half;
                rt.localScale = Vector3.LerpUnclamped(peak, baseScale, Evaluate(TweenEase.OutQuad, Mathf.Clamp01(t)));
                yield return null;
            }

            rt.localScale = baseScale;
            IsPlaying = false;
            routine = null;
            onComplete.Invoke();
        }

        private IEnumerator ShakeRoutine(float strength, float dur)
        {
            IsPlaying = true;

            Vector2 origin = rt.anchoredPosition;
            float elapsed = 0f;
            while (elapsed < dur)
            {
                elapsed += DeltaTime;
                float damper = 1f - Mathf.Clamp01(elapsed / dur);
                float x = (Random.value * 2f - 1f) * strength * damper;
                float y = (Random.value * 2f - 1f) * strength * damper;
                rt.anchoredPosition = origin + new Vector2(x, y);
                yield return null;
            }

            rt.anchoredPosition = origin;
            IsPlaying = false;
            routine = null;
            onComplete.Invoke();
        }

        // ───────────────────────────── Helpers ────────────────────────
        private void StopInternal(bool invokeComplete)
        {
            StopActive();
            if (IsPlaying)
            {
                IsPlaying = false;
                if (invokeComplete)
                    onComplete.Invoke();
            }
        }

        private void StopActive()
        {
            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }
        }

        private void CaptureBase()
        {
            if (!baseCaptured && rt != null)
            {
                basePos = rt.anchoredPosition;
                baseCaptured = true;
            }
        }

        private void EnsureCanvasGroup()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        /// <summary>Evaluates an easing function at normalized time t (0..1).</summary>
        private float Evaluate(TweenEase type, float t)
        {
            switch (type)
            {
                case TweenEase.Linear: return t;

                case TweenEase.InQuad: return t * t;
                case TweenEase.OutQuad: return t * (2f - t);
                case TweenEase.InOutQuad: return t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;

                case TweenEase.InCubic: return t * t * t;
                case TweenEase.OutCubic: { float f = t - 1f; return f * f * f + 1f; }
                case TweenEase.InOutCubic:
                    return t < 0.5f ? 4f * t * t * t : (t - 1f) * (2f * t - 2f) * (2f * t - 2f) + 1f;

                case TweenEase.InBack: { const float s = 1.70158f; return t * t * ((s + 1f) * t - s); }
                case TweenEase.OutBack: { const float s = 1.70158f; float f = t - 1f; return f * f * ((s + 1f) * f + s) + 1f; }

                case TweenEase.OutElastic:
                {
                    if (t <= 0f) return 0f;
                    if (t >= 1f) return 1f;
                    const float p = 0.3f;
                    return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - p / 4f) * (2f * Mathf.PI) / p) + 1f;
                }

                case TweenEase.OutBounce: return OutBounce(t);

                case TweenEase.Custom: return customCurve != null ? customCurve.Evaluate(t) : t;

                default: return t;
            }
        }

        private static float OutBounce(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;
            if (t < 1f / d1) return n1 * t * t;
            if (t < 2f / d1) { t -= 1.5f / d1; return n1 * t * t + 0.75f; }
            if (t < 2.5f / d1) { t -= 2.25f / d1; return n1 * t * t + 0.9375f; }
            t -= 2.625f / d1;
            return n1 * t * t + 0.984375f;
        }

#if UNITY_EDITOR
        // ─────────────────── Edit-mode preview hooks ──────────────────
        // Called by UITweenerEditor to preview tweens without entering Play mode.
        internal void EditorPrepare()
        {
            if (rt == null) rt = transform as RectTransform;
            // Never auto-add a CanvasGroup in edit mode; only preview fade if one already exists.
            if (tweenFade && canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            baseCaptured = false;
            CaptureBase();
        }

        internal void EditorSampleTween(float t01, bool reverse)
        {
            Apply(Coeff(reverse, Mathf.Clamp01(t01)));
        }

        internal float EditorEase(TweenEase type, float t) => Evaluate(type, Mathf.Clamp01(t));

        internal void EditorSetLocalScale(Vector3 s)
        {
            if (rt == null) rt = transform as RectTransform;
            if (rt != null) rt.localScale = s;
        }

        internal void EditorSetAnchoredPos(Vector2 p)
        {
            if (rt == null) rt = transform as RectTransform;
            if (rt != null) rt.anchoredPosition = p;
        }

        internal float PreviewDuration => Mathf.Max(0.0001f, duration);
        internal float PreviewDelay => Mathf.Max(0f, delay);
        internal float PreviewBounceScale => bounceScale;
        internal float PreviewBounceDuration => Mathf.Max(0.01f, bounceDuration);
        internal float PreviewPulseScale => pulseScale;
        internal float PreviewPulseDuration => Mathf.Max(0.01f, pulseDuration);
        internal float PreviewShakeStrength => shakeStrength;
        internal float PreviewShakeDuration => Mathf.Max(0.01f, shakeDuration);

        private void OnValidate()
        {
            duration = Mathf.Max(0f, duration);
            delay = Mathf.Max(0f, delay);
            bounceDuration = Mathf.Max(0.01f, bounceDuration);
            pulseDuration = Mathf.Max(0.01f, pulseDuration);
            shakeDuration = Mathf.Max(0.01f, shakeDuration);
            fromAlpha = Mathf.Clamp01(fromAlpha);
            toAlpha = Mathf.Clamp01(toAlpha);
        }
#endif
    }
}
