using UnityEngine;
using UnityEngine.UI;

namespace PixelPocketPhoneUI.Tweening
{
    /// <summary>
    /// Convenience extension methods for animating common Unity UI / Transform
    /// properties from code. Every method returns the <see cref="PixelTween"/>
    /// handle so you can chain configuration:
    /// <code>
    /// myCanvasGroup.TweenFade(1f, 0.25f).SetEase(PixelEaseType.OutCubic);
    /// myRect.TweenAnchoredMove(Vector2.zero, 0.3f).SetDelay(0.1f);
    /// </code>
    /// All tweens auto-cancel if their target object is destroyed.
    /// </summary>
    public static class PixelTweenExtensions
    {
        // ---------------- CanvasGroup ----------------

        public static PixelTween TweenFade(this CanvasGroup cg, float to, float duration)
        {
            float from = cg.alpha;
            return PixelTween.To(t => { if (cg) cg.alpha = Mathf.LerpUnclamped(from, to, t); }, duration)
                             .SetValidity(() => cg);
        }

        // ---------------- Transform / RectTransform ----------------

        public static PixelTween TweenScale(this Transform tr, Vector3 to, float duration)
        {
            Vector3 from = tr.localScale;
            return PixelTween.To(t => { if (tr) tr.localScale = Vector3.LerpUnclamped(from, to, t); }, duration)
                             .SetValidity(() => tr);
        }

        public static PixelTween TweenScale(this Transform tr, float uniformTo, float duration)
            => tr.TweenScale(Vector3.one * uniformTo, duration);

        public static PixelTween TweenMove(this Transform tr, Vector3 to, float duration)
        {
            Vector3 from = tr.position;
            return PixelTween.To(t => { if (tr) tr.position = Vector3.LerpUnclamped(from, to, t); }, duration)
                             .SetValidity(() => tr);
        }

        public static PixelTween TweenLocalMove(this Transform tr, Vector3 to, float duration)
        {
            Vector3 from = tr.localPosition;
            return PixelTween.To(t => { if (tr) tr.localPosition = Vector3.LerpUnclamped(from, to, t); }, duration)
                             .SetValidity(() => tr);
        }

        public static PixelTween TweenRotate(this Transform tr, Vector3 toEuler, float duration)
        {
            Vector3 from = tr.localEulerAngles;
            return PixelTween.To(t => { if (tr) tr.localEulerAngles = Vector3.LerpUnclamped(from, toEuler, t); }, duration)
                             .SetValidity(() => tr);
        }

        public static PixelTween TweenAnchoredMove(this RectTransform rt, Vector2 to, float duration)
        {
            Vector2 from = rt.anchoredPosition;
            return PixelTween.To(t => { if (rt) rt.anchoredPosition = Vector2.LerpUnclamped(from, to, t); }, duration)
                             .SetValidity(() => rt);
        }

        // ---------------- Graphic (Image / RawImage / TMP_Text) ----------------

        public static PixelTween TweenColor(this Graphic g, Color to, float duration)
        {
            Color from = g.color;
            return PixelTween.To(t => { if (g) g.color = Color.LerpUnclamped(from, to, t); }, duration)
                             .SetValidity(() => g);
        }

        public static PixelTween TweenAlpha(this Graphic g, float to, float duration)
        {
            Color from = g.color;
            return PixelTween.To(t =>
            {
                if (!g) return;
                Color c = from;
                c.a = Mathf.LerpUnclamped(from.a, to, t);
                g.color = c;
            }, duration).SetValidity(() => g);
        }

        public static PixelTween TweenFillAmount(this Image img, float to, float duration)
        {
            float from = img.fillAmount;
            return PixelTween.To(t => { if (img) img.fillAmount = Mathf.LerpUnclamped(from, to, t); }, duration)
                             .SetValidity(() => img);
        }
    }
}
