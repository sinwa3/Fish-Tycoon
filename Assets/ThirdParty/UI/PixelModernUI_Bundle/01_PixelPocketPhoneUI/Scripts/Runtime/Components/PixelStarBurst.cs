using UnityEngine;
using UnityEngine.UI;

namespace PixelPocketPhoneUI.Tweening
{
    /// <summary>
    /// Fireworks-style burst: stars shoot out from the center in all directions,
    /// spin, optionally fall with gravity, then fade out. Great for rewards,
    /// level-ups, celebrations.
    ///
    /// Add it to the object you want the burst to originate from. Fire it via a
    /// Toggle, automatically (Play On Enable / Repeat), a Button's OnClick
    /// (call <see cref="Burst"/>), or from code.
    /// </summary>
    [AddComponentMenu("Pixel Pocket UI/Tween/Pixel Star Burst")]
    [RequireComponent(typeof(RectTransform))]
    public sealed class PixelStarBurst : MonoBehaviour
    {
        [Header("Star")]
        [Tooltip("Sprite for each star. Defaults to this object's Image sprite if left empty.")]
        public Sprite starSprite;
        [Tooltip("Base tint, used when Colors is empty (alpha is animated).")]
        public Color color = new Color(1f, 0.9f, 0.3f, 1f);
        [Tooltip("Optional gradient colors. With 2+ entries each star gets a random blend between them.")]
        public Color[] colors;
        [Tooltip("Random per-star color jitter (hue/saturation/value). 0 = identical, 1 = very varied.")]
        [Range(0f, 1f)] public float colorVariation = 0.12f;
        [Tooltip("Random star size range (pixels).")]
        public float minSize = 18f;
        public float maxSize = 34f;

        [Header("Burst")]
        [Min(1)] public int count = 12;
        [Tooltip("How far the stars travel outward from the center (pixels).")]
        public float radius = 150f;
        [Tooltip("Downward pull applied over time (pixels). 0 = pure radial spread, higher = fountain/fall.")]
        public float gravity = 70f;
        [Tooltip("Max spin over the lifetime (degrees, randomized per star).")]
        public float spin = 200f;
        [Tooltip("Lifetime of each star (seconds). Slight randomization is added.")]
        public float duration = 0.9f;

        [Header("Spawn")]
        [Tooltip("Where stars are created. Leave EMPTY to spawn on this object itself " +
                 "(always visible, but may be clipped by a ScrollRect mask). " +
                 "Set to a top-level Canvas/object to avoid clipping.")]
        public RectTransform spawnParent;

        [Header("Trigger")]
        [Tooltip("Auto-fire when a Toggle on this object/parent turns ON.")]
        public bool bindToToggle = false;
        [Tooltip("If bound to a toggle, only burst when it switches to ON (not OFF).")]
        public bool onlyWhenTurnedOn = true;
        [Tooltip("Burst once automatically when this object becomes enabled.")]
        public bool playOnEnable = true;
        [Tooltip("Keep bursting on a timer while enabled.")]
        public bool repeat = false;
        [Tooltip("Seconds between bursts when Repeat is on.")]
        [Min(0.1f)] public float repeatInterval = 1.5f;

        private RectTransform _self;
        private Toggle _toggle;

        private void Awake()
        {
            _self = (RectTransform)transform;
            if (starSprite == null)
            {
                var img = GetComponent<Image>();
                if (img != null) starSprite = img.sprite;
            }
        }

        private void OnEnable()
        {
            if (bindToToggle)
            {
                _toggle = GetComponent<Toggle>();
                if (_toggle == null) _toggle = GetComponentInParent<Toggle>();
                if (_toggle != null)
                {
                    _toggle.onValueChanged.RemoveListener(OnToggleChanged);
                    _toggle.onValueChanged.AddListener(OnToggleChanged);
                }
            }

            if (playOnEnable) Burst();
            if (repeat) InvokeRepeating(nameof(Burst), repeatInterval, repeatInterval);
        }

        private void OnDisable()
        {
            if (_toggle != null) _toggle.onValueChanged.RemoveListener(OnToggleChanged);
            CancelInvoke(nameof(Burst));
        }

        private void OnToggleChanged(bool isOn)
        {
            if (onlyWhenTurnedOn && !isOn) return;
            Burst();
        }

