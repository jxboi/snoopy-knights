using UnityEngine;
using UnityEngine.EventSystems;

namespace SnoopyKnights.UI
{
    /// <summary>Gives a UI element a quick scale-punch when pressed (tactile feedback).</summary>
    public sealed class ButtonBounce : MonoBehaviour, IPointerDownHandler
    {
        RectTransform rt;
        float punch;

        void Awake() => rt = (RectTransform)transform;

        public void OnPointerDown(PointerEventData e) => punch = 1f;

        void Update()
        {
            if (punch <= 0f)
            {
                if (rt.localScale != Vector3.one) rt.localScale = Vector3.one;
                return;
            }
            punch = Mathf.Max(0f, punch - Time.unscaledDeltaTime * 6f);
            float s = 1f - 0.12f * punch; // dips in, springs back
            rt.localScale = new Vector3(s, s, 1f);
        }
    }
}
