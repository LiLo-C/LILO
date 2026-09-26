using System.Collections;
using UnityEngine;

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
        private RectTransform _rectTransform;
        private Rect _lastSafeArea;
        private int _lastScreenWidth;
        private int _lastScreenHeight;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _rectTransform = transform as RectTransform;

            ApplySafeArea();
            StartCoroutine(RefreshSafeAreaAfterCanvasLayout());
        }

        private IEnumerator RefreshSafeAreaAfterCanvasLayout()
        {
            // iOS can report its final safe area after the first scene frame,
            // especially on iPad during cold launch / orientation settling.
            yield return null;
            yield return new WaitForEndOfFrame();
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
