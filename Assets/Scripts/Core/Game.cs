using SnoopyKnights.CameraControl;
using SnoopyKnights.Grid;
using UnityEngine;

namespace SnoopyKnights.Core
{
    /// <summary>
    /// Composition root. Creates and wires all subsystems; owns no game logic.
    /// </summary>
    public sealed class Game : MonoBehaviour
    {
        public static Game Instance { get; private set; }

        public GridMap Map { get; private set; }
        public GridRenderer MapRenderer { get; private set; }
        public CameraController Cam { get; private set; }
        public InputRouter InputRouter { get; private set; }
        public SelectionController Selection { get; private set; }

        void Awake()
        {
            Instance = this;
            Application.targetFrameRate = 60;

            Map = MapGenerator.Generate(seed: 20260704);

            MapRenderer = CreateChild<GridRenderer>("Map");
            MapRenderer.Build(Map);

            Cam = CameraController.CreateMainCamera(Map);

            InputRouter = CreateChild<InputRouter>("Input");
            InputRouter.Init(Cam);

            Selection = CreateChild<SelectionController>("Selection");
            Selection.Init(Map, InputRouter);
        }

        T CreateChild<T>(string name) where T : Component
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            return go.AddComponent<T>();
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
