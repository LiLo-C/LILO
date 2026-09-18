using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using Lilo.Config;
using Lilo.MonoBehaviours.Monster;

namespace Lilo.Editor
{
    /// <summary>
    /// One-shot arena wiring for spec 002 in DevTest_Movement (idempotent):
    /// patrol route (4 waypoints), 2 monster spawn presets, NavMesh surface,
    /// feet-on-ground monster root, NavMeshAgent + MonsterAIController.
    /// Run via menu LILO/Setup Monster Arena. Calls SetupEggyMonster for visuals.
    /// </summary>
    public static class SetupMonsterArena
    {
        private static readonly Vector3[] Waypoints =
        {
            new Vector3(-7f, 0f, -7f),
            new Vector3(7f, 0f, -7f),
            new Vector3(7f, 0f, 7f),
            new Vector3(-7f, 0f, 7f),
        };

        private static readonly Vector3[] Spawns =
        {
            new Vector3(-5f, 0f, 5f),
            new Vector3(8f, 0f, 7f),
        };

        [MenuItem("LILO/Setup Monster Arena")]
        public static void Run()
        {
            var placeholder = GameObject.Find("MonsterPlaceholder");
            if (placeholder == null)
            {
                Debug.LogError("[ArenaSetup] 'MonsterPlaceholder' not found.");
                return;
            }

            EnsureChildPoints("MonsterPatrolRoute", "WP", Waypoints);
            EnsureChildPoints("MonsterSpawns", "Spawn", Spawns);

            // Feet-on-ground root for NavMeshAgent control (was capsule-center y=1).
            var rootPos = placeholder.transform.position;
            rootPos.y = 0f;
            placeholder.transform.position = rootPos;
            var capsule = placeholder.GetComponent<CapsuleCollider>();
            if (capsule != null)
            {
                capsule.center = new Vector3(0f, 1f, 0f);
                capsule.radius = 0.5f;
                capsule.height = 2f;
            }

            // Rebuild Eggy visuals aligned to the new root height.
            SetupEggyMonster.Run();

            // NavMesh surface (baked at startup by the controller if missing).
            var navGo = GameObject.Find("NavMesh");
            if (navGo == null)
            {
                navGo = new GameObject("NavMesh");
            }
            var surface = navGo.GetComponent<NavMeshSurface>();
            if (surface == null)
                surface = navGo.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.All;
            surface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
            surface.agentTypeID = 0;
            surface.defaultArea = 0;

            var agent = placeholder.GetComponent<NavMeshAgent>();
            if (agent == null)
                agent = placeholder.AddComponent<NavMeshAgent>();

            var controller = placeholder.GetComponent<MonsterAIController>();
            if (controller == null)
                controller = placeholder.AddComponent<MonsterAIController>();

            var gameConfig = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/Config/GameConfig.asset");
            if (gameConfig == null)
            {
                Debug.LogError("[ArenaSetup] Assets/Config/GameConfig.asset not found.");
                return;
            }
            var so = new SerializedObject(controller);
            so.FindProperty("config").objectReferenceValue = gameConfig;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(placeholder.scene);
            Debug.Log("[ArenaSetup] Done. Run LILO/Setup Eggy Monster first if visuals are missing.");
        }

        private static void EnsureChildPoints(string parentName, string prefix, Vector3[] positions)
        {
            var parent = GameObject.Find(parentName);
            if (parent == null)
                parent = new GameObject(parentName);
            for (int k = 0; k < positions.Length; k++)
            {
                string name = $"{prefix}_{k + 1}";
                var child = parent.transform.Find(name);
                if (child == null)
                {
                    child = new GameObject(name).transform;
                    child.SetParent(parent.transform, false);
                }
                child.position = positions[k];
            }
        }
    }
}
