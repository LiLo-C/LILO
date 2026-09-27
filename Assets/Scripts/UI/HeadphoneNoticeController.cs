using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Lilo.UI
{
    /// <summary>Shows the audio recommendation once at app launch, before MainMenu.</summary>
    [DisallowMultipleComponent]
    public sealed class HeadphoneNoticeController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup content;
        [SerializeField] private string nextSceneName = "MainMenu";
        [SerializeField, Min(0f)] private float fadeSeconds = 0.55f;

        private bool _ready;
        private bool _transitioning;

        private void Awake()
        {
            if (content != null)
                content.alpha = 0f;
        }

        private IEnumerator Start()
        {
            yield return FadeTo(1f);
            _ready = true;
        }

        private void Update()
        {
            if (!_ready || _transitioning) return;

            bool tapped = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
            bool clicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            bool keyed = Keyboard.current != null
                && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame);
            if (tapped || clicked || keyed)
                Continue();
        }

        public void Continue()
        {
            if (!_ready || _transitioning) return;
            if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
            {
                Debug.LogError($"[HeadphoneNotice] Scene '{nextSceneName}' is missing from the build profile.", this);
                return;
            }

            StartCoroutine(TransitionToMenu());
        }

        private IEnumerator TransitionToMenu()
        {
            _transitioning = true;
            yield return FadeTo(0f);
            SceneManager.LoadScene(nextSceneName);
        }

        private IEnumerator FadeTo(float target)
        {
            if (content == null) yield break;
            if (fadeSeconds <= 0f)
            {
                content.alpha = target;
                yield break;
            }

            float start = content.alpha;
            float elapsed = 0f;
            while (elapsed < fadeSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                content.alpha = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / fadeSeconds));
                yield return null;
            }
            content.alpha = target;
        }
    }
}
