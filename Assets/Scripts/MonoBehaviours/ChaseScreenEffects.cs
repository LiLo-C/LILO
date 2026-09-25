using Lilo.MonoBehaviours.Monster;
using UnityEngine;
using UnityEngine.UI;

namespace Lilo.UI
{
    /// <summary>Red chase frame and additive camera shake for office gameplay.</summary>
    [DisallowMultipleComponent]
    public sealed class ChaseScreenEffects : MonoBehaviour
    {
        private const float ImpactDuration = 0.42f;
        private const float ImpactShake = 0.22f;
        private const float SustainedShake = 0.035f;
        private const float BorderAlpha = 0.72f;
        private const float BorderThicknessPixels = 16f;

        private readonly Image[] _edges = new Image[4];
        private static Sprite _solidWhiteSprite;
        private MonsterAIController _monster;
        private UnityEngine.Camera _camera;
        private bool _wasChasing;
        private float _impactShakeRemaining;
        private float _borderAlpha;

        public Vector3 CameraPositionOffset { get; private set; }

        private void Awake()
        {
            CreateEdge("ChaseFrameTop", new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, BorderThicknessPixels));
            CreateEdge("ChaseFrameBottom", Vector2.zero, new Vector2(1f, 0f),
                new Vector2(0.5f, 0f), Vector2.zero, new Vector2(0f, BorderThicknessPixels));
            CreateEdge("ChaseFrameLeft", Vector2.zero, new Vector2(0f, 1f),
                new Vector2(0f, 0.5f), Vector2.zero, new Vector2(BorderThicknessPixels, 0f));
            CreateEdge("ChaseFrameRight", new Vector2(1f, 0f), Vector2.one,
                new Vector2(1f, 0.5f), Vector2.zero, new Vector2(BorderThicknessPixels, 0f));
        }

        private void Update()
        {
            if (_monster == null)
                _monster = FindAnyObjectByType<MonsterAIController>();
            if (_camera == null)
                _camera = UnityEngine.Camera.main;

            bool chasing = _monster != null && _monster.CurrentState == Lilo.State.MonsterState.Chase;
            if (chasing && !_wasChasing)
                _impactShakeRemaining = ImpactDuration;
            _wasChasing = chasing;

            float dt = Time.unscaledDeltaTime;
            _impactShakeRemaining = Mathf.Max(0f, _impactShakeRemaining - dt);
            float impact = ImpactShake * Mathf.SmoothStep(0f, 1f, _impactShakeRemaining / ImpactDuration);
            float shakeStrength = (chasing ? SustainedShake : 0f) + impact;
            float t = Time.unscaledTime * 28f;
            Vector2 noise = new Vector2(
                Mathf.PerlinNoise(t, 0.37f) - 0.5f,
                Mathf.PerlinNoise(0.73f, t) - 0.5f) * 2f;
            CameraPositionOffset = _camera != null
                ? (_camera.transform.right * noise.x + _camera.transform.up * noise.y) * shakeStrength
                : Vector3.zero;

            _borderAlpha = Mathf.MoveTowards(_borderAlpha,
                chasing ? BorderAlpha : 0f, dt * (chasing ? 7f : 4f));
            float scale = Mathf.Max(0.01f, GetComponentInParent<Canvas>()?.scaleFactor ?? 1f);
            SetBorderSizes(scale);
            Color color = new Color(0.95f, 0.035f, 0.075f, _borderAlpha);
            foreach (Image edge in _edges)
            {
                if (edge == null) continue;
                edge.color = color;
                edge.enabled = _borderAlpha > 0.005f;
            }
        }

        private void CreateEdge(string objectName, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 position, Vector2 size)
        {
            var edgeObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            edgeObject.transform.SetParent(transform, false);
            RectTransform rect = (RectTransform)edgeObject.transform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Image image = edgeObject.GetComponent<Image>();
            if (_solidWhiteSprite == null)
                _solidWhiteSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f),
                    new Vector2(0.5f, 0.5f));
            image.sprite = _solidWhiteSprite;
            image.color = new Color(0.95f, 0.035f, 0.075f, 0f);
            image.raycastTarget = false;
            image.enabled = false;
            _edges[objectName.EndsWith("Top") ? 0 : objectName.EndsWith("Bottom") ? 1 :
                objectName.EndsWith("Left") ? 2 : 3] = image;
            edgeObject.transform.SetAsLastSibling();
        }

        private void SetBorderSizes(float scale)
        {
            float thickness = BorderThicknessPixels / scale;
            if (_edges[0] != null) _edges[0].rectTransform.sizeDelta = new Vector2(0f, thickness);
            if (_edges[1] != null) _edges[1].rectTransform.sizeDelta = new Vector2(0f, thickness);
            if (_edges[2] != null) _edges[2].rectTransform.sizeDelta = new Vector2(thickness, 0f);
            if (_edges[3] != null) _edges[3].rectTransform.sizeDelta = new Vector2(thickness, 0f);
        }
    }
}
