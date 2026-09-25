using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Lilo.MonoBehaviours.Audio;
using Lilo.MonoBehaviours.Player;
using Lilo.MonoBehaviours.Monster;

namespace Lilo.Editor
{
    /// <summary>
    /// One-shot wiring: creates SfxController, assigns clips, and hooks it to
    /// PlayerMovementController and MonsterAIController in the active scene.
    /// Run via menu LILO/Setup Sfx.
    /// </summary>
    public static class SetupSfx
    {
        private const string DevTestScenePath = "Assets/Scenes/DevTest_Movement.unity";
        private const string BehindYouPath = "Assets/Sfx/behind-you.mp3";
        private const string HorrorChasePath = "Assets/Sfx/horror-chase.mp3";
        private const string PlayerCaughtPath = "Assets/Sfx/player-caught.mp3";

        /// <summary>
        /// Batch-mode entry point used to apply the same idempotent setup to the development
        /// scene and persist it. The regular menu command below remains available for any scene.
        /// </summary>
        public static void RunDevTestScene()
        {
            var scene = EditorSceneManager.OpenScene(DevTestScenePath, OpenSceneMode.Single);
            Run();
            EditorSceneManager.SaveScene(scene);
        }

        [MenuItem("LILO/Setup Sfx")]
        public static void Run()
        {
            // 1. Load clips.
            var behindYou = AssetDatabase.LoadAssetAtPath<AudioClip>(BehindYouPath);
            if (behindYou == null)
            {
                Debug.LogError($"[SfxSetup] Clip not found at {BehindYouPath}");
                return;
            }
            var horrorChase = AssetDatabase.LoadAssetAtPath<AudioClip>(HorrorChasePath);
            if (horrorChase == null)
            {
                Debug.LogError($"[SfxSetup] Clip not found at {HorrorChasePath}");
                return;
            }
            var playerCaught = AssetDatabase.LoadAssetAtPath<AudioClip>(PlayerCaughtPath);
            if (playerCaught == null)
            {
                Debug.LogError($"[SfxSetup] Clip not found at {PlayerCaughtPath}");
                return;
            }

            // 2. Find or create SfxController GameObject.
            var sfxGo = GameObject.Find("SfxController");
            if (sfxGo == null)
                sfxGo = new GameObject("SfxController");
            var sfx = sfxGo.GetComponent<SfxController>();
            if (sfx == null)
                sfx = sfxGo.AddComponent<SfxController>();

            // 3. Assign clips via SerializedObject.
            var so = new SerializedObject(sfx);
            so.FindProperty("behindYouClip").objectReferenceValue = behindYou;
            so.FindProperty("horrorChaseClip").objectReferenceValue = horrorChase;
            so.FindProperty("playerCaughtClip").objectReferenceValue = playerCaught;
            so.ApplyModifiedPropertiesWithoutUndo();

            // 4. Wire to PlayerMovementController.
            var player = GameObject.Find("PlayerCharacter");
            if (player != null)
            {
                var pmc = player.GetComponent<PlayerMovementController>();
                if (pmc != null)
                {
                    var pmcSo = new SerializedObject(pmc);
                    pmcSo.FindProperty("sfx").objectReferenceValue = sfx;
                    pmcSo.ApplyModifiedPropertiesWithoutUndo();
                }
                else
                {
                    Debug.LogWarning("[SfxSetup] PlayerCharacter has no PlayerMovementController.");
                }
            }
            else
            {
                Debug.LogWarning("[SfxSetup] PlayerCharacter not found.");
            }

            // 5. Wire to MonsterAIController.
            var monster = GameObject.Find("MonsterPlaceholder");
            if (monster != null)
            {
                var mac = monster.GetComponent<MonsterAIController>();
                if (mac != null)
                {
                    var macSo = new SerializedObject(mac);
                    macSo.FindProperty("sfx").objectReferenceValue = sfx;
                    macSo.ApplyModifiedPropertiesWithoutUndo();
                }
                else
                {
                    Debug.LogWarning("[SfxSetup] MonsterPlaceholder has no MonsterAIController.");
                }
            }
            else
            {
                Debug.LogWarning("[SfxSetup] MonsterPlaceholder not found.");
            }

            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log("[SfxSetup] Done. behind-you on first move, horror-chase on Chase, player-caught on catch.");
        }

        [MenuItem("LILO/Assign Player Caught Audio To Office Floors")]
        public static void AssignPlayerCaughtAudioToOfficeFloors()
        {
            var playerCaught = AssetDatabase.LoadAssetAtPath<AudioClip>(PlayerCaughtPath);
            if (playerCaught == null)
            {
                Debug.LogError($"[SfxSetup] Clip not found at {PlayerCaughtPath}");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            string returnScenePath = EditorSceneManager.GetActiveScene().path;
            int assigned = 0;
            foreach (string sceneName in new[] { "OfficeLevel1", "OfficeLevel2", "OfficeLevel3" })
            {
                var scene = EditorSceneManager.OpenScene($"Assets/Scenes/{sceneName}.unity", OpenSceneMode.Single);
                var sfx = GameObject.Find("SfxController")?.GetComponent<SfxController>();
                if (sfx == null)
                {
                    Debug.LogWarning($"[SfxSetup] {sceneName} has no SfxController.");
                    continue;
                }

                var serialized = new SerializedObject(sfx);
                serialized.FindProperty("playerCaughtClip").objectReferenceValue = playerCaught;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(sfx);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                assigned++;
            }

            EditorSceneManager.OpenScene(string.IsNullOrEmpty(returnScenePath)
                ? "Assets/Scenes/OfficeLevel1.unity"
                : returnScenePath, OpenSceneMode.Single);
            Debug.Log($"[SfxSetup] Assigned player-caught.mp3 in {assigned} office floor scene(s).");
        }
    }
}
