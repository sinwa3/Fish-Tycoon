using UnityEngine;

namespace PixelPocketPhoneUI.Tweening
{
    /// <summary>
    /// Shakes a UI element around its current position, with the amplitude
    /// decaying to zero over the duration. Use it for error feedback
    /// (e.g. a wrong-password input) or incoming-call buzz.
    /// Linear easing is recommended.
    /// </summary>
    [AddComponentMenu("Pixel Pocket UI/Tween/Pixel Shake Tween")]
    [RequireComponent(typeof(RectTransform))]
    public sealed class PixelShakeTween : PixelTweenComponent
    {
        [Header("Shake")]
        [Tooltip("Maximum offset in pixels on each axis.")]
        public Vector2 strength = new Vector2(28f, 0f);
        [Tooltip("How many back-and-forth oscillations over the duration.")]
        public int vibrato = 10;
        [Tooltip("0 = amplitude stays constant, 1 = fully fades out by the end.")]
        [Range(0f, 1f)] public float falloff = 1f;

        private RectTransform _rt;
        private Vector2 _origin;

        // Shake must use constant-speed (linear) progress, otherwise the
        // oscillation bunches up at the start and looks like nothing happens.
        // This is enforced regardless of the inspector's Ease setting.
        protected override bool ForceLinearEase => true;

        // Shake-specific defaults applied when the component is first added.
        private void Reset()
        {
            duration = 0.5f;
            ease = PixelEaseType.Linear;
            playOn = PlayTrigger.OnEnable;
        }

        protected override bool CaptureTargets()
        {
            _rt = (RectTransform)transform;
            _origin = _rt.anchoredPosition;
            return true;
        }

        protected override void ApplyProgress(float t)
        {
            if (_rt == null) return;
            // t is the eased 0..1 progress. Amplitude optionally fades as t -> 1.
            float damper = Mathf.LerpUnclamped(1f, 1f - t, falloff);
            float phase = t * vibrato * Mathf.PI * 2f;
            Vector2 offset;
            offset.x = Mathf.Sin(phase) * strength.x * damper;
            offset.y = Mathf.Cos(phase * 1.3f) * strength.y * damper;
            _rt.anchoredPosition = _origin + offset;
        }
    }
}
