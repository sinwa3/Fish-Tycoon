using System;
using UnityEngine;
using UnityEngine.Events;

namespace PixelPocketPhoneUI.Tweening
{
    /// <summary>
    /// Base class for the drag-and-drop tween components. Add a concrete one
    /// (Fade / Scale / Move / Rotate / Color / Shake) to any UI object, set the
    /// values in the Inspector, and it plays — no code required.
    ///
    /// You can also drive it from code or UnityEvents: <see cref="Play"/>,
    /// <see cref="Stop"/>, <see cref="TogglePause"/>.
    /// </summary>
    public abstract class PixelTweenComponent : MonoBehaviour
    {
        public enum PlayTrigger
        {
            /// <summary>Play automatically every time the object is enabled.</summary>
            OnEnable,
            /// <summary>Play once on Start.</summary>
            OnStart,
            /// <summary>Only play when Play() is called (code / button / UnityEvent).</summary>
            Manual
        }

        [Header("Playback")]
        [Tooltip("When the animation should start.")]
        public PlayTrigger playOn = PlayTrigger.OnEnable;
        [Tooltip("Seconds to wait before the animation begins.")]
        public float delay = 0f;
        [Tooltip("Length of the animation in seconds.")]
        public float duration = 0.3f;
        public PixelEaseType ease = PixelEaseType.OutCubic;
        [Tooltip("Used only when Ease is set to Custom.")]
        public AnimationCurve customCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Looping")]
        public PixelLoopType loop = PixelLoopType.None;
        [Tooltip("How many loop cycles. -1 = forever. Ignored when Loop is None.")]
        public int loopCount = -1;

        [Header("Options")]
        [Tooltip("Ignore Time.timeScale (recommended for UI so it still animates when the game is paused).")]
        public bool useUnscaledTime = true;
        [Tooltip("Play the animation from end to start.")]
        public bool playReversed = false;

        [Header("Events")]
        public UnityEvent onComplete;

        private PixelTween _tween;

        // Cached delegates so repeated Play() calls (e.g. every OnEnable) don't
        // allocate new closures each time.
        private Action<float> _applyDelegate;
        private Func<bool> _validityDelegate;
        private Action _onCompleteDelegate;

        /// <summary>True while this component's tween is running.</summary>
        public bool IsPlaying => _tween != null && _tween.IsActive;

        protected virtual void OnEnable()
        {
            if (playOn == PlayTrigger.OnEnable) Play();
        }

        protected virtual void Start()
        {
            if (playOn == PlayTrigger.OnStart) Play();
        }

        protected virtual void OnDisable()
        {
            Kill();
        }

        /// <summary>Starts (or restarts) the animation using the current Inspector settings.</summary>
        [ContextMenu("Play")]
        public void Play()
        {
            Kill();

            if (!CaptureTargets())
                return;

            ApplyProgress(playReversed ? 1f : 0f); // avoid a one-frame flash at the wrong value

            _applyDelegate ??= ApplyProgress;
            _validityDelegate ??= () => this != null && isActiveAndEnabled;
            _onCompleteDelegate ??= () => onComplete?.Invoke();

            _tween = PixelTween.To(_applyDelegate, duration)
                .SetDelay(delay)
                .SetUnscaled(useUnscaledTime)
                .SetLoop(loop, loopCount)
                .SetReversed(playReversed)
                .SetValidity(_validityDelegate)
                .OnComplete(_onCompleteDelegate);

            if (ForceLinearEase) _tween.SetEase(PixelEaseType.Linear);
            else if (ease == PixelEaseType.Custom) _tween.SetEase(customCurve);
            else _tween.SetEase(ease);
        }

        /// <summary>
        /// When true, the 'Ease' field is ignored and Linear is always used.
        /// Shake overrides this — easing would ruin its constant-speed oscillation.
        /// </summary>
        protected virtual bool ForceLinearEase => false;

        /// <summary>Stops the animation where it is.</summary>
        [ContextMenu("Stop")]
        public void Stop() => Kill();

        /// <summary>Pauses or resumes the running animation.</summary>
        public void TogglePause()
        {
            if (_tween == null || !_tween.IsActive) return;
            if (_tween.IsPlaying) _tween.Pause();
            else _tween.Resume();
        }

        private void Kill()
        {
            _tween?.Kill();
            _tween = null;
        }

        /// <summary>
        /// Cache the components/values this tween needs. Return false to skip playing
        /// (e.g. a required component is missing).
        /// </summary>
        protected abstract bool CaptureTargets();

        /// <summary>Apply eased progress (0..1) to the target. Called every frame.</summary>
        protected abstract void ApplyProgress(float t);

#if UNITY_EDITOR
        // ---- Hooks used by the custom inspector's edit-mode preview ----
        /// <summary>Editor only: capture targets and set the starting value. Returns false if not playable.</summary>
        public bool EditorBeginPreview() => CaptureTargets();

        /// <summary>Editor only: apply the value at a normalized 0..1 time.</summary>
        public void EditorApplyNormalized(float normalized) => ApplyProgress(EvaluateEased(normalized));

        /// <summary>Resolve eased progress for a normalized 0..1 time, honoring ease + reverse.</summary>
        private float EvaluateEased(float normalized)
        {
            float p = playReversed ? 1f - normalized : normalized;
            if (ForceLinearEase) return p;
            return (ease == PixelEaseType.Custom && customCurve != null)
                ? customCurve.Evaluate(p)
                : PixelEase.Evaluate(ease, p);
        }
#endif
    }
}
