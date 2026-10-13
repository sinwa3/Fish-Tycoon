using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PixelInventoryDiaryUI
{
    /// <summary>
    /// Plays a frame-by-frame sprite animation on a UGUI <see cref="Image"/>.
    /// Designed for UI flipbook effects such as the diary page-turn animation.
    /// Attach to any GameObject that has an Image component, assign the frames,
    /// and call <see cref="Play"/> (or enable Play On Enable).
    /// </summary>
    [AddComponentMenu("Pixel Inventory Diary UI/UI Sprite Flipbook")]
    [RequireComponent(typeof(Image))]
    [DisallowMultipleComponent]
    public class UISpriteFlipbook : MonoBehaviour
    {
        [Header("Frames")]
        [Tooltip("Sprite frames played in order (e.g. UI_PageAnim_0 ... UI_PageAnim_19).")]
        [SerializeField] private List<Sprite> frames = new List<Sprite>();

        [Header("Playback")]
        [Tooltip("Frames per second.")]
        [Range(1f, 60f)]
        [SerializeField] private float framesPerSecond = 12f;

        [Tooltip("Loop forever instead of playing once.")]
        [SerializeField] private bool loop = false;

        [Tooltip("Play automatically when this component is enabled.")]
        [SerializeField] private bool playOnEnable = false;

        [Tooltip("Use unscaled time so the animation keeps playing while the game is paused (Time.timeScale = 0). Recommended for UI.")]
        [SerializeField] private bool useUnscaledTime = true;

        [Tooltip("When the animation stops, snap the Image back to the first frame.")]
        [SerializeField] private bool resetToFirstFrameOnStop = false;

        [Header("Events")]
        [Tooltip("Invoked when a non-looping animation reaches its last frame.")]
        [SerializeField] private UnityEvent onComplete = new UnityEvent();

        private Image image;
        private float timer;
        private int currentFrame;
        private bool playing;
        private bool reverse;

        /// <summary>Fires once a non-looping animation finishes.</summary>
        public UnityEvent OnComplete => onComplete;

        /// <summary>True while the animation is actively advancing frames.</summary>
        public bool IsPlaying => playing;

        /// <summary>Index of the frame currently shown on the Image.</summary>
        public int CurrentFrame => currentFrame;

        /// <summary>Number of assigned frames.</summary>
        public int FrameCount => frames.Count;

        /// <summary>Frames per second. Clamped to a minimum of 1.</summary>
        public float FramesPerSecond
        {
            get => framesPerSecond;
            set => framesPerSecond = Mathf.Max(1f, value);
        }

        /// <summary>Whether the animation loops.</summary>
        public bool Loop
        {
            get => loop;
            set => loop = value;
        }

        private void Awake()
        {
            image = GetComponent<Image>();
        }

        private void OnEnable()
        {
            if (playOnEnable)
                Play();
        }

        private void OnDisable()
        {
            // Stop silently so a re-enable starts clean without firing OnComplete.
            playing = false;
        }

        private void Update()
        {
            if (!playing || frames.Count == 0)
                return;

            timer += (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime) * framesPerSecond;

            while (timer >= 1f)
            {
                timer -= 1f;
                if (!AdvanceFrame())
                    return; // finished (and Update already stopped playback)
            }
        }

        /// <summary>Plays the animation from the first frame, forward.</summary>
        public void Play()
        {
            Play(false);
        }

        /// <summary>Plays the animation from the last frame, backward (e.g. turning a page back).</summary>
        public void PlayReverse()
        {
            Play(true);
        }

        /// <summary>Plays the animation from the start in the given direction.</summary>
        /// <param name="playReversed">When true, plays from last frame to first.</param>
        public void Play(bool playReversed)
        {
            if (frames.Count == 0)
            {
                Debug.LogWarning($"{nameof(UISpriteFlipbook)} on '{name}' has no frames assigned.", this);
                return;
            }

            reverse = playReversed;
            currentFrame = reverse ? frames.Count - 1 : 0;
            timer = 0f;
            playing = true;
            ApplyFrame();
        }

        /// <summary>Stops playback. Optionally resets to the first frame (per the inspector setting).</summary>
        public void Stop()
        {
            playing = false;
            if (resetToFirstFrameOnStop)
            {
                currentFrame = 0;
                ApplyFrame();
            }
        }

        /// <summary>Pauses on the current frame. Resume with <see cref="Resume"/>.</summary>
        public void Pause()
        {
            playing = false;
        }

        /// <summary>Resumes a paused animation from the current frame.</summary>
        public void Resume()
        {
            if (frames.Count > 0)
                playing = true;
        }

        /// <summary>Jumps to a specific frame and displays it without starting playback.</summary>
        public void GoToFrame(int index)
        {
            if (frames.Count == 0)
                return;

            currentFrame = Mathf.Clamp(index, 0, frames.Count - 1);
            ApplyFrame();
        }

        /// <summary>Replaces the frame list at runtime (e.g. to reskin the animation).</summary>
        public void SetFrames(IEnumerable<Sprite> newFrames)
        {
            frames.Clear();
            if (newFrames != null)
                frames.AddRange(newFrames);
        }

        /// <summary>Advances one frame. Returns false and stops playback when a non-looping run ends.</summary>
        private bool AdvanceFrame()
        {
            int next = currentFrame + (reverse ? -1 : 1);
            bool reachedEnd = reverse ? next < 0 : next >= frames.Count;

            if (reachedEnd)
            {
                if (loop)
                {
                    currentFrame = reverse ? frames.Count - 1 : 0;
                    ApplyFrame();
                    return true;
                }

                playing = false;
                if (resetToFirstFrameOnStop)
                {
                    currentFrame = 0;
                    ApplyFrame();
                }
                onComplete.Invoke();
                return false;
            }

            currentFrame = next;
            ApplyFrame();
            return true;
        }

        private void ApplyFrame()
        {
            if (image == null)
                image = GetComponent<Image>();

            Sprite sprite = frames[currentFrame];
            if (sprite != null)
                image.sprite = sprite;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            framesPerSecond = Mathf.Max(1f, framesPerSecond);
        }
#endif
    }
}
