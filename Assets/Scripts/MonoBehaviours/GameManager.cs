using Lilo.Config;
using Lilo.State;
using UnityEngine;

namespace Lilo.MonoBehaviours
{
    /// <summary>
    /// The sole owner of GameState. Created once in Bootstrap, persists via DontDestroyOnLoad
    /// across every later scene. A second instance appearing anywhere fails visibly (spec 002 FR-002/FR-005).
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private GameConfig config;
        [Header("Optional standalone scene bootstrap")]
        [SerializeField] private bool useConfiguredStartingFloor;
        [SerializeField] private FloorId configuredStartingFloor = FloorId.Floor52;

        public GameConfig Config => config;
        public GameState State { get; private set; }

        public void BeginNewRun()
        {
            if (config == null)
            {
                Debug.LogError("[GameManager] Cannot start a run without a GameConfig.");
                return;
            }

            if (State == null)
                State = new GameState(config);
            else
                State.StartNewRun(config);
            GameplayGuidanceHud.ResetRunVoiceOverState();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                if (useConfiguredStartingFloor && Instance.State != null)
                    Instance.State.AdvanceToFloor(configuredStartingFloor);
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            GameConfigLoader.LoadAndValidate(config);
            State = new GameState(config);
            GameplayGuidanceHud.ResetRunVoiceOverState();
            if (useConfiguredStartingFloor)
                State.AdvanceToFloor(configuredStartingFloor);
        }
    }
}
