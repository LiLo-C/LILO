using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using Lilo.Config;
using Lilo.MonoBehaviours.Interaction;
using Lilo.MonoBehaviours.Monster;

namespace Lilo.Editor
{
    /// <summary>
    /// Wires ExitDoorInteraction, BatteryPickup, and NavMeshObstacle to the existing
    /// scene placeholders. Idempotent — safe to run multiple times.
    /// Run via LILO/Setup Exit Door & Battery.
    /// </summary>
    public static class SetupExitDoorAndBattery
    {
        [MenuItem("LILO/Setup Exit Door & Battery")]
        public static void Run()
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/Config/GameConfig.asset");
            if (config == null)
            {
                Debug.LogError("[ExitDoorBatterySetup] Assets/Config/GameConfig.asset not found.");
                return;
            }

            // --- Exit Door ---
            var door = GameObject.Find("ExitDoorPlaceholder");
            if (door == null)
            {
                Debug.LogError("[ExitDoorBatterySetup] 'ExitDoorPlaceholder' not found — run LILO/Setup Monster Arena first.");
                return;
            }

            // NavMeshObstacle blocks the monster from walking through the door.
            var obstacle = door.GetComponent<NavMeshObstacle>();
            if (obstacle == null)
                obstacle = door.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.carving = true;

            var doorInteraction = door.GetComponent<ExitDoorInteraction>();
            if (doorInteraction == null)
                doorInteraction = door.AddComponent<ExitDoorInteraction>();

            // --- Battery ---
            var battery = GameObject.Find("BatteryPlaceholder");
            if (battery == null)
            {
                Debug.LogError("[ExitDoorBatterySetup] 'BatteryPlaceholder' not found — run LILO/Setup Monster Arena first.");
                return;
            }

            var batteryPickup = battery.GetComponent<BatteryPickup>();
            if (batteryPickup == null)
                batteryPickup = battery.AddComponent<BatteryPickup>();

            var monsterGo = GameObject.Find("MonsterPlaceholder");
            var monster = monsterGo != null ? monsterGo.GetComponent<MonsterAIController>() : null;
            var batterySO = new SerializedObject(batteryPickup);
            batterySO.FindProperty("config").objectReferenceValue = config;
            batterySO.FindProperty("interactRadius").floatValue = 2f;
            batterySO.FindProperty("monster").objectReferenceValue = monster;
            batterySO.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(door.scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[ExitDoorBatterySetup] Done. Walk into door/battery to interact (distance-based, iOS-ready).");
        }
    }
}
