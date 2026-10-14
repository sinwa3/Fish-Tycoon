using System;
using UnityEngine;

namespace PixelPocketPhoneUI.Tweening
{
    /// <summary>How a tween repeats after finishing one cycle.</summary>
    public enum PixelLoopType
    {
        /// <summary>Play once and stop.</summary>
        None,
        /// <summary>Jump back to the start and play again.</summary>
        Restart,
        /// <summary>Reverse direction each cycle (A->B->A->B...).</summary>
        PingPong
    }

    /// <summary>
    /// A single running tween. You normally do not create this directly —
    /// use <see cref="PixelTween.To"/> or the extension methods in PixelTweenExtensions.
    /// The returned handle supports fluent configuration and runtime control
    /// (Pause / Resume / Kill / Complete).
    /// </summary>
    public sealed class PixelTween
    {
        // ---- Configuration ----
        internal float Duration = 0.3f;
        internal float Delay;
        internal PixelEaseType Ease = PixelEaseType.OutCubic;
        internal AnimationCurve Curve;          // used when Ease == Custom
        internal PixelLoopType Loop = PixelLoopType.None;
        internal int Loops = -1;                 // -1 = infinite, >=1 = finite cycles
        internal bool UnscaledTime = true;       // UI animations usually ignore Time.timeScale
        internal Action<float> OnUpdateCb;       // receives eased 0..1 progress
        internal Action OnCompleteCb;
        internal Func<bool> Validity;            // return false => target gone, auto-kill

        // ---- Runtime state ----
        private float _time;                     // elapsed seconds inside current cycle
        private float _delayLeft;
        private int _cyclesDone;
        private bool _forward = true;
        private bool _reversed;                   // requested via SetReversed, applied in Init
        private bool _initialized;
        private bool _warmup;                     // skip time on the very first progress frame

        /// <summary>True while the tween is registered with the runner (not killed/finished).</summary>
        public bool IsActive { get; internal set; }
        /// <summary>True while actively advancing (false when paused).</summary>
        public bool IsPlaying { get; internal set; }

        private void Init()
        {
            _delayLeft = Delay;
            _time = 0f;
            _cyclesDone = 0;
            _forward = !_reversed;
            _warmup = true;
            _initialized = true;
        }

        /// <summary>
        /// Advances the tween. Returns false when it should be removed from the runner.
        /// </summary>
        internal bool Tick(float scaledDt, float unscaledDt)
        {
            if (!IsActive) return false;

            // Initialize on the first tick so all fluent SetXxx() calls are applied.
            if (!_initialized) Init();

            if (Validity != null && !Validity())
            {
                IsActive = false;
                IsPlaying = false;
                return false;
            }

            if (!IsPlaying) return true;

            float dt = UnscaledTime ? unscaledDt : scaledDt;

            if (_delayLeft > 0f)
            {
                _delayLeft -= dt;
                if (_delayLeft > 0f) return true;
                dt = -_delayLeft;     // carry the overshoot into the first frame
                _delayLeft = 0f;
            }

            // First real frame: emit the start value but don't advance time. This
            // absorbs the large delta that occurs right after Play / scene load,
            // so a tween that starts with delay = 0 doesn't jump on frame one.
            if (_warmup)
            {
                _warmup = false;
                dt = 0f;
            }

            _time += dt;
            float n = Duration <= 0f ? 1f : Mathf.Clamp01(_time / Duration);
            float dir = _forward ? n : 1f - n;
            float eased = (Ease == PixelEaseType.Custom && Curve != null)
                ? Curve.Evaluate(dir)
                : PixelEase.Evaluate(Ease, dir);

            OnUpdateCb?.Invoke(eased);

            if (n < 1f) return true;

            // ---- one cycle finished ----
            if (Loop == PixelLoopType.None)
            {
                Finish();
                return false;
            }

            _cyclesDone++;
            if (Loops > 0 && _cyclesDone >= Loops)
            {
                Finish();
                return false;
            }

            _time = 0f;
            if (Loop == PixelLoopType.PingPong) _forward = !_forward;
            return true;
        }

        private void Finish()
        {
            IsActive = false;
            IsPlaying = false;
            OnCompleteCb?.Invoke();
        }

        // ---------- Fluent configuration ----------

        public PixelTween SetEase(PixelEaseType ease) { Ease = ease; return this; }
        public PixelTween SetEase(AnimationCurve curve) { Ease = PixelEaseType.Custom; Curve = curve; return this; }
        public PixelTween SetDelay(float seconds) { Delay = seconds; if (!_initialized) _delayLeft = seconds; return this; }
        public PixelTween SetUnscaled(bool unscaled) { UnscaledTime = unscaled; return this; }

        /// <param name="count">-1 = infinite, otherwise the number of cycles.</param>
        public PixelTween SetLoop(PixelLoopType type, int count = -1) { Loop = type; Loops = count; return this; }

        public PixelTween SetReversed(bool reversed) { _reversed = reversed; if (_initialized) _forward = !reversed; return this; }
        public PixelTween SetValidity(Func<bool> isValid) { Validity = isValid; return this; }
        public PixelTween OnUpdate(Action<float> cb) { OnUpdateCb += cb; return this; }
        public PixelTween OnComplete(Action cb) { OnCompleteCb += cb; return this; }

        // ---------- Runtime control ----------

        public void Pause() => IsPlaying = false;
        public void Resume() { if (IsActive) IsPlaying = true; }

        /// <summary>Stops immediately. If <paramref name="invokeComplete"/> is true, fires OnComplete.</summary>
        public void Kill(bool invokeComplete = false)
        {
            if (!IsActive) return;
            IsActive = false;
            IsPlaying = false;
            if (invokeComplete) OnCompleteCb?.Invoke();
        }

        /// <summary>Snaps to the final value and fires OnComplete.</summary>
        public void Complete()
        {
            if (!IsActive) return;
            OnUpdateCb?.Invoke(_forward ? 1f : 0f);
            Finish();
        }

        /// <summary>Restarts from the beginning (including the delay).</summary>
        public void Restart()
        {
            _initialized = false;   // re-initializes on the next tick
            IsActive = true;
            IsPlaying = true;
            PixelTweenRunner.Ensure()?.Register(this);
        }

        // ---------- Factory ----------

        /// <summary>
        /// Creates and starts a generic tween. <paramref name="onUpdate"/> receives the
        /// eased progress (usually 0..1) every frame — apply it however you like.
        /// </summary>
        public static PixelTween To(Action<float> onUpdate, float duration)
        {
            var t = new PixelTween
            {
                Duration = duration,
                OnUpdateCb = onUpdate,
                IsActive = true,
                IsPlaying = true
            };
            // Init() runs on the first tick, after all fluent setters have applied.
            PixelTweenRunner.Ensure()?.Register(t);
            return t;
        }
    }
}
