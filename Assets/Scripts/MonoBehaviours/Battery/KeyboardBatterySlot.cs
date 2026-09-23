using UnityEngine;

namespace Lilo.MonoBehaviours.Battery
{
    /// <summary>
    /// Authoring component on a keyboard prop: holds at most one battery.
    /// Loaded state is chosen per floor entry by <see cref="KeyboardBatteryDirector"/>
    /// and signalled by swapping the keyboard mesh to an emissive "loaded"
    /// material. Assigned by LILO/Setup Keyboard Batteries.
    /// </summary>
    public class KeyboardBatterySlot : MonoBehaviour
    {
        [Tooltip("Interaction distance threshold for taking this slot's battery.")]
        [SerializeField] private float interactionRadius = 2f;
        [SerializeField] private Material baseMaterial;
        [SerializeField] private Material loadedMaterial;

        private Renderer _renderer;

        public bool IsLoaded { get; private set; }
        public float InteractionRadius => interactionRadius;

        private void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
            if (_renderer == null)
                _renderer = GetComponentInChildren<Renderer>();
            if (baseMaterial == null && _renderer != null)
                baseMaterial = _renderer.sharedMaterial;
            if (loadedMaterial == null)
                loadedMaterial = Resources.Load<Material>("KeyboardBatteryLoaded");
            UpdateVisual();
        }

        public void SetLoaded(bool loaded)
        {
            IsLoaded = loaded;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (_renderer == null) return;

            Material mat = IsLoaded && loadedMaterial != null ? loadedMaterial : baseMaterial;
            if (mat != null)
                _renderer.sharedMaterial = mat;
        }
    }
}
