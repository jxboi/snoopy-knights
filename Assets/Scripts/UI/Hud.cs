using SnoopyKnights.Core;
using UnityEngine;

namespace SnoopyKnights.UI
{
    /// <summary>Root of the mobile HUD. Composes the individual UI pieces.</summary>
    public sealed class Hud : MonoBehaviour
    {
        public ResourceBar ResourceBar { get; private set; }
        public BuildMenu BuildMenu { get; private set; }
        public BuildingPanel BuildingPanel { get; private set; }
        public UnitPanel UnitPanel { get; private set; }
        public WavePanel WavePanel { get; private set; }
        public ObjectivesPanel ObjectivesPanel { get; private set; }
        public GameOverScreen GameOverScreen { get; private set; }
        public PauseMenu PauseMenu { get; private set; }
        public SpeedButton SpeedButton { get; private set; }

        public static Hud Create(Game game)
        {
            var canvas = UiFactory.CreateCanvas("HUD");
            var hud = canvas.gameObject.AddComponent<Hud>();

            var safe = UiFactory.Group(canvas.transform, "SafeArea");
            UiFactory.Stretch(safe);
            safe.gameObject.AddComponent<SafeArea>();

            hud.ResourceBar = ResourceBar.Create(safe, game);
            hud.BuildMenu = BuildMenu.Create(safe, game);
            hud.BuildingPanel = BuildingPanel.Create(safe, game);
            hud.UnitPanel = UnitPanel.Create(safe, game);
            hud.WavePanel = WavePanel.Create(safe, game.Waves);
            hud.ObjectivesPanel = ObjectivesPanel.Create(safe, game.Mission);
            hud.SpeedButton = SpeedButton.Create(safe); // before PauseMenu: its overlay must cover this
            hud.PauseMenu = PauseMenu.Create(safe, game);
            hud.GameOverScreen = GameOverScreen.Create(safe, game);
            return hud;
        }
    }
}
