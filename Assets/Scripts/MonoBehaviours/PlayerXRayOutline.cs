using System.Collections.Generic;
using UnityEngine;

namespace Lilo.MonoBehaviours
{
    /// <summary>Draws only Eddie's outline through walls without changing obstacle materials.</summary>
    [DisallowMultipleComponent]
    public sealed class PlayerXRayOutline : MonoBehaviour
    {
        public static readonly Color DefaultOutlineColor = new Color(1f, 0.86f, 0.48f, 0.95f);

        [SerializeField] private Color outlineColor = new Color(1f, 0.86f, 0.48f, 0.95f);
        [SerializeField, Range(0.001f, 0.03f)] private float outlineWidth = 0.006f;
        [SerializeField, Min(0.05f)] private float transitionSeconds = 0.3f;

        private readonly List<(Renderer source, Renderer mask, Renderer outline)> _pairs =
            new List<(Renderer, Renderer, Renderer)>();
        private readonly RaycastHit[] _raycastHits = new RaycastHit[16];
        private readonly float[] _probeHeights = { 0.18f, 0.48f, 0.8f, 1.12f, 1.42f, 1.72f };
        private readonly Vector2[] _probeOffsets = { Vector2.zero, new Vector2(-0.25f, 1.12f), new Vector2(0.25f, 1.12f) };
        private Material _outlineMaterial;
        private Material _flatOutlineMaterial;
        private Material _maskMaterial;
        private UnityEngine.Camera _camera;
        private float _xrayFade;
        private float _fadeVelocity;
        private bool _alwaysVisibleOutline;
        private bool _outlineVisible = true;

        public void ConfigureOutline(Color color, float width, bool alwaysVisible)
        {
            outlineColor = color;
            outlineWidth = Mathf.Clamp(width, 0.001f, 0.03f);
            _alwaysVisibleOutline = alwaysVisible;
            if (_outlineMaterial == null) return;
            _outlineMaterial.SetColor("_OutlineColor", outlineColor);
            _outlineMaterial.SetFloat("_OutlineWidth", outlineWidth);
            if (_flatOutlineMaterial != null)
            {
                _flatOutlineMaterial.SetColor("_OutlineColor", outlineColor);
                _flatOutlineMaterial.SetFloat("_OutlineWidth", outlineWidth);
                _flatOutlineMaterial.SetFloat("_AlphaOutlineRadius", Mathf.Clamp(outlineWidth * 384f, 1.5f, 4f));
            }
        }

        public void SetOutlineVisible(bool visible)
        {
            _outlineVisible = visible;
            if (!visible)
            {
                _xrayFade = 0f;
                _fadeVelocity = 0f;
                if (_outlineMaterial != null)
                    _outlineMaterial.SetFloat("_XRayFade", 0f);
                if (_flatOutlineMaterial != null)
                    _flatOutlineMaterial.SetFloat("_XRayFade", 0f);
            }
        }

        private void Awake()
        {
            _camera = UnityEngine.Camera.main;
            Shader outlineShader = Shader.Find("LILO/XRayOutline");
            Shader maskShader = Shader.Find("LILO/XRayStencilMask");
            if (outlineShader == null || maskShader == null)
            {
                Debug.LogError("[PlayerXRayOutline] Could not load the X-ray outline shaders.", this);
                enabled = false;
                return;
            }

            _outlineMaterial = new Material(outlineShader) { name = "Eddie XRay Outline (Runtime)" };
            _outlineMaterial.SetColor("_OutlineColor", outlineColor);
            _outlineMaterial.SetFloat("_OutlineWidth", outlineWidth);
            // The shell should pass only where the scene depth is in front of Eddie.
            // The previous reversed-Z branch selected Less on Metal and hid the covered edge.
            _outlineMaterial.SetInt("_DepthTest", (int)UnityEngine.Rendering.CompareFunction.Greater);
            _outlineMaterial.SetFloat("_XRayFade", 0f);
            _maskMaterial = new Material(maskShader) { name = "Eddie XRay Stencil Mask (Runtime)" };

            foreach (Renderer source in GetComponentsInChildren<Renderer>(true))
            {
                if (source is SkinnedMeshRenderer skinned)
                    CreateSkinnedOutline(skinned);
                else if (source is MeshRenderer meshRenderer)
                    CreateMeshOutline(meshRenderer);
            }
        }

