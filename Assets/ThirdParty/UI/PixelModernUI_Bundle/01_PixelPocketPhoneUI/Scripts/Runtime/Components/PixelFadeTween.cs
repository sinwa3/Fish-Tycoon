using UnityEngine;

namespace PixelPocketPhoneUI.Tweening
{
    /// <summary>
    /// Fades a UI element's transparency via a CanvasGroup (added automatically).
    /// Great for popups, alerts and screen transitions.
    /// </summary>
    [AddComponentMenu("Pixel Pocket UI/Tween/Pixel Fade Tween")]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class PixelFadeTween : PixelTweenComponent
    {
        [Header("Fade")]
        [Range(0f, 1f)] public float from = 0f;
        [Range(0f, 1f)] public float to = 1f;
        [Tooltip("Start from the element's current alpha instead of the 'From' value.")]
        public bool fromCurrent = false;

        private CanvasGroup _cg;
        private float _start, _end;

        protected override bool CaptureTargets()
        {
            _cg = GetComponent<CanvasGroup>();
            _start = fromCurrent ? _cg.alpha : from;
            _end = to;
            return true;
        }

        protected override void ApplyProgress(float t)
        {
            if (_cg == null) return;
            _cg.alpha = Mathf.LerpUnclamped(_start, _end, t);
        }
    }
}
