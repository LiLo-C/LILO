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
        private const string BatteryPickupPath = "Assets/Sfx/pickup-battery-sfx.wav";
        private const string AmbienceBedPath = "Assets/Sfx/ambience/ambience_sfx.wav";
        private const string PlayerCaughtPath = "Assets/Sfx/player-caught.mp3";
        private const string ChaseBgmPath = "Assets/Sfx/ambience/chase-refine.wav";
        private const string KeyPickupPath = "Assets/Sfx/ambience/key-pickup-sfx.mp3";
        private const string WaterDispenserPath = "Assets/Sfx/ambience/water-dispenser-sfx.wav";
        private const string ToiletFlushPath = "Assets/Sfx/ambience/toilet-flush-sfx.mp3";
        private const string Life2VoicePath = "Assets/Sfx/vos/Life-2-sfx.wav";
        private const string Life1VoicePath = "Assets/Sfx/vos/life-1-sfx.wav";
        private const string LastLifeVoicePath = "Assets/Sfx/vos/last-life-sfx.wav";
        private const string NeedAwayVoicePath = "Assets/Sfx/vos/need-away-sfx.wav";
        private const string BatteryRunsOutVoicePath = "Assets/Sfx/vos/battery-runs-out-sfx.wav";
        private const string NeedAccessKeyVoicePath = "Assets/Sfx/vos/need-access-key-sfx.wav";
        private static readonly string[] FootstepPaths =
        {
            "Assets/Sfx/walking/left-foot-sfx.wav", "Assets/Sfx/walking/right-foot-sfx.wav",
            "Assets/Sfx/walking/left-foot-2-sfx.wav", "Assets/Sfx/walking/right-foot-2-sfx.wav",
            "Assets/Sfx/walking/left-foot-3-sfx.wav", "Assets/Sfx/walking/right-foot-3-sfx.wav"
        };
        private static readonly string[] AmbienceStingPaths =
        {
            "Assets/Sfx/ambience/empty-room-sfx.mp3", "Assets/Sfx/ambience/hey-1-sfx.wav",
            "Assets/Sfx/ambience/hey-2-sfx.wav", "Assets/Sfx/ambience/smoking-sfx.wav",
            "Assets/Sfx/ambience/tape-sfx.wav", "Assets/Sfx/ambience/keyboard-sfx.wav"
        };

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
            var chaseBgm = AssetDatabase.LoadAssetAtPath<AudioClip>(ChaseBgmPath);
            if (chaseBgm == null)
            {
                // The previous loop remains available in local scenes while the
                // replacement is downloaded and imported from origin/main.
                chaseBgm = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sfx/horror-chase.mp3");
                if (chaseBgm == null)
                    Debug.LogWarning($"[SfxSetup] Clip not found at {ChaseBgmPath}; chase audio will stay silent.");
            }
            var keyPickup = AssetDatabase.LoadAssetAtPath<AudioClip>(KeyPickupPath);
            if (keyPickup == null)
            {
                Debug.LogError($"[SfxSetup] Clip not found at {KeyPickupPath}");
                return;
            }
            var waterDispenser = AssetDatabase.LoadAssetAtPath<AudioClip>(WaterDispenserPath);
            var toiletFlush = AssetDatabase.LoadAssetAtPath<AudioClip>(ToiletFlushPath);
            var batteryPickup = AssetDatabase.LoadAssetAtPath<AudioClip>(BatteryPickupPath);
            var ambienceBed = AssetDatabase.LoadAssetAtPath<AudioClip>(AmbienceBedPath);
            var playerCaught = AssetDatabase.LoadAssetAtPath<AudioClip>(PlayerCaughtPath);
            if (playerCaught == null)
            {
                Debug.LogError($"[SfxSetup] Clip not found at {PlayerCaughtPath}");
                return;
            }
            var life2Voice = AssetDatabase.LoadAssetAtPath<AudioClip>(Life2VoicePath);
            var life1Voice = AssetDatabase.LoadAssetAtPath<AudioClip>(Life1VoicePath);
            var lastLifeVoice = AssetDatabase.LoadAssetAtPath<AudioClip>(LastLifeVoicePath);
            var needAwayVoice = AssetDatabase.LoadAssetAtPath<AudioClip>(NeedAwayVoicePath);
            var batteryRunsOutVoice = AssetDatabase.LoadAssetAtPath<AudioClip>(BatteryRunsOutVoicePath);
            var needAccessKeyVoice = AssetDatabase.LoadAssetAtPath<AudioClip>(NeedAccessKeyVoicePath);

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
            so.FindProperty("playerCaughtClip").objectReferenceValue = playerCaught;
            if (chaseBgm != null)
                so.FindProperty("chaseBgmClip").objectReferenceValue = chaseBgm;
            so.FindProperty("keyPickupClip").objectReferenceValue = keyPickup;
            so.FindProperty("waterDispenserClip").objectReferenceValue = waterDispenser;
            so.FindProperty("toiletFlushClip").objectReferenceValue = toiletFlush;
            if (batteryPickup != null)
                so.FindProperty("batteryPickupClip").objectReferenceValue = batteryPickup;
            so.FindProperty("life2VoiceOverClip").objectReferenceValue = life2Voice;
            so.FindProperty("life1VoiceOverClip").objectReferenceValue = life1Voice;
            so.FindProperty("lastLifeVoiceOverClip").objectReferenceValue = lastLifeVoice;
            so.FindProperty("needAwayVoiceOverClip").objectReferenceValue = needAwayVoice;
            so.FindProperty("batteryRunsOutVoiceOverClip").objectReferenceValue = batteryRunsOutVoice;
            so.FindProperty("needAccessKeyVoiceOverClip").objectReferenceValue = needAccessKeyVoice;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Keep standalone Office floor launches audible as well as full runs
            // that carry the first floor's persistent ambience director forward.
            if (ambienceBed != null)
            {
                var ambienceGo = GameObject.Find("AmbienceDirector");
                if (ambienceGo == null)
                    ambienceGo = new GameObject("AmbienceDirector");
                var bedSource = ambienceGo.GetComponent<AudioSource>();
                if (bedSource == null)
                    bedSource = ambienceGo.AddComponent<AudioSource>();
                var ambience = ambienceGo.GetComponent<AmbienceDirector>();
                if (ambience == null)
                    ambience = ambienceGo.AddComponent<AmbienceDirector>();
                var ambienceSo = new SerializedObject(ambience);
                ambienceSo.FindProperty("roomTone").objectReferenceValue = ambienceBed;
                ambienceSo.FindProperty("bedSource").objectReferenceValue = bedSource;
                var stingClips = new System.Collections.Generic.List<AudioClip>();
                foreach (string path in AmbienceStingPaths)
                {
                    var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                    if (clip != null) stingClips.Add(clip);
                    else Debug.LogWarning($"[SfxSetup] Optional ambience clip missing: {path}");
                }
                var stings = ambienceSo.FindProperty("stings");
                stings.arraySize = stingClips.Count;
                for (int i = 0; i < stingClips.Count; i++)
                    stings.GetArrayElementAtIndex(i).objectReferenceValue = stingClips[i];
                ambienceSo.ApplyModifiedPropertiesWithoutUndo();
            }

            WireFootsteps();

            WirePropPassBySounds(sfx);

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
            Debug.Log("[SfxSetup] Done. Audio clips assigned and player/monster controllers wired.");
        }

        private static void WirePropPassBySounds(SfxController sfx)
        {
            foreach (GameObject candidate in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                string objectName = candidate.name.ToLowerInvariant();
                PropPassBySfx.SoundType soundType;
                if (objectName.Contains("dispenser"))
                    soundType = PropPassBySfx.SoundType.WaterDispenser;
                else if (objectName.Contains("toiletpartition"))
                    soundType = PropPassBySfx.SoundType.ToiletPartition;
                else
                    continue;

                Collider propCollider = candidate.GetComponent<Collider>();
                if (propCollider == null)
                {
                    Debug.LogWarning($"[SfxSetup] {candidate.name} has no collider for pass-by audio.");
                    continue;
                }

                var passBy = candidate.GetComponent<PropPassBySfx>();
                if (passBy == null)
                    passBy = candidate.AddComponent<PropPassBySfx>();

                var serialized = new SerializedObject(passBy);
                serialized.FindProperty("soundType").enumValueIndex = (int)soundType;
                serialized.FindProperty("proximityCollider").objectReferenceValue = propCollider;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(passBy);
            }
        }

        private static void WireFootsteps()
        {
            var player = GameObject.Find("PlayerCharacter");
            if (player == null) return;
            var footstepPlayer = player.GetComponent<FootstepPlayer>();
            if (footstepPlayer == null) footstepPlayer = player.AddComponent<FootstepPlayer>();
            var clips = new System.Collections.Generic.List<AudioClip>();
            foreach (string path in FootstepPaths)
            {
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip != null) clips.Add(clip);
                else Debug.LogWarning($"[SfxSetup] Footstep clip missing: {path}");
            }
            var serialized = new SerializedObject(footstepPlayer);
            var stepClips = serialized.FindProperty("stepClips");
            stepClips.arraySize = clips.Count;
            for (int i = 0; i < clips.Count; i++)
                stepClips.GetArrayElementAtIndex(i).objectReferenceValue = clips[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(footstepPlayer);

            // The Starter Assets animator also emits footstep events. Keep its
            // landing sound, but clear its step array so it cannot double-play
            // alongside the distance-driven FootstepPlayer above.
            var builtInControllers = player.GetComponentsInChildren<StarterAssets.ThirdPersonController>(true);
            int clearedAnimatorStepArrays = 0;
            foreach (var starterController in builtInControllers)
            {
                var starterSerialized = new SerializedObject(starterController);
                var builtInSteps = starterSerialized.FindProperty("FootstepAudioClips");
                if (builtInSteps == null || builtInSteps.arraySize == 0) continue;
                builtInSteps.arraySize = 0;
                starterSerialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(starterController);
                clearedAnimatorStepArrays++;
            }

            int removedDuplicatePlayers = 0;
            foreach (var extra in player.GetComponentsInChildren<FootstepPlayer>(true))
            {
                if (extra == footstepPlayer || !(extra.transform == player.transform || extra.transform.IsChildOf(player.transform)))
                    continue;
                Object.DestroyImmediate(extra);
                removedDuplicatePlayers++;
            }
            Debug.Log($"[SfxSetup] Footsteps: one distance-driven player, cleared built-in arrays on {clearedAnimatorStepArrays} controller(s), removed {removedDuplicatePlayers} duplicate FootstepPlayer(s).");
        }

        [MenuItem("LILO/Setup Sfx On Office Floors")]
        public static void RunOnOfficeFloors()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string returnScenePath = EditorSceneManager.GetActiveScene().path;
            int saved = 0;
            foreach (string sceneName in new[] { "OfficeLevel1", "OfficeLevel2", "OfficeLevel3" })
            {
                var scene = EditorSceneManager.OpenScene($"Assets/Scenes/{sceneName}.unity", OpenSceneMode.Single);
                Run();
                if (scene.IsValid())
                {
                    EditorSceneManager.SaveScene(scene);
                    saved++;
                }
            }
            if (!string.IsNullOrEmpty(returnScenePath))
                EditorSceneManager.OpenScene(returnScenePath, OpenSceneMode.Single);
            Debug.Log($"[SfxSetup] Configured audio in {saved} Office floor scenes.");
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