        private void LateUpdate()
        {
            if (_camera == null) _camera = UnityEngine.Camera.main;
            float targetFade = _outlineVisible && (_alwaysVisibleOutline || IsOccluded()) ? 1f : 0f;
            _xrayFade = Mathf.SmoothDamp(_xrayFade, targetFade, ref _fadeVelocity,
                Mathf.Max(0.0125f, transitionSeconds * 0.25f));
            if (Mathf.Abs(_xrayFade - targetFade) < 0.001f)
                _xrayFade = targetFade;
            if (_outlineMaterial != null)
                _outlineMaterial.SetFloat("_XRayFade", _xrayFade);
            if (_flatOutlineMaterial != null)
                _flatOutlineMaterial.SetFloat("_XRayFade", _xrayFade);

            foreach (var pair in _pairs)
            {
                if (pair.source == null || pair.mask == null || pair.outline == null) continue;
                bool sourceVisible = pair.source.enabled && pair.source.gameObject.activeInHierarchy;
                pair.mask.enabled = sourceVisible;
                pair.outline.enabled = sourceVisible && _xrayFade > 0f;
                Transform source = pair.source.transform;
                SyncTransform(pair.mask.transform, source);
                SyncTransform(pair.outline.transform, source);
            }
        }

        private static void SyncTransform(Transform destination, Transform source)
        {
            destination.localPosition = source.localPosition;
            destination.localRotation = source.localRotation;
            destination.localScale = source.localScale;
        }

        private bool IsOccluded()
        {
            if (_camera == null) return false;
            Vector3 origin = _camera.transform.position;
            for (int i = 0; i < _probeHeights.Length; i++)
            {
                Vector3 target = transform.position + Vector3.up * _probeHeights[i];
                if (IsPointOccluded(origin, target)) return true;
            }

            Vector3 shoulder = transform.position + Vector3.up * 1.12f;
            for (int i = 1; i < _probeOffsets.Length; i++)
            {
                Vector3 target = shoulder + transform.right * _probeOffsets[i].x;
                if (IsPointOccluded(origin, target)) return true;
            }
            return false;
        }

        private bool IsPointOccluded(Vector3 origin, Vector3 target)
        {
            Vector3 direction = target - origin;
            float distance = direction.magnitude;
            if (distance <= 0.01f) return false;

            int hitCount = Physics.RaycastNonAlloc(origin, direction / distance, _raycastHits, distance,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = _raycastHits[i];
                Transform hitTransform = hit.collider.transform;
                if (hitTransform == transform || hitTransform.IsChildOf(transform)) continue;
                Renderer hitRenderer = hit.collider.GetComponent<Renderer>();
                if (hitRenderer == null) hitRenderer = hit.collider.GetComponentInParent<Renderer>();
                if (hitRenderer == null) hitRenderer = hit.collider.GetComponentInChildren<Renderer>();
                if (hitRenderer != null) return true;
            }
            return false;
        }

        private void CreateSkinnedOutline(SkinnedMeshRenderer source)
        {
            if (source.sharedMesh == null || source.sharedMaterials.Length == 0) return;
            GameObject outlineObject = CreateOutlineObject(source.transform);
            SkinnedMeshRenderer mask = outlineObject.AddComponent<SkinnedMeshRenderer>();
            mask.sharedMesh = source.sharedMesh;
            mask.bones = source.bones;
            mask.rootBone = source.rootBone;
            mask.localBounds = source.localBounds;
            mask.quality = source.quality;
            mask.updateWhenOffscreen = true;
            mask.sharedMaterials = CreateMaterialSlots(source.sharedMaterials.Length, _maskMaterial);
            Configure(mask, source);

            outlineObject = CreateOutlineObject(source.transform);
            SkinnedMeshRenderer outline = outlineObject.AddComponent<SkinnedMeshRenderer>();
            outline.sharedMesh = source.sharedMesh;
            outline.bones = source.bones;
            outline.rootBone = source.rootBone;
            outline.localBounds = source.localBounds;
            outline.quality = source.quality;
            outline.updateWhenOffscreen = true;
            outline.sharedMaterials = CreateMaterialSlots(source.sharedMaterials.Length, _outlineMaterial);
            Configure(outline, source);
            _pairs.Add((source, mask, outline));
        }

