using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lilo.MonoBehaviours.UI
{
    /// <summary>Plays shared UI feedback for buttons and other selectable controls.</summary>
    [DisallowMultipleComponent]
    public sealed class UiButtonClickHook : MonoBehaviour, IPointerClickHandler, ISubmitHandler
    {
        private Button _button;
        private Selectable _selectable;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _selectable = GetComponent<Selectable>();

            if (_button != null)
                _button.onClick.AddListener(PlayClick);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_button == null && _selectable != null && _selectable.IsInteractable())
                PlayClick();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (_button == null && _selectable != null && _selectable.IsInteractable())
                PlayClick();
        }

        private void PlayClick()
        {
            UiClickFeedback.Instance?.PlayClick();
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(PlayClick);
        }
    }
}
