using UnityEngine;

namespace PixelPocketPhoneUI.Tweening
{
    /// <summary>
    /// Rotates a Transform around its local Z (or any axis) by animating euler angles.
    /// A linear 0->360 loop makes a perfect loading spinner.
    /// </summary>
    [AddComponentMenu("Pixel Pocket UI/Tween/Pixel Rotate Tween")]
    public sealed class PixelRotateTween : PixelTweenComponent
    {
        [Header("Rotate (euler angles)")]
        public Vector3 from = Vector3.zero;
        public Vector3 to = new Vector3(0f, 0f, -360f);
        [Tooltip("Start from the object's current rotation instead of the 'From' value.")]
        public bool fromCurrent = false;
        [Tooltip("Treat 'To' as an offset relative to the start rotation.")]
        public bool toIsOffset = false;

        private Transform _tr;
        private Vector3 _start, _end;

        protected override bool CaptureTargets()
        {
            _tr = transform;
            _start = fromCurrent ? _tr.localEulerAngles : from;
            _end = toIsOffset ? _start + to : to;
            return true;
        }

        protected override void ApplyProgress(float t)
        {
            if (_tr == null) return;
            _tr.localEulerAngles = Vector3.LerpUnclamped(_start, _end, t);
        }
    }
}