        private void CreateMeshOutline(MeshRenderer source)
        {
            MeshFilter sourceFilter = source.GetComponent<MeshFilter>();
            if (sourceFilter == null || sourceFilter.sharedMesh == null || source.sharedMaterials.Length == 0) return;
            GameObject outlineObject = CreateOutlineObject(source.transform);
            MeshFilter filter = outlineObject.AddComponent<MeshFilter>();
            filter.sharedMesh = sourceFilter.sharedMesh;
            MeshRenderer mask = outlineObject.AddComponent<MeshRenderer>();
            mask.sharedMaterials = CreateMaterialSlots(source.sharedMaterials.Length, _maskMaterial);
            Configure(mask, source);

            outlineObject = CreateOutlineObject(source.transform);
            filter = outlineObject.AddComponent<MeshFilter>();
            filter.sharedMesh = sourceFilter.sharedMesh;
            MeshRenderer outline = outlineObject.AddComponent<MeshRenderer>();
            bool flatMesh = sourceFilter.sharedMesh.vertexCount <= 4;
            if (flatMesh)
            {
                if (_flatOutlineMaterial == null)
                {
                    _flatOutlineMaterial = new Material(_outlineMaterial)
                    {
                        name = "Flat XRay Outline (Runtime)"
                    };
                    _flatOutlineMaterial.SetFloat("_RadialExpansion", 1f);
                    _flatOutlineMaterial.SetFloat("_OutlineCull", (float)UnityEngine.Rendering.CullMode.Off);
                    _flatOutlineMaterial.SetFloat("_AlphaOutlineRadius", Mathf.Clamp(outlineWidth * 384f, 1.5f, 4f));
                    // Flat pickups need a visible edge even when their coplanar source
                    // mesh is behind the scene depth due to floor/camera precision.
                    _flatOutlineMaterial.SetInt("_DepthTest", (int)UnityEngine.Rendering.CompareFunction.Always);
                }
                outline.sharedMaterials = CreateMaterialSlots(source.sharedMaterials.Length, _flatOutlineMaterial);
            }
            else
                outline.sharedMaterials = CreateMaterialSlots(source.sharedMaterials.Length, _outlineMaterial);
            if (flatMesh)
                ConfigureAlphaSilhouette(source, mask, outline);
            Configure(outline, source);
            _pairs.Add((source, mask, outline));
        }

        private static void ConfigureAlphaSilhouette(MeshRenderer source, MeshRenderer mask, MeshRenderer outline)
        {
            Material sourceMaterial = null;
            string textureProperty = null;
            foreach (Material candidate in source.sharedMaterials)
            {
                if (candidate == null) continue;
                if (candidate.HasProperty("_BaseMap") && candidate.GetTexture("_BaseMap") != null)
                {
                    sourceMaterial = candidate;
                    textureProperty = "_BaseMap";
                    break;
                }
                if (candidate.HasProperty("_MainTex") && candidate.GetTexture("_MainTex") != null)
                {
                    sourceMaterial = candidate;
                    textureProperty = "_MainTex";
                    break;
                }
            }

            if (sourceMaterial == null) return;
            Texture alphaTexture = sourceMaterial.GetTexture(textureProperty);
            Vector2 scale = sourceMaterial.GetTextureScale(textureProperty);
            Vector2 offset = sourceMaterial.GetTextureOffset(textureProperty);
            float threshold = sourceMaterial.HasProperty("_Cutoff")
                ? sourceMaterial.GetFloat("_Cutoff")
                : 0.5f;
            Vector4 textureTransform = new Vector4(scale.x, scale.y, offset.x, offset.y);

            var maskProperties = new MaterialPropertyBlock();
            mask.GetPropertyBlock(maskProperties);
            maskProperties.SetTexture("_StencilAlphaTex", alphaTexture);
            maskProperties.SetVector("_StencilAlphaTex_ST", textureTransform);
            maskProperties.SetFloat("_UseAlphaMask", 1f);
            maskProperties.SetFloat("_StencilAlphaThreshold", threshold);
            mask.SetPropertyBlock(maskProperties);

            var outlineProperties = new MaterialPropertyBlock();
            outline.GetPropertyBlock(outlineProperties);
            outlineProperties.SetTexture("_OutlineAlphaTex", alphaTexture);
            outlineProperties.SetVector("_OutlineAlphaTex_ST", textureTransform);
            outlineProperties.SetFloat("_UseAlphaOutline", 1f);
            outlineProperties.SetFloat("_AlphaThreshold", threshold);
            outline.SetPropertyBlock(outlineProperties);
        }

        private GameObject CreateOutlineObject(Transform source)
        {
            var outlineObject = new GameObject(source.name + " XRay Outline");
            outlineObject.layer = source.gameObject.layer;
            outlineObject.transform.SetParent(source.parent, false);
            outlineObject.transform.localPosition = source.localPosition;
            outlineObject.transform.localRotation = source.localRotation;
            outlineObject.transform.localScale = source.localScale;
            return outlineObject;
        }

        private static Material[] CreateMaterialSlots(int count, Material material)
        {
            var materials = new Material[count];
            for (int i = 0; i < count; i++) materials[i] = material;
            return materials;
        }

        private static void Configure(Renderer outline, Renderer source)
        {
            outline.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            outline.receiveShadows = false;
            outline.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            outline.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            outline.sortingLayerID = source.sortingLayerID;
            outline.sortingOrder = source.sortingOrder;
            outline.enabled = source.enabled;
        }

        private void OnDestroy()
        {
            foreach (var pair in _pairs)
            {
                if (pair.outline != null)
                    Destroy(pair.outline.gameObject);
                if (pair.mask != null)
                    Destroy(pair.mask.gameObject);
            }
            if (_outlineMaterial != null) Destroy(_outlineMaterial);
            if (_flatOutlineMaterial != null) Destroy(_flatOutlineMaterial);
            if (_maskMaterial != null) Destroy(_maskMaterial);
        }
    }
}
