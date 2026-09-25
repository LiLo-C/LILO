using Lilo.MonoBehaviours;
using Lilo.State;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Lilo.UI
{
    public sealed class EndingController : MonoBehaviour
    {
        [SerializeField] private RunOutcome outcome;
        [SerializeField] private Text title;
        [SerializeField] private Text message;
        [SerializeField] private Button mainMenuButton;

        private void Awake()
        {
            if (title != null)
                title.text = outcome == RunOutcome.GoodEnding ? "HAPPY ENDING" : "SAD ENDING";
            if (message != null)
            {
                if (outcome == RunOutcome.GoodEnding)
                    message.text = "You made it through the office. The light is still yours.";
                else
                {
                    string lastLifeMessage = GameManager.Instance?.State?.RespawnMessage;
                    message.text = !string.IsNullOrWhiteSpace(lastLifeMessage)
                        ? lastLifeMessage
                        : "The darkness took the last chance. Try again.";
                }
            }
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }

        public void ReturnToMainMenu() => SceneManager.LoadScene("MainMenu");
    }
}
