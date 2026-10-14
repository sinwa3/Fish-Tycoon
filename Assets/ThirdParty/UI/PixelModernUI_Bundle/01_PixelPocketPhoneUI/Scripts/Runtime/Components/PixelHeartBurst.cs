using UnityEngine;
using UnityEngine.UI;

namespace PixelPocketPhoneUI.Tweening
{
    /// <summary>
    /// Spawns a little burst of hearts that float up and fade out — the classic
    /// "like" feedback. Add it to the heart/checkmark object inside a toggle.
    ///
    /// If a Toggle is found on this object or a parent, it auto-fires when the
    /// toggle turns ON. You can also call <see cref="Burst"/> from code, a Button,
    /// or a UnityEvent.
    ///
    /// Hearts are spawned on the root Canvas so they are NOT clipped by a
    /// ScrollRect's mask.
    /// </summary>
    [AddComponentMenu("Pixel Pocket UI/Tween/Pixel Heart Burst")]
    [RequireComponent(typeof(RectTransform))]
    public sealed class PixelHeartBurst : MonoBehaviour
    {
        [Header("Heart")]
        [Tooltip("Sprite used for each floating heart. Defaults to this object's Image sprite if left empty.")]
        public Sprite heartSprite;
        [Tooltip("Base tint, used when Colors is empty (alpha is animated to 0).")]
        public Color color = new Color(1f, 0.30f, 0.45f, 1f);
        [Tooltip("Optional gradient colors. With 2+ entries each heart gets a RANDOM blend between them " +
                 "(2 colors = a random mix somewhere between the two). Overrides Color when set.")]
        public Color[] colors;
        [Tooltip("Random per-heart color jitter (hue/saturation/value). 0 = identical, 1 = very varied.")]
        [Range(0f, 1f)] public float colorVariation = 0.15f;
        [Tooltip("Random heart size range (pixels).")]
        public float minSize = 18f;
        public float maxSize = 30f;

        [Header("Burst")]
        [Min(1)] public int count = 6;
        [Tooltip("How far up the hearts travel (pixels).")]
        public float riseDistance = 130f;
        [Tooltip("Horizontal spread of the spawn fan (pixels).")]
        public float spread = 44f;
        [Tooltip("Vertical size of the spawn area (pixels). Hearts start scattered within this height.")]
        public float verticalSpread = 12f;
        [Tooltip("Sideways wiggle amplitude while rising (pixels).")]
        public float sway = 14f;
        [Tooltip("Lifetime of each heart (seconds). Slight randomization is added.")]
        public float duration = 0.9f;

        [Header("Spawn")]
        [Tooltip("Where hearts are created. Leave EMPTY to spawn on this object itself " +
                 "(always visible, but may be clipped by a ScrollRect mask). " +
                 "Set to a top-level Canvas/object to avoid clipping.")]
        public RectTransform spawnParent;

        [Header("Trigger")]
        [Tooltip("Auto-fire when a Toggle on this object/parent turns ON.")]
        public bool bindToToggle = true;
        [Tooltip("If bound to a toggle, only burst when it switches to ON (not OFF).")]
        public bool onlyWhenTurnedOn = true;
        [Tooltip("Burst once automatically when this object becomes enabled (no toggle/button needed).")]
        public bool playOnEnable = false;
        [Tooltip("Keep bursting on a timer while enabled.")]
        public bool repeat = false;
        [Tooltip("Seconds between bursts when Repeat is on.")]
        [Min(0.1f)] public float repeatInterval = 1.5f;

        private RectTransform _self;
        private Toggle _toggle;

