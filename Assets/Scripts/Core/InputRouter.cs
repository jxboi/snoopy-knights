using SnoopyKnights.CameraControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SnoopyKnights.Core
{
    /// <summary>
    /// A tool that consumes taps/drags. When no mode is active, drags pan the
    /// camera and taps go to selection (via the TapWorld event).
    /// </summary>
    public interface IInputMode
    {
        /// <summary>If true, one-finger drags go to the mode instead of panning.</summary>
        bool UsesDrag { get; }
        void OnTap(Vector2 world);
        /// <summary>Mouse moved with no button held (desktop only; touch has no hover).</summary>
        void OnHover(Vector2 world);
        void OnDragStart(Vector2 world);
        void OnDrag(Vector2 world);
        void OnDragEnd(Vector2 world);
    }

    /// <summary>
    /// Turns raw touch/mouse input into gestures: tap, one-finger drag,
    /// two-finger pinch (zoom + pan). Touch-first; mouse works in the editor
    /// (drag pans, wheel zooms). Touches that start on UI are ignored.
    /// </summary>
    public sealed class InputRouter : MonoBehaviour
    {
        public event System.Action<Vector2> TapWorld;

        IInputMode mode;
        public IInputMode Mode
        {
            get => mode;
            set
            {
                if (gesture == Gesture.Drag && mode != null && mode.UsesDrag)
                    mode.OnDragEnd(cam.ScreenToWorld(lastScreen));
                mode = value;
                if (gesture == Gesture.Drag) gesture = Gesture.Ignored;
            }
        }

        enum Gesture { None, Pending, Drag, Pinch, Ignored }

        CameraController cam;
        Gesture gesture = Gesture.None;
        Vector2 startScreen, lastScreen;
        Vector2 pinchA, pinchB;

        float DragThresholdPx => Mathf.Max(18f, (Screen.dpi > 0 ? Screen.dpi : 160f) * 0.1f);

        public void Init(CameraController cameraController) => cam = cameraController;

        void Update()
        {
            if (cam == null) return;
            if (Input.touchCount > 0) UpdateTouch();
            else UpdateMouse();
        }

        // ---- Touch -------------------------------------------------------

        void UpdateTouch()
        {
            if (Input.touchCount >= 2)
            {
                UpdatePinch(Input.GetTouch(0), Input.GetTouch(1));
                return;
            }

            var t = Input.GetTouch(0);
            switch (t.phase)
            {
                case TouchPhase.Began:
                    BeginPointer(t.position, t.fingerId);
                    break;
                case TouchPhase.Moved:
                    MovePointer(t.position);
                    break;
                case TouchPhase.Ended:
                    EndPointer(t.position);
                    break;
                case TouchPhase.Canceled:
                    CancelPointer();
                    break;
            }
        }

        void UpdatePinch(Touch a, Touch b)
        {
            if (gesture == Gesture.Drag && mode != null && mode.UsesDrag)
                mode.OnDragEnd(cam.ScreenToWorld(lastScreen));

            if (gesture != Gesture.Pinch)
            {
                gesture = Gesture.Pinch;
                pinchA = a.position;
                pinchB = b.position;
                return;
            }

            Vector2 prevMid = (pinchA + pinchB) * 0.5f;
            Vector2 mid = (a.position + b.position) * 0.5f;
            float prevDist = Mathf.Max(1f, Vector2.Distance(pinchA, pinchB));
            float dist = Mathf.Max(1f, Vector2.Distance(a.position, b.position));

            cam.ZoomAt(mid, prevDist / dist);
            cam.PanByScreenDelta(mid - prevMid);

            pinchA = a.position;
            pinchB = b.position;
        }

        // ---- Mouse (editor / desktop convenience) -------------------------

        void UpdateMouse()
        {
            if (gesture == Gesture.Pinch) gesture = Gesture.None;

            Vector2 pos = Input.mousePosition;

            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f && !IsOverUI(-1))
                cam.ZoomAt(pos, scroll > 0 ? 1f / 1.12f : 1.12f);

            if (Input.GetMouseButtonDown(0)) BeginPointer(pos, -1);
            else if (Input.GetMouseButton(0)) MovePointer(pos);
            else if (Input.GetMouseButtonUp(0)) EndPointer(pos);
            else if (mode != null && !IsOverUI(-1)) mode.OnHover(cam.ScreenToWorld(pos));
        }

        // ---- Shared pointer state machine ---------------------------------

        void BeginPointer(Vector2 screen, int pointerId)
        {
            gesture = IsOverUI(pointerId) ? Gesture.Ignored : Gesture.Pending;
            startScreen = lastScreen = screen;
        }

        void MovePointer(Vector2 screen)
        {
            switch (gesture)
            {
                case Gesture.Pending when (screen - startScreen).magnitude > DragThresholdPx:
                    gesture = Gesture.Drag;
                    if (mode != null && mode.UsesDrag)
                        mode.OnDragStart(cam.ScreenToWorld(startScreen));
                    else
                        cam.PanByScreenDelta(screen - lastScreen);
                    break;
                case Gesture.Drag:
                    if (mode != null && mode.UsesDrag)
                        mode.OnDrag(cam.ScreenToWorld(screen));
                    else
                        cam.PanByScreenDelta(screen - lastScreen);
                    break;
            }
            lastScreen = screen;
        }

        void EndPointer(Vector2 screen)
        {
            switch (gesture)
            {
                case Gesture.Pending:
                    var world = cam.ScreenToWorld(screen);
                    if (mode != null) mode.OnTap(world);
                    else TapWorld?.Invoke(world);
                    break;
                case Gesture.Drag when mode != null && mode.UsesDrag:
                    mode.OnDragEnd(cam.ScreenToWorld(screen));
                    break;
            }
            gesture = Gesture.None;
        }

        void CancelPointer()
        {
            if (gesture == Gesture.Drag && mode != null && mode.UsesDrag)
                mode.OnDragEnd(cam.ScreenToWorld(lastScreen));
            gesture = Gesture.None;
        }

        static bool IsOverUI(int pointerId)
        {
            if (EventSystem.current == null) return false;
            return pointerId >= 0
                ? EventSystem.current.IsPointerOverGameObject(pointerId)
                : EventSystem.current.IsPointerOverGameObject();
        }
    }
}
