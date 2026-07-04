using SnoopyKnights.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SnoopyKnights.UI
{
    /// <summary>Full-screen victory/defeat overlay: mission stats and a restart button.</summary>
    public sealed class GameOverScreen : MonoBehaviour
    {
        CanvasGroup fade;
        RectTransform titleRt;

        public static GameOverScreen Create(Transform parent, Game game)
        {
            var rt = UiFactory.Panel(parent, "GameOver", new Color(0f, 0f, 0f, 0.78f));
            UiFactory.Stretch(rt);

            var screen = rt.gameObject.AddComponent<GameOverScreen>();
            screen.fade = rt.gameObject.AddComponent<CanvasGroup>();

            var title = UiFactory.Label(rt, "Title", "", 96, Color.white, TextAnchor.MiddleCenter);
            UiFactory.Place((RectTransform)title.transform, new Vector2(0.5f, 0.5f),
                new Vector2(0f, 170f), new Vector2(1200f, 130f));
            screen.titleRt = (RectTransform)title.transform;

            var subtitle = UiFactory.Label(rt, "Subtitle", "", 36,
                new Color(0.9f, 0.9f, 0.85f), TextAnchor.MiddleCenter);
            UiFactory.Place((RectTransform)subtitle.transform, new Vector2(0.5f, 0.5f),
                new Vector2(0f, 70f), new Vector2(1400f, 60f));

            var stats = UiFactory.Label(rt, "Stats", "", 30,
                new Color(0.78f, 0.75f, 0.65f), TextAnchor.MiddleCenter);
            UiFactory.Place((RectTransform)stats.transform, new Vector2(0.5f, 0.5f),
                new Vector2(0f, -20f), new Vector2(1700f, 44f));

            var restart = UiFactory.Button(rt, "Restart", "Restart", 44,
                new Color(0.3f, 0.23f, 0.13f, 1f), Restart);
            UiFactory.Place((RectTransform)restart.transform, new Vector2(0.5f, 0.5f),
                new Vector2(0f, -170f), new Vector2(340f, 130f));

            rt.gameObject.SetActive(false);

            game.Mission.GameEnded += won =>
            {
                title.text = won ? "Victory!" : "Defeat";
                title.color = won ? new Color(1f, 0.85f, 0.3f) : new Color(0.95f, 0.4f, 0.35f);
                subtitle.text = game.Mission.EndReason;
                stats.text = StatsLine(game);
                screen.fade.alpha = 0f;
                rt.gameObject.SetActive(true);
                rt.SetAsLastSibling();
            };
            return screen;
        }

        static string StatsLine(Game game)
        {
            int sec = Mathf.RoundToInt(game.Stats.PlaySeconds);
            return $"{sec / 60}:{sec % 60:00} played  ·  waves {game.Waves.WavesCleared}/{game.Waves.TotalWaves}" +
                   $"  ·  {game.Stats.EnemiesSlain} enemies slain  ·  {game.Stats.VillagersLost} villagers lost" +
                   $"  ·  {game.Stats.BuildingsLost} buildings lost";
        }

        // The sim is frozen on the end screen, so animate on unscaled time.
        void Update()
        {
            if (fade.alpha >= 1f) return;
            fade.alpha = Mathf.MoveTowards(fade.alpha, 1f, Time.unscaledDeltaTime * 2.5f);
            titleRt.localScale = Vector3.one * Mathf.Lerp(1.25f, 1f, fade.alpha);
        }

        public static void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
