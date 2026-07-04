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

        public static Hud Create(Game game)
        {
            var canvas = UiFactory.CreateCanvas("HUD");
            var hud = canvas.gameObject.AddComponent<Hud>();

            var safe = UiFactory.Group(canvas.transform, "SafeArea");
            UiFactory.Stretch(safe);
            safe.gameObject.AddComponent<SafeArea>();

            hud.ResourceBar = ResourceBar.Create(safe, game.Stock);
            hud.BuildMenu = BuildMenu.Create(safe, game);
            hud.BuildingPanel = BuildingPanel.Create(safe, game);
            return hud;
        }
    }
}
