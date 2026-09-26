using Lilo.MonoBehaviours.Interaction;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace Lilo.MonoBehaviours
{
    /// <summary>A compact, top-down player/exit locator for the three office floors.</summary>
    [DisallowMultipleComponent]
    public sealed class GameplayMinimap : MonoBehaviour
    {
        private const float Diameter = 112f;
        private const float EdgePadding = 11f;
        private RectTransform _mapRect;
        private RectTransform _playerDot;
        private RectTransform _exitDot;
        private Transform _player;
        private ExitDoorInteraction _exit;
        private Sprite _circleSprite;
        private Texture2D _circleTexture;
        private Vector2 _mapCenter;
        private float _mapHalfExtent;

        private void Awake()
        {
            BuildMap();
            ResolveActors();
            RecalculateBounds();
        }

        private void Update()
        {
            if (_player == null || _exit == null)
                ResolveActors();
            if (_playerDot != null && _player != null)
                PlaceDot(_playerDot, _player.position);
            if (_exitDot != null && _exit != null)
                PlaceDot(_exitDot, _exit.transform.position);
        }

        private void BuildMap()
        {
            GameObject map = new GameObject("GameplayMinimap", typeof(RectTransform), typeof(CanvasRenderer),
                typeof(Image), typeof(Mask), typeof(Outline));
            map.transform.SetParent(transform, false);
            _mapRect = map.GetComponent<RectTransform>();
            _mapRect.anchorMin = new Vector2(1f, 0f);
            _mapRect.anchorMax = new Vector2(1f, 0f);
            _mapRect.pivot = new Vector2(1f, 0f);
            _mapRect.sizeDelta = new Vector2(Diameter, Diameter);
            _mapRect.anchoredPosition = new Vector2(-16f, 16f);

            _circleSprite = CreateCircleSprite(out _circleTexture);
            Image background = map.GetComponent<Image>();
            background.sprite = _circleSprite;
            background.color = new Color(0.015f, 0.025f, 0.035f, 0.78f);
            background.raycastTarget = false;
            Mask mask = map.GetComponent<Mask>();
            mask.showMaskGraphic = true;
            Outline border = map.GetComponent<Outline>();
            border.effectColor = new Color(0.78f, 0.9f, 0.94f, 0.72f);
            border.effectDistance = new Vector2(1.5f, -1.5f);
            border.useGraphicAlpha = true;

            _playerDot = CreateDot(map.transform, "PlayerDot", 9f, _circleSprite,
                new Color(0.35f, 0.9f, 1f, 1f));
            _exitDot = CreateDot(map.transform, "ExitDoorDot", 10f, _circleSprite,
                new Color(0.25f, 1f, 0.38f, 1f));
        }

        private void ResolveActors()
        {
            if (_player == null)
                _player = GameObject.Find("PlayerCharacter")?.transform;
            if (_exit == null)
                _exit = Object.FindAnyObjectByType<ExitDoorInteraction>(FindObjectsInactive.Include);
        }

        private void RecalculateBounds()
        {
            bool hasBounds = false;
            float minX = 0f;
            float maxX = 0f;
            float minZ = 0f;
            float maxZ = 0f;
            NavMeshTriangulation navMesh = NavMesh.CalculateTriangulation();
            if (navMesh.vertices != null && navMesh.vertices.Length > 0)
            {
                foreach (Vector3 point in navMesh.vertices)
                    Encapsulate(point, ref hasBounds, ref minX, ref maxX, ref minZ, ref maxZ);
            }
            else
            {
                foreach (Renderer renderer in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
                {
                    Bounds bounds = renderer.bounds;
                    Encapsulate(bounds.min, ref hasBounds, ref minX, ref maxX, ref minZ, ref maxZ);
                    Encapsulate(bounds.max, ref hasBounds, ref minX, ref maxX, ref minZ, ref maxZ);
                }
            }

            if (_player != null)
                Encapsulate(_player.position, ref hasBounds, ref minX, ref maxX, ref minZ, ref maxZ);
            if (_exit != null)
                Encapsulate(_exit.transform.position, ref hasBounds, ref minX, ref maxX, ref minZ, ref maxZ);

            if (!hasBounds)
            {
                _mapCenter = Vector2.zero;
                _mapHalfExtent = 1f;
                return;
            }

            const float worldPadding = 1f;
            _mapCenter = new Vector2((minX + maxX) * 0.5f, (minZ + maxZ) * 0.5f);
            _mapHalfExtent = Mathf.Max((maxX - minX) * 0.5f, (maxZ - minZ) * 0.5f) + worldPadding;
            _mapHalfExtent = Mathf.Max(0.5f, _mapHalfExtent);
        }

        private void PlaceDot(RectTransform dot, Vector3 worldPosition)
        {
            float radius = Diameter * 0.5f - EdgePadding;
            dot.anchoredPosition = new Vector2(
                (worldPosition.x - _mapCenter.x) / _mapHalfExtent * radius,
                (worldPosition.z - _mapCenter.y) / _mapHalfExtent * radius);
        }

        private static RectTransform CreateDot(Transform parent, string name, float size, Sprite sprite, Color color)
        {
            GameObject dot = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            dot.transform.SetParent(parent, false);
            RectTransform rect = dot.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(size, size);
            Image image = dot.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.color = color;
            image.raycastTarget = false;
            return rect;
        }

        private static Sprite CreateCircleSprite(out Texture2D texture)
        {
            const int size = 64;
            texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Gameplay Minimap Circle",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave,
            };

            var pixels = new Color32[size * size];
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float radius = size * 0.5f - 1f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    byte alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(radius + 0.75f - distance) * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }
            texture.SetPixels32(pixels);
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f), size, 0, SpriteMeshType.Tight, Vector4.zero);
            texture.Apply(false, true);
            return sprite;
        }

        private static void Encapsulate(Vector3 point, ref bool hasBounds,
            ref float minX, ref float maxX, ref float minZ, ref float maxZ)
        {
            if (!hasBounds)
            {
                minX = maxX = point.x;
                minZ = maxZ = point.z;
                hasBounds = true;
                return;
            }
            minX = Mathf.Min(minX, point.x);
            maxX = Mathf.Max(maxX, point.x);
            minZ = Mathf.Min(minZ, point.z);
            maxZ = Mathf.Max(maxZ, point.z);
        }

        private void OnDestroy()
        {
            if (_circleSprite != null) Destroy(_circleSprite);
            if (_circleTexture != null) Destroy(_circleTexture);
        }
    }
}
