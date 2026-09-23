using UnityEngine;

namespace Lilo.MonoBehaviours.Battery
{
    /// <summary>
    /// Authoring component on a keyboard prop: holds at most one battery.
    /// Loaded state is chosen per floor entry by <see cref="KeyboardBatteryDirector"/>;
    /// the battery visual (a "BatteryVisual" child created by setup) toggles with it.
    /// </summary>
    public class KeyboardBatterySlot : MonoBehaviour
    {
        [Tooltip("Interaction distance threshold for taking this slot's battery.")]
        [SerializeField] private float interactionRadius = 2f;

        private GameObject _visual;

        public bool IsLoaded { get; private set; }
        public float InteractionRadius => interactionRadius;

        private void Awake()
        {
            var visual = transform.Find("BatteryVisual");
            _visual = visual != null ? visual.gameObject : null;
            UpdateVisual();
        }

        public void SetLoaded(bool loaded)
        {
            IsLoaded = loaded;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (_visual != null)
                _visual.SetActive(IsLoaded);
        }
    }
}
