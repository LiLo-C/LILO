using UnityEngine;
using Lilo.MonoBehaviours;

namespace Lilo.MonoBehaviours.Battery
{
    /// <summary>
    /// Authoring component on a keyboard prop: holds at most one battery.
    /// Loaded state is chosen per floor entry by <see cref="KeyboardBatteryDirector"/>
    /// and signalled by a continuous perimeter outline. Assigned by LILO/Setup Keyboard Batteries.
    /// </summary>
    public class KeyboardBatterySlot : MonoBehaviour
    {
        private static readonly Color LoadedOutlineColor = new Color(0.05f, 0.9f, 1f, 1f);
        [Tooltip("Interaction distance threshold for taking this slot's battery.")]
        [SerializeField] private float interactionRadius = 2f;
        private LineRenderer _outline;

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
            ApplyPulse();
        }

        private void Update()
        {
            ApplyPulse();
        }

        private void ApplyPulse()
        {
            if (_outline == null) return;
            float opacity = IsLoaded ? PlayerXRayOutline.PickupPulseOpacity(Time.time) : 0f;
            _outline.enabled = opacity > 0.001f;
            Color color = LoadedOutlineColor;
            color.a = opacity;
            _outline.startColor = color;
            _outline.endColor = color;
            Material material = _outline.sharedMaterial;
            if (material == null) return;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        }

        private void EnsureOutline()
        {
            if (_outline == null)
            {
                _outline = GetComponent<LineRenderer>();
                if (_outline == null)
                    _outline = gameObject.AddComponent<LineRenderer>();
                ConfigureOutline();
            }
        }

        private void ConfigureOutline()
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
                return;

            Bounds bounds = meshFilter.sharedMesh.bounds;
            _outline.useWorldSpace = false;
            _outline.loop = true;
            _outline.positionCount = 4;
            _outline.SetPosition(0, new Vector3(bounds.min.x, bounds.min.y, bounds.center.z));
            _outline.SetPosition(1, new Vector3(bounds.max.x, bounds.min.y, bounds.center.z));
            _outline.SetPosition(2, new Vector3(bounds.max.x, bounds.max.y, bounds.center.z));
            _outline.SetPosition(3, new Vector3(bounds.min.x, bounds.max.y, bounds.center.z));
            _outline.startWidth = 0.018f;
            _outline.endWidth = 0.018f;
            _outline.alignment = LineAlignment.View;
            _outline.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _outline.receiveShadows = false;
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Unlit/Color")
                ?? Shader.Find("Sprites/Default");
            if (shader != null)
            {
                var material = new Material(shader) { name = "Keyboard Outline (Runtime)" };
                // URP Unlit defaults to opaque; explicitly enable alpha blending
                // so lowering opacity visibly fades the complete perimeter.
                if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f);
                if (material.HasProperty("_Blend")) material.SetFloat("_Blend", 0f);
                if (material.HasProperty("_SrcBlend")) material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                if (material.HasProperty("_DstBlend")) material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 0f);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.SetOverrideTag("RenderType", "Transparent");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", LoadedOutlineColor);
                if (material.HasProperty("_Color")) material.SetColor("_Color", LoadedOutlineColor);
                _outline.sharedMaterial = material;
            }
            _outline.startColor = LoadedOutlineColor;
            _outline.endColor = LoadedOutlineColor;
            _outline.enabled = IsLoaded;
        }

        private void OnDestroy()
        {
            if (_outline != null && _outline.sharedMaterial != null
                && _outline.sharedMaterial.name == "Keyboard Outline (Runtime)")
                Destroy(_outline.sharedMaterial);
        }
    }
}
