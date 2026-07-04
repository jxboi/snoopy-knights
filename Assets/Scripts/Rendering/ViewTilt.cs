using UnityEngine;

namespace SnoopyKnights.Rendering
{
    /// <summary>
    /// The isometric-style view: the camera pitches down at the ground plane,
    /// which foreshortens the terrain, while buildings, trees and units are
    /// billboarded upright so they rise out of it. All simulation stays on the
    /// flat XY grid — this is purely a view transform.
    /// </summary>
    public static class ViewTilt
    {
        public const float Degrees = 40f;

        public static readonly float Cos = Mathf.Cos(Degrees * Mathf.Deg2Rad);
        public static readonly float Tan = Mathf.Tan(Degrees * Mathf.Deg2Rad);

        /// <summary>Rotation that stands a sprite upright, facing the tilted camera.</summary>
        public static readonly Quaternion Upright = Quaternion.Euler(-Degrees, 0f, 0f);

        /// <summary>
        /// World-y lift for a billboarded center-pivot sprite so its bottom edge
        /// still touches the ground point it covered when flat.
        /// (b = pivot-to-bottom distance in world units.)
        /// </summary>
        public static float PivotLift(float pivotToBottom) =>
            pivotToBottom * (1f - Cos) / Cos;

        /// <summary>
        /// Converts an intended on-screen y offset (as it looked untilted) into
        /// the world-y offset that produces it under the tilt. For head-anchored
        /// markers like bars and carry icons.
        /// </summary>
        public static float MarkerY(float screenOffset) => screenOffset / Cos;
    }
}
