using Lilo.Config;
using Lilo.State;
using UnityEngine;

namespace Lilo.MonoBehaviours
{
    /// <summary>
    /// The sole owner of GameState. Created once in Bootstrap, persists via DontDestroyOnLoad
    /// across every later scene. A second instance appearing anywhere fails visibly (spec 002 FR-002/FR-005).
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private GameConfig config;

        public GameConfig Config => config;
        public GameState State { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError("Duplicate GameManager detected — destroying the new instance. Exactly one must exist (spec 002 FR-002/FR-005).");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            GameConfigLoader.LoadAndValidate(config);
            State = new GameState(config);
        }
    }
}
