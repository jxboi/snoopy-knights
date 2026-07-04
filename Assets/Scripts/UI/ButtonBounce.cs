using UnityEngine;
using UnityEngine.EventSystems;

namespace SnoopyKnights.UI
{
    /// <summary>
    /// Tactile feedback for a UI element: a subtle scale-up while the pointer
    /// hovers over it, plus a quick scale-punch when pressed.
    /// </summary>
    public sealed class ButtonBounce : MonoBehaviour,
        IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        const float HoverScale = 0.06f; // how much bigger while hovered
        const float PunchDip = 0.12f;   // how much smaller at the press instant

        RectTransform rt;
        bool hovered;
        float hoverT;   // eased 0..1 hover amount
        float punch;    // 1 at press, decays to 0

        void Awake() => rt = (RectTransform)transform;

        public void OnPointerDown(PointerEventData e) => punch = 1f;

        public void OnPointerEnter(PointerEventData e)
        {
            hovered = true;
            transform.SetAsLastSibling(); // pop above neighbouring cards
        }

        public void OnPointerExit(PointerEventData e) => hovered = false;

        void Update()
        {
            hoverT = Mathf.MoveTowards(hoverT, hovered ? 1f : 0f, Time.unscaledDeltaTime * 8f);
            punch = Mathf.Max(0f, punch - Time.unscaledDeltaTime * 6f);

            float s = (1f + HoverScale * hoverT) * (1f - PunchDip * punch);
            var target = new Vector3(s, s, 1f);
            if (rt.localScale != target) rt.localScale = target;
        }
    }
}
