using UnityEngine;

namespace SnoopyKnights.Rendering
{
    /// <summary>
    /// Plays a frame-based sprite animation on a SpriteRenderer — the KaM-style
    /// walk/work/attack cycles. Frames advance on Time.deltaTime, so animation
    /// pauses when the game pauses and speeds up under fast-forward, matching the
    /// simulation. Attach to the same GameObject as the SpriteRenderer.
    /// Everything degrades to nothing if no frames are supplied.
    /// </summary>
    public sealed class SpriteAnimator : MonoBehaviour
    {
        SpriteRenderer sr;
        Sprite[] frames;
        float fps;
        bool loop;
        float t;
        int frame;
        bool done;
        System.Action onComplete;

        /// <summary>Id of the clip currently playing, so callers can avoid restarts.</summary>
        public string CurrentClip { get; private set; }

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// Starts a clip. A looping clip with the same id is a no-op (won't
        /// restart). Non-looping clips hold the last frame and fire onComplete.
        /// </summary>
        public void Play(string clipId, Sprite[] clipFrames, float clipFps,
                         bool clipLoop, System.Action complete = null)
        {
            if (clipFrames == null || clipFrames.Length == 0) return;
            if (clipLoop && clipId == CurrentClip && !done) return;

            CurrentClip = clipId;
            frames = clipFrames;
            fps = Mathf.Max(0.01f, clipFps);
            loop = clipLoop;
            onComplete = complete;
            t = 0f;
            frame = 0;
            done = false;
            if (sr == null) sr = GetComponent<SpriteRenderer>();
            sr.sprite = frames[0];
        }

        void Update()
        {
            if (frames == null || done) return;
            if (frames.Length <= 1) return;

            t += Time.deltaTime * fps;
            while (t >= 1f)
            {
                t -= 1f;
                frame++;
                if (frame >= frames.Length)
                {
                    if (loop)
                    {
                        frame = 0;
                    }
                    else
                    {
                        frame = frames.Length - 1;
                        done = true;
                        var cb = onComplete;
                        onComplete = null;
                        cb?.Invoke();
                        break;
                    }
                }
            }
            sr.sprite = frames[frame];
        }
    }
}
