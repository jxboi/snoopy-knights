using SnoopyKnights.Mission;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SnoopyKnights.UI
{
    /// <summary>Full-screen victory/defeat overlay with a restart button.</summary>
    public sealed class GameOverScreen : MonoBehaviour
    {
        public static GameOverScreen Create(Transform parent, MissionController mission)
        {
            var rt = UiFactory.Panel(parent, "GameOver", new Color(0f, 0f, 0f, 0.78f));
            UiFactory.Stretch(rt);

            var screen = rt.gameObject.AddComponent<GameOverScreen>();

            var title = UiFactory.Label(rt, "Title", "", 96, Color.white, TextAnchor.MiddleCenter);
            UiFactory.Place((RectTransform)title.transform, new Vector2(0.5f, 0.5f),
                new Vector2(0f, 130f), new Vector2(1200f, 130f));

            var subtitle = UiFactory.Label(rt, "Subtitle", "", 36,
                new Color(0.9f, 0.9f, 0.85f), TextAnchor.MiddleCenter);
            UiFactory.Place((RectTransform)subtitle.transform, new Vector2(0.5f, 0.5f),
                new Vector2(0f, 30f), new Vector2(1400f, 60f));

            var restart = UiFactory.Button(rt, "Restart", "Restart", 44,
                new Color(0.3f, 0.23f, 0.13f, 1f), Restart);
            UiFactory.Place((RectTransform)restart.transform, new Vector2(0.5f, 0.5f),
                new Vector2(0f, -110f), new Vector2(340f, 130f));

            rt.gameObject.SetActive(false);

            mission.GameEnded += won =>
            {
                title.text = won ? "Victory!" : "Defeat";
                title.color = won ? new Color(1f, 0.85f, 0.3f) : new Color(0.95f, 0.4f, 0.35f);
                subtitle.text = mission.EndReason;
                rt.gameObject.SetActive(true);
                rt.SetAsLastSibling();
            };
            return screen;
        }

        public static void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