        /// <summary>Emit one firework burst right now.</summary>
        [ContextMenu("Burst")]
        public void Burst()
        {
            if (_self == null) _self = (RectTransform)transform;
            if (starSprite == null)
            {
                var img = GetComponent<Image>();
                if (img != null) starSprite = img.sprite;
            }
            if (starSprite == null)
            {
                Debug.LogWarning("[PixelStarBurst] No star sprite assigned.", this);
                return;
            }

            RectTransform parent;
            Vector2 origin;
            if (spawnParent != null)
            {
                parent = spawnParent;
                origin = WorldToLocal(parent, _self.position);
            }
            else
            {
                parent = _self;
                origin = Vector2.zero;
            }

            for (int i = 0; i < count; i++)
                SpawnStar(parent, origin, i);
        }

        private void SpawnStar(RectTransform root, Vector2 origin, int index)
        {
            var go = new GameObject("StarParticle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.hideFlags = HideFlags.HideInHierarchy; // temp effect — keep it out of the hierarchy
            var rt = (RectTransform)go.transform;
            rt.SetParent(root, false);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);

            var img = go.GetComponent<Image>();
            img.sprite = starSprite;
            img.raycastTarget = false;
            img.preserveAspect = true;

            // Even angular distribution + a little jitter so it reads as a burst.
            float baseAngle = (index / (float)count) * Mathf.PI * 2f;
            float angle = baseAngle + Random.Range(-0.25f, 0.25f);
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            float dist = radius * Random.Range(0.7f, 1.2f);
            float life = duration * Random.Range(0.85f, 1.15f);
            float spinAmt = Random.Range(-spin, spin);
            float size = Random.Range(minSize, maxSize);

            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = origin;
            rt.localScale = Vector3.zero;

            Color baseColor = VaryColor(PickColor());

            PixelTween.To(t =>
            {
                if (rt == null || img == null) return;

                // Shoot outward fast then decelerate, with a parabolic gravity drop.
                float spread = PixelEase.Evaluate(PixelEaseType.OutCubic, t);
                Vector2 p = origin + dir * (dist * spread);
                p.y -= gravity * t * t;
                rt.anchoredPosition = p;

                // Pop in, then ease down slightly.
                float s = t < 0.15f
                    ? Mathf.Lerp(0f, 1.15f, t / 0.15f)
                    : Mathf.Lerp(1.15f, 0.9f, (t - 0.15f) / 0.85f);
                rt.localScale = new Vector3(s, s, 1f);
                rt.localEulerAngles = new Vector3(0f, 0f, spinAmt * t);

                // Hold, then fade over the last 45%.
                float a = t < 0.55f ? 1f : 1f - (t - 0.55f) / 0.45f;
                Color c = baseColor; c.a *= Mathf.Clamp01(a);
                img.color = c;
            }, life)
            .SetEase(PixelEaseType.Linear)
            .OnComplete(() => { if (go != null) Destroy(go); });
        }

        // --- helpers (shared shape with PixelHeartBurst) ---

        private Color PickColor()
        {
            Color c;
            if (colors == null || colors.Length == 0) c = color;
            else if (colors.Length == 1) c = colors[0];
            else
            {
                float r = Random.value * (colors.Length - 1);
                int i = Mathf.FloorToInt(r);
                c = (i >= colors.Length - 1)
                    ? colors[colors.Length - 1]
                    : Color.Lerp(colors[i], colors[i + 1], r - i);
            }
            c.a = 1f; // start opaque; alpha is driven by the fade
            return c;
        }

        private Color VaryColor(Color c)
        {
            if (colorVariation <= 0f) return c;
            Color.RGBToHSV(c, out float h, out float s, out float v);
            h = Mathf.Repeat(h + Random.Range(-colorVariation, colorVariation) * 0.5f, 1f);
            s = Mathf.Clamp01(s + Random.Range(-colorVariation, colorVariation) * 0.3f);
            v = Mathf.Clamp01(v + Random.Range(-colorVariation, colorVariation) * 0.3f);
            Color result = Color.HSVToRGB(h, s, v);
            result.a = c.a;
            return result;
        }

        private Vector2 WorldToLocal(RectTransform target, Vector3 worldPos)
        {
            var canvas = target.GetComponentInParent<Canvas>();
            Camera cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                ? canvas.worldCamera
                : null;
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(target, screen, cam, out var local);
            return local;
        }
    }
}
