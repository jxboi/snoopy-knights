using SnoopyKnights.Grid;
using UnityEngine;

namespace SnoopyKnights.CameraControl
{
    /// <summary>
    /// Orthographic top-down camera with pan, pinch/scroll zoom and map clamping.
    /// Receives gestures from InputRouter; does no input reading itself.
    /// </summary>
    public sealed class CameraController : MonoBehaviour
    {
        const float MinOrthoSize = 3.5f;
        const float EdgePadding = 2f;

        Camera cam;
        GridMap map;
        float maxOrthoSize = 12f;

        float trauma;
        Vector3 appliedShake;

        public Camera Camera => cam;

        public static CameraController CreateMainCamera(GridMap map)
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 7f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.13f, 0.19f, 0.11f);

            var ctrl = go.AddComponent<CameraController>();
            ctrl.cam = cam;
            ctrl.map = map;
            ctrl.maxOrthoSize = map.Height * 0.5f + EdgePadding;

            go.transform.position = new Vector3(map.Width * 0.5f, map.Height * 0.4f, -10f);
            ctrl.ClampToMap();
            return ctrl;
        }

        public Vector2 ScreenToWorld(Vector2 screenPos) => cam.ScreenToWorldPoint(screenPos);

        /// <summary>Adds screen shake. amount ~0.2 small, ~0.6 big hit. Scaled by zoom.</summary>
        public void Shake(float amount) => trauma = Mathf.Clamp01(trauma + amount);

        void LateUpdate()
        {
            // Keep shake separate from the panned/clamped base position.
            transform.position -= appliedShake;
            appliedShake = Vector3.zero;

            if (trauma > 0f)
            {
                trauma = Mathf.Max(0f, trauma - Time.unscaledDeltaTime * 1.6f);
                float mag = trauma * trauma * cam.orthographicSize * 0.12f;
                appliedShake = new Vector3(
                    (Random.value * 2f - 1f) * mag,
                    (Random.value * 2f - 1f) * mag, 0f);
                transform.position += appliedShake;
            }
        }

        public float WorldUnitsPerPixel => cam.orthographicSize * 2f / Screen.height;

        /// <summary>Grab-style pan: the world follows the finger.</summary>
        public void PanByScreenDelta(Vector2 screenDelta)
        {
            Vector3 worldDelta = (Vector3)(screenDelta * WorldUnitsPerPixel);
            transform.position -= worldDelta;
            ClampToMap();
        }

        /// <summary>Zooms keeping the world point under screenFocus stationary.</summary>
        public void ZoomAt(Vector2 screenFocus, float sizeFactor)
        {
            Vector2 before = ScreenToWorld(screenFocus);
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize * sizeFactor, MinOrthoSize, maxOrthoSize);
            Vector2 after = ScreenToWorld(screenFocus);
            transform.position += (Vector3)(before - after);
            ClampToMap();
        }

        public void CenterOn(Vector2 worldPos)
        {
            transform.position = new Vector3(worldPos.x, worldPos.y, -10f);
            ClampToMap();
        }

        /// <summary>Used by save/load.</summary>
        public void SetView(Vector2 worldPos, float orthoSize)
        {
            cam.orthographicSize = Mathf.Clamp(orthoSize, MinOrthoSize, maxOrthoSize);
            CenterOn(worldPos);
        }

        void ClampToMap()
        {
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;
            var p = transform.position;
            p.x = ClampAxis(p.x, halfW, map.Width);
            p.y = ClampAxis(p.y, halfH, map.Height);
            transform.position = p;
        }

        static float ClampAxis(float value, float halfView, float mapExtent)
        {
            float min = halfView - EdgePadding;
            float max = mapExtent - halfView + EdgePadding;
            return min > max ? mapExtent * 0.5f : Mathf.Clamp(value, min, max);
        }
    }
}
