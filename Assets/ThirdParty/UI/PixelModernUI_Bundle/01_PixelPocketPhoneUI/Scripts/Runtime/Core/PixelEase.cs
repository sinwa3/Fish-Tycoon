namespace PixelPocketPhoneUI.Tweening
{
    /// <summary>
    /// Easing curve presets used by the Pixel Pocket tween system.
    /// All functions are normalized: input/output map 0..1 to 0..1.
    /// </summary>
    public enum PixelEaseType
    {
        Linear,
        InSine, OutSine, InOutSine,
        InQuad, OutQuad, InOutQuad,
        InCubic, OutCubic, InOutCubic,
        InQuart, OutQuart, InOutQuart,
        InExpo, OutExpo, InOutExpo,
        InBack, OutBack, InOutBack,
        OutBounce,
        OutElastic,
        /// <summary>Use the custom AnimationCurve supplied on the tween instead of a preset.</summary>
        Custom
    }

    /// <summary>
    /// Stateless easing evaluator. No allocations, no dependencies.
    /// </summary>
    public static class PixelEase
    {
        private const float PI = 3.1415926535897931f;

        public static float Evaluate(PixelEaseType type, float t)
        {
            switch (type)
            {
                case PixelEaseType.Linear: return t;

                case PixelEaseType.InSine: return 1f - Cos((t * PI) * 0.5f);
                case PixelEaseType.OutSine: return Sin((t * PI) * 0.5f);
                case PixelEaseType.InOutSine: return -(Cos(PI * t) - 1f) * 0.5f;

                case PixelEaseType.InQuad: return t * t;
                case PixelEaseType.OutQuad: return 1f - (1f - t) * (1f - t);
                case PixelEaseType.InOutQuad:
                    return t < 0.5f ? 2f * t * t : 1f - Pow(-2f * t + 2f, 2f) * 0.5f;

                case PixelEaseType.InCubic: return t * t * t;
                case PixelEaseType.OutCubic: return 1f - Pow(1f - t, 3f);
                case PixelEaseType.InOutCubic:
                    return t < 0.5f ? 4f * t * t * t : 1f - Pow(-2f * t + 2f, 3f) * 0.5f;

                case PixelEaseType.InQuart: return t * t * t * t;
                case PixelEaseType.OutQuart: return 1f - Pow(1f - t, 4f);
                case PixelEaseType.InOutQuart:
                    return t < 0.5f ? 8f * t * t * t * t : 1f - Pow(-2f * t + 2f, 4f) * 0.5f;

                case PixelEaseType.InExpo: return t <= 0f ? 0f : Pow(2f, 10f * t - 10f);
                case PixelEaseType.OutExpo: return t >= 1f ? 1f : 1f - Pow(2f, -10f * t);
                case PixelEaseType.InOutExpo:
                    if (t <= 0f) return 0f;
                    if (t >= 1f) return 1f;
                    return t < 0.5f
                        ? Pow(2f, 20f * t - 10f) * 0.5f
                        : (2f - Pow(2f, -20f * t + 10f)) * 0.5f;

                case PixelEaseType.InBack:
                {
                    const float c1 = 1.70158f;
                    const float c3 = c1 + 1f;
                    return c3 * t * t * t - c1 * t * t;
                }
                case PixelEaseType.OutBack:
                {
                    const float c1 = 1.70158f;
                    const float c3 = c1 + 1f;
                    float p = t - 1f;
                    return 1f + c3 * p * p * p + c1 * p * p;
                }
                case PixelEaseType.InOutBack:
                {
                    const float c1 = 1.70158f;
                    const float c2 = c1 * 1.525f;
                    return t < 0.5f
                        ? (Pow(2f * t, 2f) * ((c2 + 1f) * 2f * t - c2)) * 0.5f
                        : (Pow(2f * t - 2f, 2f) * ((c2 + 1f) * (t * 2f - 2f) + c2) + 2f) * 0.5f;
                }

                case PixelEaseType.OutBounce: return OutBounce(t);

                case PixelEaseType.OutElastic:
                {
                    if (t <= 0f) return 0f;
                    if (t >= 1f) return 1f;
                    const float c4 = (2f * PI) / 3f;
                    return Pow(2f, -10f * t) * Sin((t * 10f - 0.75f) * c4) + 1f;
                }

                case PixelEaseType.Custom:
                default:
                    return t;
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

        // Thin wrappers so this file stays self-contained and easy to read.
        private static float Sin(float v) => (float)System.Math.Sin(v);
        private static float Cos(float v) => (float)System.Math.Cos(v);
        private static float Pow(float b, float e) => (float)System.Math.Pow(b, e);
    }
}
