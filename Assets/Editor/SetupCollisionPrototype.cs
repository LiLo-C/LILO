using System.Collections.Generic;
using Lilo.Config;
using Lilo.MonoBehaviours.Collision;
using Lilo.MonoBehaviours.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Lilo.Editor
{
    /// <summary>Idempotently wires placeholder walls/furniture into the movement test scene.</summary>
    public static class SetupCollisionPrototype
    {
        private const string ScenePath = "Assets/Scenes/DevTest_Movement.unity";

        private static readonly string[] ObstacleNames =
        {
            "Wall_West",
            "Wall_North",
            "Wall_South",
            "Wall_East",
            "Desk",
        };

        [MenuItem("LILO/Setup Collision Prototype")]
        public static void Run()
        {
            GameObject player = GameObject.Find("PlayerCharacter");
            GameConfig config = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/Config/GameConfig.asset");
            if (player == null || config == null)
            {
                Debug.LogError("[CollisionSetup] PlayerCharacter or GameConfig is missing.");
                return;
            }

            var authored = new List<CollisionObstacleAuthoring>();
            foreach (string obstacleName in ObstacleNames)
            {
                GameObject obstacle = GameObject.Find(obstacleName);
                if (obstacle == null || obstacle.GetComponent<BoxCollider>() == null)
                {
                    Debug.LogWarning($"[CollisionSetup] Skipping '{obstacleName}': no object/BoxCollider.");
                    continue;
                }

                var authoring = obstacle.GetComponent<CollisionObstacleAuthoring>();
                if (authoring == null)
                    authoring = obstacle.AddComponent<CollisionObstacleAuthoring>();
                authored.Add(authoring);
            }

            var controller = player.GetComponent<CharacterCollisionController>();
            if (controller == null)
                controller = player.AddComponent<CharacterCollisionController>();

            var serialized = new SerializedObject(controller);
            serialized.FindProperty("config").objectReferenceValue = config;
            SerializedProperty obstacleList = serialized.FindProperty("authoredObstacles");
            obstacleList.arraySize = authored.Count;
            for (int index = 0; index < authored.Count; index++)
                obstacleList.GetArrayElementAtIndex(index).objectReferenceValue = authored[index];
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(player.scene);
            Debug.Log($"[CollisionSetup] Wired {authored.Count} placeholder obstacles.");
        }

        public static void RunDevTestScene()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Run();
            EditorSceneManager.SaveOpenScenes();
        }
    }
}
