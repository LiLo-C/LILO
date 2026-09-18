using Lilo.Systems.Movement;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Lilo.MonoBehaviours.Input
{
    /// <summary>
    /// Thin adapter: reads the on-screen joystick's current drag state and produces a
    /// MovementInput each frame. Works with mouse (Editor testing) and touch alike via uGUI's
    /// pointer events. No gameplay logic lives here (spec FR-010) — MovementSystem owns that.
    /// </summary>
    public class JoystickInputAdapter : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;
        [SerializeField] private float radius = 80f;

        private Vector2 _rawOffset;
        private bool _active;

        public MovementInput CurrentInput
        {
            get
            {
                if (!_active || _rawOffset == Vector2.zero)
                {
                    return MovementInput.Zero;
                }

                // US4: clamp/normalize magnitude to 0..1 at the input boundary (spec T020) so
                // diagonal drag can never report a magnitude above 1 — direction itself is
                // normalized again in MovementInput's constructor.
                float magnitude = Mathf.Clamp01(_rawOffset.magnitude / radius);
                return new MovementInput(_rawOffset, magnitude);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _active = true;
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (background == null)
            {
                return;
            }

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, eventData.pressEventCamera, out localPoint);
            _rawOffset = localPoint;

            if (handle != null)
            {
                Vector2 clamped = Vector2.ClampMagnitude(_rawOffset, radius);
                handle.anchoredPosition = clamped;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // Spec Edge Cases: a lost touch mid-gesture MUST be treated as zero deflection, never
            // "last known deflection persists."
            _active = false;
            _rawOffset = Vector2.zero;
            if (handle != null)
            {
                handle.anchoredPosition = Vector2.zero;
            }
        }
    }
}
