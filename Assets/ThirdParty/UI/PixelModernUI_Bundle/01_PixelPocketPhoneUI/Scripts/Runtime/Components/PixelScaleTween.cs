using UnityEngine;

namespace PixelPocketPhoneUI.Tweening
{
    /// <summary>
    /// Scales a Transform. Use OutBack easing for a satisfying "pop" on popups,
    /// or a short PingPong loop for a button press / heartbeat effect.
    /// </summary>
    [AddComponentMenu("Pixel Pocket UI/Tween/Pixel Scale Tween")]
    public sealed class PixelScaleTween : PixelTweenComponent
    {
        [Header("Scale")]
        public Vector3 from = Vector3.one * 0.6f;
        public Vector3 to = Vector3.one;
        [Tooltip("Start from the object's current scale instead of the 'From' value.")]
        public bool fromCurrent = false;

        private Transform _tr;
        private Vector3 _start, _end;

        protected override bool CaptureTargets()
        {
            _tr = transform;
            _start = fromCurrent ? _tr.localScale : from;
            _end = to;
            return true;
        }

        protected override void ApplyProgress(float t)
        {
            if (_tr == null) return;
            _tr.localScale = Vector3.LerpUnclamped(_start, _end, t);
        }
    }
}
