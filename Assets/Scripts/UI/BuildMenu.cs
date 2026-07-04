using System.Collections.Generic;
using SnoopyKnights.Buildings;
using SnoopyKnights.Core;
using SnoopyKnights.Rendering;
using SnoopyKnights.Res;
using UnityEngine;
using UnityEngine.UI;

namespace SnoopyKnights.UI
{
    /// <summary>
    /// The build flow: a big Build button opens a card strip; picking a card
    /// enters placement (confirm/cancel) or road painting (done).
    /// </summary>
    public sealed class BuildMenu : MonoBehaviour
    {
        static readonly Color BtnBrown = new Color(0.3f, 0.23f, 0.13f, 0.95f);
        static readonly Color BtnGreen = new Color(0.2f, 0.5f, 0.2f, 0.95f);
        static readonly Color BtnRed = new Color(0.55f, 0.18f, 0.15f, 0.95f);
        static readonly Color CardBg = new Color(0.22f, 0.18f, 0.12f, 0.95f);

        Game game;
        RectTransform strip;
        Button buildBtn, doneBtn, confirmBtn, cancelBtn;
        readonly List<(BuildingDef def, Button btn)> cards = new List<(BuildingDef, Button)>();

        BuildPlacementMode placement;
        RoadPaintMode roadMode;

        public bool IsBusy => placement != null || roadMode != null;

        public static BuildMenu Create(Transform parent, Game game)
        {
            var root = UiFactory.Group(parent, "BuildMenu");
            UiFactory.Stretch(root);
            var menu = root.gameObject.AddComponent<BuildMenu>();
            menu.game = game;
            menu.BuildUi(root);
            game.Stock.Changed += menu.RefreshCards;
            return menu;
        }

        void BuildUi(RectTransform root)
        {
            buildBtn = UiFactory.Button(root, "BuildBtn", "Build", 46, BtnBrown, ToggleStrip);
            UiFactory.Place((RectTransform)buildBtn.transform, new Vector2(1f, 0f),
                new Vector2(-30f, 30f), new Vector2(260f, 120f));

            doneBtn = UiFactory.Button(root, "DoneBtn", "Done", 46, BtnGreen, ExitRoadMode);
            UiFactory.Place((RectTransform)doneBtn.transform, new Vector2(1f, 0f),
                new Vector2(-30f, 30f), new Vector2(260f, 120f));
            doneBtn.gameObject.SetActive(false);

            confirmBtn = UiFactory.Button(root, "ConfirmBtn", "Place", 44, BtnGreen, ConfirmPlacement);
            UiFactory.Place((RectTransform)confirmBtn.transform, new Vector2(0.5f, 0f),
                new Vector2(-115f, 30f), new Vector2(220f, 120f));
            confirmBtn.gameObject.SetActive(false);

            cancelBtn = UiFactory.Button(root, "CancelBtn", "Cancel", 44, BtnRed, CancelPlacement);
            UiFactory.Place((RectTransform)cancelBtn.transform, new Vector2(0.5f, 0f),
                new Vector2(115f, 30f), new Vector2(220f, 120f));
            cancelBtn.gameObject.SetActive(false);

            BuildStrip(root);
        }

        void BuildStrip(RectTransform root)
        {
            var buildable = new List<BuildingDef>();
            foreach (var def in BuildingDefs.All)
                if (def.Type != BuildingType.TownCenter)
                    buildable.Add(def);

            int slots = buildable.Count + 1; // + road card
            float cardW = 172f, cardH = 236f, gap = 10f;
            float totalW = slots * (cardW + gap) + gap;

            strip = UiFactory.Panel(root, "BuildStrip", new Color(0f, 0f, 0f, 0.55f));
            UiFactory.Place(strip, new Vector2(0.5f, 0f), new Vector2(0f, 170f),
                new Vector2(totalW, cardH + 20f));
            strip.gameObject.SetActive(false);

            MakeCard(0, cardW, cardH, gap, totalW, "Road",
                $"{GameConfig.RoadStoneCost}s /tile", SpriteFactory.Square,
                new Color(0.62f, 0.51f, 0.34f), StartRoadMode, null);

            for (int i = 0; i < buildable.Count; i++)
            {
                var def = buildable[i];
                var btn = MakeCard(i + 1, cardW, cardH, gap, totalW, def.Name,
                    ResourceInfo.CostString(def.Cost), IconSprite(def.Icon), def.IconColor,
                    () => StartPlacement(def), def);
                cards.Add((def, btn));
            }
            RefreshCards();
        }

