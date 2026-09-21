using Lilo.Config;
using Lilo.Systems.Movement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lilo.MonoBehaviours.Input
{
    /// <summary>
    /// Static virtual joystick following Apple HIG for iOS touch controls.
    /// - Fixed position at configured anchor point
    /// - 44pt minimum touch target
    /// - Semi-transparent when idle, brightens on press (visual press state)
    /// - Fades out after release
    /// - Sprint is combined into the joystick pull distance (pull far = sprint)
    /// - Circle sprite applied automatically at runtime
    /// </summary>
    public class JoystickInputAdapter : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;
        [SerializeField] private float radius = 80f;

        private Vector2 _rawOffset;
        private bool _active;

        private static Sprite _bgCircleSprite;
        private static Sprite _handleCircleSprite;
        private Image _bgImage;
        private Image _handleImage;
        private Canvas _parentCanvas;
        private float _idleOpacity;
        private float _pressOpacity = 0.85f;
        private float _fadeSpeed = 4f;
        private float _fadeTimer;
        private bool _fading;

        private const float MinTouchTargetPt = 44f;
        private const float ScreenMargin = 16f;

        private void Awake()
        {
            EnsureSprites();
            CacheImages();
            ApplyCircleShape();
            ApplyPresentation();
        }

        private void Update()
        {
            if (!_active && _fading)
            {
                _fadeTimer += Time.deltaTime * _fadeSpeed;
                float t = Mathf.Clamp01(_fadeTimer);
                if (_bgImage != null)
                {
                    Color c = _bgImage.color;
                    c.a = Mathf.Lerp(_pressOpacity * 0.5f, _idleOpacity * 0.5f, t);
                    _bgImage.color = c;
                }
                if (_handleImage != null)
                {
                    Color c = _handleImage.color;
                    c.a = Mathf.Lerp(_pressOpacity, _idleOpacity, t);
                    _handleImage.color = c;
                }
                if (t >= 1f) _fading = false;
            }
        }

        private static void EnsureSprites()
        {
            if (_bgCircleSprite != null && _handleCircleSprite != null) return;

            // High-res anti-aliased circle for background ring
            _bgCircleSprite = CreateCircleSprite(256, 0.92f, 0.04f);
            // Solid circle for handle
            _handleCircleSprite = CreateCircleSprite(128, 1f, 0f);
        }

        private static Sprite CreateCircleSprite(int resolution, float edgeFade, float ringThickness)
        {
            int size = resolution;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size * 0.5f;
            float outerR = center - 1f;

            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center + 0.5f;
                    float dy = y - center + 0.5f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy) / outerR;

                    float alpha;
                    if (ringThickness > 0f)
                    {
                        // Ring: smooth inner/outer edge
                        float innerEdge = edgeFade - ringThickness;
                        alpha = 1f - Mathf.InverseLerp(innerEdge, edgeFade, dist);
                        alpha = Mathf.Clamp01(alpha);
                        // Anti-alias the edges
                        float aa = 1.5f / outerR;
                        alpha *= Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(1f + aa, 1f - aa, dist));
                        alpha *= Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(innerEdge - aa, innerEdge + aa, dist));
                    }
                    else
                    {
                        // Solid circle with anti-aliased edge
                        float aa = 1.5f / outerR;
                        alpha = Mathf.SmoothStep(1f, 0f, Mathf.InverseLerp(1f - aa, 1f + aa, dist));
                    }

                    pixels[y * size + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private void CacheImages()
        {
            if (background != null) _bgImage = background.GetComponent<Image>();
            if (handle != null) _handleImage = handle.GetComponent<Image>();
            _parentCanvas = GetComponentInParent<Canvas>();
        }

        private void ApplyCircleShape()
        {
            if (_bgImage != null)
            {
                _bgImage.sprite = _bgCircleSprite;
                _bgImage.type = Image.Type.Simple;
                _bgImage.raycastTarget = true;
            }
            if (_handleImage != null)
            {
                _handleImage.sprite = _handleCircleSprite;
                _handleImage.type = Image.Type.Simple;
            }
        }

        /// <summary>
        /// Sizes the joystick from GameConfig and enforces Apple HIG minimums.
        /// joystickDiameter is a screen-height fraction (0–1), so it scales on every device.
        /// </summary>
        public void ApplyPresentation()
        {
            GameConfig cfg = config != null ? config : GameManager.Instance?.Config;
            if (cfg == null)
                return;
            config = cfg;

            // joystickDiameter is a fraction of screen height (0.35 = 35%).
            float fraction = cfg.joystickDiameter > 0f ? cfg.joystickDiameter : 0.35f;
            float screenHeight = Screen.height;
            float diameter = screenHeight * fraction;

            // Apple HIG: minimum 44pt touch target. At ~2x scale, 44pt ≈ 88px.
            float minDiameter = MinTouchTargetPt * 2f;
            diameter = Mathf.Max(diameter, minDiameter);

            radius = diameter * 0.5f;

            if (background != null)
            {
                background.sizeDelta = new Vector2(diameter, diameter);
                // Keep joystick fully visible — offset from bottom-left edge by half diameter + margin
                background.anchoredPosition = new Vector2(radius + ScreenMargin, radius + ScreenMargin)
                    + cfg.joystickCenterOffset;
            }
            if (handle != null)
            {
                handle.sizeDelta = new Vector2(diameter, diameter) * 0.45f;
            }

            // Apple HIG: semi-transparent so controls don't obscure gameplay.
            _idleOpacity = cfg.joystickOpacity > 0f ? cfg.joystickOpacity : 0.35f;

            if (_bgImage != null)
            {
                Color c = _bgImage.color;
                c.a = _idleOpacity * 0.5f;
                _bgImage.color = c;
            }
            if (_handleImage != null)
            {
                Color c = _handleImage.color;
                c.a = _idleOpacity;
                _handleImage.color = c;
            }
        }

        public MovementInput CurrentInput
        {
            get
            {
                if (!_active || _rawOffset == Vector2.zero)
                {
                    return MovementInput.Zero;
                }

                float magnitude = Mathf.Clamp01(_rawOffset.magnitude / radius);
                return new MovementInput(_rawOffset, magnitude);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _active = true;
            _fading = false;
            _fadeTimer = 0f;

            // Apple HIG: visual press state — brighten on touch.
            if (_bgImage != null)
            {
                Color c = _bgImage.color;
                c.a = _pressOpacity * 0.5f;
                _bgImage.color = c;
            }
            if (_handleImage != null)
            {
                Color c = _handleImage.color;
                c.a = _pressOpacity;
                _handleImage.color = c;
            }

            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (background == null)
                return;

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background, eventData.position, eventData.pressEventCamera, out localPoint);
            _rawOffset = localPoint;

            if (handle != null)
            {
                Vector2 clamped = Vector2.ClampMagnitude(_rawOffset, radius);
                handle.anchoredPosition = clamped;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _active = false;
            _rawOffset = Vector2.zero;
            _fading = true;
            _fadeTimer = 0f;

            if (handle != null)
            {
                handle.anchoredPosition = Vector2.zero;
            }
        }
    }
}