        private void Awake()
        {
            _self = (RectTransform)transform;
            if (heartSprite == null)
            {
                var img = GetComponent<Image>();
                if (img != null) heartSprite = img.sprite;
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
                    // Remove first so re-enabling never stacks duplicate listeners.
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

        /// <summary>Emit one burst of hearts right now.</summary>
        [ContextMenu("Burst")]
        public void Burst()
        {
            if (_self == null) _self = (RectTransform)transform;
            if (heartSprite == null)
            {
                var img = GetComponent<Image>();
                if (img != null) heartSprite = img.sprite;
            }
            if (heartSprite == null)
            {
                Debug.LogWarning("[PixelHeartBurst] No heart sprite assigned.", this);
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
                parent = _self;            // spawn directly on the checkmark — no conversion, always visible
                origin = Vector2.zero;
            }

            for (int i = 0; i < count; i++)
                SpawnHeart(parent, origin, i);
        }

        private void SpawnHeart(RectTransform root, Vector2 origin, int index)
        {
            var go = new GameObject("HeartParticle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.hideFlags = HideFlags.HideInHierarchy; // temp effect — keep it out of the hierarchy
            var rt = (RectTransform)go.transform;
            rt.SetParent(root, false);
            // Anchor & pivot to center so anchoredPosition matches the
            // center-based coordinate returned by ScreenPointToLocalPointInRectangle.
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);

            var img = go.GetComponent<Image>();
            img.sprite = heartSprite;
            img.raycastTarget = false;
            img.preserveAspect = true;

            // Per-heart randomization (UnityEngine.Random is fine at runtime).
            float size = Random.Range(minSize, maxSize);
            float life = duration * Random.Range(0.85f, 1.15f);
            float swayAmp = Random.Range(-sway, sway);
            float swayFreq = Random.Range(1.5f, 2.5f);
            float spin = Random.Range(-12f, 12f);

            // Fan the start points out a little, then rise with a bit of variance.
            float fan = count > 1 ? (index / (float)(count - 1) - 0.5f) * 2f : 0f;
            float vHalf = verticalSpread * 0.5f;
            Vector2 start = origin + new Vector2(fan * spread * 0.5f, Random.Range(-vHalf, vHalf));
            Vector2 end = start + new Vector2(fan * spread, riseDistance * Random.Range(0.85f, 1.2f));

            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = start;
            rt.localScale = Vector3.zero;

            Color baseColor = VaryColor(PickColor());

            PixelTween.To(t =>
            {
                if (rt == null || img == null) return;

                // Vertical/horizontal travel with an ease-out, plus a sine wiggle.
                float move = PixelEase.Evaluate(PixelEaseType.OutCubic, t);
                Vector2 p = Vector2.LerpUnclamped(start, end, move);
                p.x += Mathf.Sin(t * Mathf.PI * 2f * swayFreq) * swayAmp;
                rt.anchoredPosition = p;

                // Pop in fast, then drift to a slightly smaller size.
                float s = t < 0.18f
                    ? Mathf.Lerp(0f, 1.1f, t / 0.18f)
                    : Mathf.Lerp(1.1f, 0.85f, (t - 0.18f) / 0.82f);
                rt.localScale = new Vector3(s, s, 1f);
                rt.localEulerAngles = new Vector3(0f, 0f, spin * t);

                // Hold opacity, then fade out over the last 40%.
                float a = t < 0.6f ? 1f : 1f - (t - 0.6f) / 0.4f;
                Color c = baseColor; c.a *= Mathf.Clamp01(a);
                img.color = c;
            }, life)
            .SetEase(PixelEaseType.Linear)
            .OnComplete(() => { if (go != null) Destroy(go); });
        }

        // --- helpers ---

        /// <summary>
        /// Pick a base color by randomly blending across the gradient colors.
        /// 2 colors => a random mix between them; 3+ => a random point along the ramp.
        /// </summary>
        private Color PickColor()
        {
            Color c;
            if (colors == null || colors.Length == 0) c = color;
            else if (colors.Length == 1) c = colors[0];
            else
            {
                float r = Random.value * (colors.Length - 1);   // 0 .. (n-1)
                int i = Mathf.FloorToInt(r);
                c = (i >= colors.Length - 1)
                    ? colors[colors.Length - 1]
                    : Color.Lerp(colors[i], colors[i + 1], r - i);
            }

            // Start fully opaque — the fade is driven by the animation. This also
            // guards against Unity defaulting new Color-array entries to alpha 0.
            c.a = 1f;
            return c;
        }

        /// <summary>Apply a small random HSV jitter so hearts aren't all identical.</summary>
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
