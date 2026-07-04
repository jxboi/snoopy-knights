using SnoopyKnights.Grid;
using SnoopyKnights.Rendering;
using UnityEngine;

namespace SnoopyKnights.CameraControl
{
    /// <summary>
    /// Orthographic camera pitched down at the ground plane (ViewTilt) for an
    /// isometric-style view, with pan, pinch/scroll zoom and map clamping.
    /// Receives gestures from InputRouter; does no input reading itself.
    /// All world positions exposed here are points on the z=0 ground plane.
    /// </summary>
    public sealed class CameraController : MonoBehaviour
    {
        const float MinOrthoSize = 3.5f;
        const float EdgePadding = 2f;
        const float CamDistance = 10f;

        // Ground point at screen center sits this far north of transform.y.
        static readonly float CenterOffset = CamDistance * ViewTilt.Tan;

        Camera cam;
        GridMap map;
        float maxOrthoSize = 12f;

        float trauma;
        Vector3 appliedShake;

        public Camera Camera => cam;

        /// <summary>The ground-plane point currently at screen center (save/load).</summary>
        public Vector2 GroundCenterWorld =>
            new Vector2(transform.position.x, transform.position.y + CenterOffset);

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
            // The tilt compresses the map vertically, so less zoom fits it all.
            ctrl.maxOrthoSize = (map.Height * 0.5f + EdgePadding) * ViewTilt.Cos;

            go.transform.rotation = ViewTilt.Upright;
            ctrl.CenterOn(new Vector2(map.Width * 0.5f, map.Height * 0.4f));
            return ctrl;
        }

        /// <summary>Casts the screen point onto the z=0 ground plane.</summary>
        public Vector2 ScreenToWorld(Vector2 screenPos)
        {
            var ray = cam.ScreenPointToRay(screenPos);
            float t = -ray.origin.z / ray.direction.z;
            return ray.GetPoint(t);
        }

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

        /// <summary>Grab-style pan: the ground under the finger follows it.
        /// Vertical screen distance covers more ground under the tilt.</summary>
        public void PanByScreenDelta(Vector2 screenDelta)
        {
            float upp = WorldUnitsPerPixel;
            var worldDelta = new Vector3(
                screenDelta.x * upp, screenDelta.y * upp / ViewTilt.Cos, 0f);
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
            transform.position = new Vector3(worldPos.x, worldPos.y - CenterOffset, -CamDistance);
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
            // Clamp the ground point at screen center; the screen's vertical
            // half-extent covers orthoSize / cos on the tilted ground.
            float halfH = cam.orthographicSize / ViewTilt.Cos;
            float halfW = cam.orthographicSize * cam.aspect;
            var p = transform.position;
            p.x = ClampAxis(p.x, halfW, map.Width);
            p.y = ClampAxis(p.y + CenterOffset, halfH, map.Height) - CenterOffset;
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
