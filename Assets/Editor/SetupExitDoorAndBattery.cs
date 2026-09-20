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
    /// Wires ExitDoorInteraction and BatteryPickup to the existing scene placeholders.
    /// Idempotent — safe to run multiple times. Run via LILO/Setup Exit Door & Battery.
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

            // Ensure PlayerCharacter has "Player" tag and a kinematic Rigidbody
            // (required for OnTriggerEnter to fire on iOS/Android).
            var player = GameObject.Find("PlayerCharacter");
            if (player != null)
            {
                if (!player.CompareTag("Player"))
                {
                    player.tag = "Player";
                    Debug.Log("[ExitDoorBatterySetup] Set PlayerCharacter tag to 'Player'.");
                }

                var rb = player.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = player.AddComponent<Rigidbody>();
                    rb.isKinematic = true;
                    rb.useGravity = false;
                    Debug.Log("[ExitDoorBatterySetup] Added kinematic Rigidbody to PlayerCharacter for trigger detection.");
                }
            }

            // --- Exit Door ---
            var door = GameObject.Find("ExitDoorPlaceholder");
            if (door == null)
            {
                Debug.LogError("[ExitDoorBatterySetup] 'ExitDoorPlaceholder' not found — run LILO/Setup Monster Arena first.");
                return;
            }

            // BoxCollider must be trigger for OnTriggerEnter to fire.
            var doorCollider = door.GetComponent<BoxCollider>();
            if (doorCollider != null && !doorCollider.isTrigger)
            {
                doorCollider.isTrigger = true;
                Debug.Log("[ExitDoorBatterySetup] Set ExitDoor BoxCollider.isTrigger = true.");
            }

            // NavMeshObstacle blocks the monster from walking through the door.
            var obstacle = door.GetComponent<NavMeshObstacle>();
            if (obstacle == null)
                obstacle = door.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.carving = true;
            obstacle.center = doorCollider != null ? doorCollider.center : Vector3.zero;
            obstacle.size = doorCollider != null ? doorCollider.size : new Vector3(1f, 1f, 1f);

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

            // BatteryBoxCollider must be trigger.
            var batteryCollider = battery.GetComponent<Collider>();
            if (batteryCollider != null && !batteryCollider.isTrigger)
            {
                batteryCollider.isTrigger = true;
                Debug.Log("[ExitDoorBatterySetup] Set Battery BoxCollider.isTrigger = true.");
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
            Debug.Log("[ExitDoorBatterySetup] Done. Walk into door/battery to interact (trigger-based, iOS-ready).");
        }
    }
}
