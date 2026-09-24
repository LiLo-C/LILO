using UnityEngine;
using UnityEngine.UI;

namespace Lilo.MonoBehaviours.UI
{
    /// <summary>
    /// Keeps screen-space UI inside the device safe area and scales layouts evenly
    /// across phone and tablet aspect ratios.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ResponsiveCanvasAdapter : MonoBehaviour
    {
        private Canvas _canvas;
        private CanvasScaler _scaler;
        private RectTransform _rectTransform;
        private Rect _lastSafeArea;
        private int _lastScreenWidth;
        private int _lastScreenHeight;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _scaler = GetComponent<CanvasScaler>();
            _rectTransform = transform as RectTransform;

            if (_scaler != null && _scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                _scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                _scaler.matchWidthOrHeight = 0.5f;
            }

            ApplySafeArea();
        }

        private void Update()
        {
            if (Screen.width == _lastScreenWidth
                && Screen.height == _lastScreenHeight
                && Screen.safeArea == _lastSafeArea)
                return;

            ApplySafeArea();
        }

        private void ApplySafeArea()
        {
            if (_canvas == null || !_canvas.isRootCanvas || _rectTransform == null
                || _canvas.renderMode == RenderMode.WorldSpace)
                return;

            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;
            _lastSafeArea = Screen.safeArea;

            float width = Mathf.Max(1, Screen.width);
            float height = Mathf.Max(1, Screen.height);
            Rect safeArea = Screen.safeArea;
            Vector2 min = new Vector2(safeArea.xMin / width, safeArea.yMin / height);
            Vector2 max = new Vector2(safeArea.xMax / width, safeArea.yMax / height);

            _rectTransform.anchorMin = min;
            _rectTransform.anchorMax = max;
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;
        }
    }
}
