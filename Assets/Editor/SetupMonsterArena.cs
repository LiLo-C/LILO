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
        private const string ScenePath = "Assets/Scenes/DevTest_Movement.unity";
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
            EnsureArenaProps();

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

            // The desk is the obstacle loop the player can circle. It carves the
            // runtime NavMesh because its visual is a SpriteRenderer, not a bake mesh.
            var desk = GameObject.Find("Desk");
            if (desk != null)
            {
                var obstacle = desk.GetComponent<NavMeshObstacle>();
                if (obstacle == null) obstacle = desk.AddComponent<NavMeshObstacle>();
                obstacle.shape = NavMeshObstacleShape.Box;
                var deskCollider = desk.GetComponent<BoxCollider>();
                obstacle.center = deskCollider != null ? deskCollider.center : Vector3.zero;
                obstacle.size = deskCollider != null ? deskCollider.size : new Vector3(2f, 1f, 1f);
                obstacle.carving = true;
            }

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
            so.FindProperty("floorProfile").enumValueIndex = (int)FloorId.Floor51;
            so.FindProperty("spawnObjectives").objectReferenceValue = GameObject.Find("MonsterSpawnObjectives")?.transform;
            so.ApplyModifiedPropertiesWithoutUndo();

            surface.BuildNavMesh();
            EditorSceneManager.MarkSceneDirty(placeholder.scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[ArenaSetup] Done. Run LILO/Setup Eggy Monster first if visuals are missing.");
        }

        public static void RunBatch()
        {
            EditorSettings.serializationMode = SerializationMode.ForceText;
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Run();
        }

        public static void ValidateBatch()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            string[] required =
            {
                "MonsterPlaceholder",
                "MonsterPlaceholderVisual",
                "MonsterPatrolRoute",
                "MonsterSpawns",
                "MonsterSpawnObjectives",
                "ExitDoorPlaceholder",
                "BatteryPlaceholder",
                "ArenaObstaclePlaceholder",
                "NavMesh",
            };
            foreach (string name in required)
            {
                if (GameObject.Find(name) == null)
                    throw new System.InvalidOperationException($"Monster arena is missing '{name}'.");
            }

            var route = GameObject.Find("MonsterPatrolRoute").transform;
            var spawns = GameObject.Find("MonsterSpawns").transform;
            if (route.childCount < 4 || spawns.childCount < 2)
                throw new System.InvalidOperationException("Monster arena requires 4 patrol points and 2 spawn presets.");

            Debug.Log("[ArenaValidation] Scene opened with visible monster, 4 patrol points, 2 spawns, obstacle, door, battery, and objective marker.");
        }

        private static void EnsureArenaProps()
        {
            Material doorMaterial = EnsureMaterial(
                "Assets/Materials/MonsterArenaDoor.mat",
                new Color(0.2f, 0.45f, 0.8f));
            Material batteryMaterial = EnsureMaterial(
                "Assets/Materials/MonsterArenaBattery.mat",
                new Color(0.95f, 0.75f, 0.12f));

            GameObject door = EnsurePrimitive(
                "ExitDoorPlaceholder",
                PrimitiveType.Cube,
                new Vector3(-9.4f, 1f, 0f),
                new Vector3(0.35f, 2f, 2.5f),
                doorMaterial);
            GameObject battery = EnsurePrimitive(
                "BatteryPlaceholder",
                PrimitiveType.Cylinder,
                new Vector3(-6f, 0.3f, -2f),
                new Vector3(0.3f, 0.3f, 0.3f),
                batteryMaterial);

            IgnoreFromNavMeshBake(door);
            IgnoreFromNavMeshBake(battery);

            var objectives = GameObject.Find("MonsterSpawnObjectives");
            if (objectives == null) objectives = new GameObject("MonsterSpawnObjectives");
            var exitObjective = objectives.transform.Find("ExitDoorObjective");
            if (exitObjective == null)
            {
                exitObjective = new GameObject("ExitDoorObjective").transform;
                exitObjective.SetParent(objectives.transform, false);
            }
            exitObjective.position = door.transform.position;
        }

        private static GameObject EnsurePrimitive(
            string name,
            PrimitiveType type,
            Vector3 position,
            Vector3 scale,
            Material material)
        {
            var go = GameObject.Find(name);
            if (go == null)
            {
                go = GameObject.CreatePrimitive(type);
                go.name = name;
            }
            go.transform.position = position;
            go.transform.localScale = scale;
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;
            return go;
        }

        private static void IgnoreFromNavMeshBake(GameObject go)
        {
            var modifier = go.GetComponent<NavMeshModifier>();
            if (modifier == null) modifier = go.AddComponent<NavMeshModifier>();
            modifier.ignoreFromBuild = true;
        }

        private static Material EnsureMaterial(string path, Color color)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            material = new Material(shader) { name = System.IO.Path.GetFileNameWithoutExtension(path) };
            material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.color = color;
            AssetDatabase.CreateAsset(material, path);
            return material;
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
