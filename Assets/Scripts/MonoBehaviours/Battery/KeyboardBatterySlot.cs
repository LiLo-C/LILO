using UnityEngine;
using Lilo.MonoBehaviours;

namespace Lilo.MonoBehaviours.Battery
{
    /// <summary>
    /// Authoring component on a keyboard prop: holds at most one battery.
    /// Loaded state is chosen per floor entry by <see cref="KeyboardBatteryDirector"/>
    /// and signalled by an outer-only X-ray outline. Assigned by LILO/Setup Keyboard Batteries.
    /// </summary>
    public class KeyboardBatterySlot : MonoBehaviour
    {
        private static readonly Color LoadedOutlineColor = new Color(0.05f, 0.9f, 1f, 1f);
        [Tooltip("Interaction distance threshold for taking this slot's battery.")]
        [SerializeField] private float interactionRadius = 2f;
        private PlayerXRayOutline _outline;

        public bool IsLoaded { get; private set; }
        public float InteractionRadius => interactionRadius;

        private void Awake()
        {
            EnsureOutline();
            UpdateVisual();
        }

        public void SetLoaded(bool loaded)
        {
            IsLoaded = loaded;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            EnsureOutline();
            if (_outline != null)
                _outline.SetOutlineVisible(IsLoaded);
        }

        private void EnsureOutline()
        {
            if (_outline == null)
                _outline = GetComponent<PlayerXRayOutline>();
            if (_outline == null)
                _outline = gameObject.AddComponent<PlayerXRayOutline>();
            if (_outline != null)
            {
                _outline.ConfigureOutline(LoadedOutlineColor, 0.016f, true);
                _outline.SetOutlineVisible(IsLoaded);
            }
        }
    }
}