        Button MakeCard(int index, float w, float h, float gap, float totalW,
            string title, string cost, Sprite icon, Color iconColor,
            System.Action onClick, BuildingDef def)
        {
            var btn = UiFactory.Button(strip, $"Card {title}", "", 1, CardBg, onClick);
            var rt = (RectTransform)btn.transform;
            float x = -totalW * 0.5f + gap + w * 0.5f + index * (w + gap);
            UiFactory.Place(rt, new Vector2(0.5f, 0.5f), new Vector2(x, 0f), new Vector2(w, h));

            var img = UiFactory.Icon(rt, "Icon", icon, iconColor);
            UiFactory.Place((RectTransform)img.transform, new Vector2(0.5f, 1f),
                new Vector2(0f, -14f), new Vector2(76f, 76f));

            var name = UiFactory.Label(rt, "Name", title, 28, Color.white, TextAnchor.MiddleCenter);
            UiFactory.Place((RectTransform)name.transform, new Vector2(0.5f, 0f),
                new Vector2(0f, 78f), new Vector2(w, 34f));

            var costLbl = UiFactory.Label(rt, "Cost", cost, 26,
                new Color(0.95f, 0.85f, 0.4f), TextAnchor.MiddleCenter);
            UiFactory.Place((RectTransform)costLbl.transform, new Vector2(0.5f, 0f),
                new Vector2(0f, 34f), new Vector2(w, 32f));

            return btn;
        }

        static Sprite IconSprite(IconShape shape) => shape switch
        {
            IconShape.Circle => SpriteFactory.Circle,
            IconShape.Diamond => SpriteFactory.Diamond,
            IconShape.Triangle => SpriteFactory.Triangle,
            _ => SpriteFactory.Square
        };

        void RefreshCards()
        {
            foreach (var (def, btn) in cards)
                btn.interactable = game.Stock.CanAfford(def.Cost);
        }

        // ---- Flow -------------------------------------------------------------

        void ToggleStrip()
        {
            if (IsBusy) return;
            strip.gameObject.SetActive(!strip.gameObject.activeSelf);
        }

        void StartPlacement(BuildingDef def)
        {
            if (IsBusy || !game.Buildings.CanAfford(def)) return;
            game.Selection.Deselect();

            Vector2 center = game.Cam.ScreenToWorld(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
            placement = new BuildPlacementMode(game.Buildings, game.Map, def, center);
            placement.Changed += RefreshConfirm;
            game.InputRouter.Mode = placement;

            strip.gameObject.SetActive(false);
            buildBtn.gameObject.SetActive(false);
            confirmBtn.gameObject.SetActive(true);
            cancelBtn.gameObject.SetActive(true);
            RefreshConfirm();
        }

        void RefreshConfirm()
        {
            if (placement == null) return;
            confirmBtn.interactable = placement.IsValid && game.Buildings.CanAfford(placement.Def);
        }

        void ConfirmPlacement()
        {
            if (placement == null) return;
            if (placement.Confirm() != null)
                ExitPlacement();
        }

        void CancelPlacement()
        {
            placement?.Exit();
            ExitPlacement();
        }

        void ExitPlacement()
        {
            placement = null;
            game.InputRouter.Mode = null;
            confirmBtn.gameObject.SetActive(false);
            cancelBtn.gameObject.SetActive(false);
            buildBtn.gameObject.SetActive(true);
        }

        void StartRoadMode()
        {
            if (IsBusy) return;
            game.Selection.Deselect();
            roadMode = new RoadPaintMode(game.Map, game.Stock, GameConfig.RoadStoneCost);
            game.InputRouter.Mode = roadMode;
            strip.gameObject.SetActive(false);
            buildBtn.gameObject.SetActive(false);
            doneBtn.gameObject.SetActive(true);
        }

        void ExitRoadMode()
        {
            roadMode = null;
            game.InputRouter.Mode = null;
            doneBtn.gameObject.SetActive(false);
            buildBtn.gameObject.SetActive(true);
        }
    }
}
