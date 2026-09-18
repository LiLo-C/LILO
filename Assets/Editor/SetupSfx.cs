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
        private const string BehindYouPath = "Assets/Sfx/behind-you.mp3";
        private const string HorrorChasePath = "Assets/Sfx/horror-chase.mp3";

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
            Debug.Log("[SfxSetup] Done. behind-you on first move, horror-chase on Chase.");
        }
    }
}
