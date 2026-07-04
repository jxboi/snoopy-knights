using SnoopyKnights.Core;
using SnoopyKnights.Mission;
using SnoopyKnights.Save;
using UnityEngine;
using UnityEngine.UI;

namespace SnoopyKnights.UI
{
    /// <summary>Pause button (top-right) and the pause overlay: resume, save, load, restart.</summary>
    public sealed class PauseMenu : MonoBehaviour
    {
        Game game;
        RectTransform overlay;
        Button saveBtn, loadBtn;
        Text feedback;
        float feedbackT;

        public static PauseMenu Create(Transform parent, Game game)
        {
            var root = UiFactory.Group(parent, "PauseMenu");
            UiFactory.Stretch(root);
            var menu = root.gameObject.AddComponent<PauseMenu>();
            menu.game = game;
            menu.BuildUi(root);
            return menu;
        }

        void BuildUi(RectTransform root)
        {
            var pauseBtn = UiFactory.Button(root, "PauseBtn", "II", 38,
                new Color(0.25f, 0.2f, 0.12f, 0.9f), Open);
            UiFactory.Place((RectTransform)pauseBtn.transform, new Vector2(1f, 1f),
                new Vector2(-12f, -6f), new Vector2(104f, 64f));

            overlay = UiFactory.Panel(root, "Overlay", new Color(0f, 0f, 0f, 0.72f));
            UiFactory.Stretch(overlay);

            var title = UiFactory.Label(overlay, "Title", "Paused", 80, Color.white, TextAnchor.MiddleCenter);
            UiFactory.Place((RectTransform)title.transform, new Vector2(0.5f, 0.5f),
                new Vector2(0f, 260f), new Vector2(600f, 100f));

            var btnColor = new Color(0.3f, 0.23f, 0.13f, 1f);
            var resume = UiFactory.Button(overlay, "Resume", "Resume", 40, btnColor, Resume);
            saveBtn = UiFactory.Button(overlay, "Save", "Save game", 40, btnColor, SaveGame);
            loadBtn = UiFactory.Button(overlay, "Load", "Load game", 40, btnColor, LoadGame);
            var restart = UiFactory.Button(overlay, "Restart", "Restart mission", 40,
                new Color(0.45f, 0.18f, 0.13f, 1f), GameOverScreen.Restart);

            Button[] column = { resume, saveBtn, loadBtn, restart };
            for (int i = 0; i < column.Length; i++)
                UiFactory.Place((RectTransform)column[i].transform, new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 120f - i * 130f), new Vector2(420f, 112f));

            feedback = UiFactory.Label(overlay, "Feedback", "", 32,
                new Color(0.7f, 0.95f, 0.7f), TextAnchor.MiddleCenter);
            UiFactory.Place((RectTransform)feedback.transform, new Vector2(0.5f, 0.5f),
                new Vector2(0f, -420f), new Vector2(700f, 50f));

            overlay.gameObject.SetActive(false);
        }

        void Open()
        {
            if (game.Mission.State != MissionState.Playing) return; // end screen owns the UI now
            overlay.gameObject.SetActive(true);
            overlay.SetAsLastSibling();
            Time.timeScale = 0f;
            saveBtn.interactable = true;
            loadBtn.interactable = SaveSystem.HasSave;
            feedback.text = "";
        }

        void Resume()
        {
            overlay.gameObject.SetActive(false);
            if (game.Mission.State == MissionState.Playing)
                Time.timeScale = 1f;
        }

        void SaveGame()
        {
            SaveSystem.Save(game);
            loadBtn.interactable = true;
            ShowFeedback("Game saved.");
        }

        void LoadGame()
        {
            if (!SaveSystem.LoadAndRestart())
                ShowFeedback("No save found.");
        }

        void ShowFeedback(string msg)
        {
            feedback.text = msg;
            feedbackT = 2f;
        }

        void Update()
        {
            if (feedbackT <= 0f) return;
            feedbackT -= Time.unscaledDeltaTime;
            if (feedbackT <= 0f) feedback.text = "";
        }
    }
}
