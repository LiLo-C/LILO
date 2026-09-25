using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Lilo.MonoBehaviours.Interaction;
using Lilo.Config;

/// <summary>Read-only scene inspection for manually authoring key spawn tables.</summary>
public static class AccessKeySpawnTableAuthoring
{
    private static readonly string[] GameplayScenes = { "OfficeLevel1", "OfficeLevel2", "OfficeLevel3" };

    // These paths were inspected in the scene candidate report and approved as
    // reachable desks. Keeping the list explicit prevents runtime name matching
    // from turning decorative or blocked objects into spawn surfaces.
    private static readonly System.Collections.Generic.Dictionary<string, AuthoredDesk[]> ApprovedDesks =
        new System.Collections.Generic.Dictionary<string, AuthoredDesk[]>
        {
            ["OfficeLevel1"] = new[]
            {
                new AuthoredDesk("CubiclesCard/OpenCubicles/FCubicle-1/DeskBase", new Vector3(-6.69f, 1.48f, 3.58f)),
                new AuthoredDesk("CubiclesCard/OpenCubicles/Cubicle-9/desk", new Vector3(7.98f, 1.69f, 2.06f)),
            },
            ["OfficeLevel2"] = new[]
            {
                new AuthoredDesk("Cubicles/Cubicle-1 (1)/desk", new Vector3(-11.00f, 1.86f, 10.15f)),
                new AuthoredDesk("Cubicles/Cubicle-1 (12)/desk", new Vector3(-13.80f, 1.86f, 6.18f)),
            },
            ["OfficeLevel3"] = new[]
            {
                // This is the one reachable authored desk surface within the
                // floor's existing 5–8 m spawn-distance band.
                new AuthoredDesk("DeskSideRight", new Vector3(2.30f, 2.12f, -2.48f)),
            },
        };

    private readonly struct AuthoredDesk
    {
        public readonly string Path;
        public readonly Vector3 SurfacePoint;

        public AuthoredDesk(string path, Vector3 surfacePoint)
        {
            Path = path;
            SurfacePoint = surfacePoint;
        }
    }

    [MenuItem("LILO/Mark Approved Key Spawn Tables")]
    public static void MarkApprovedTables()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        var openedScenes = new System.Collections.Generic.List<Scene>();
        var markedCount = 0;

        foreach (string sceneName in GameplayScenes)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                scene = EditorSceneManager.OpenScene($"Assets/Scenes/{sceneName}.unity", OpenSceneMode.Additive);
                openedScenes.Add(scene);
            }

            if (!ApprovedDesks.TryGetValue(sceneName, out AuthoredDesk[] desks)) continue;
            bool sceneChanged = false;
            var approvedPaths = new System.Collections.Generic.HashSet<string>();
            foreach (AuthoredDesk approved in desks) approvedPaths.Add(approved.Path);
            foreach (AccessKeySpawnTable existing in FindSceneComponents<AccessKeySpawnTable>(scene))
            {
                if (approvedPaths.Contains(GetPath(existing.transform))) continue;
                Transform oldSurface = existing.transform.Find("AccessKeySpawnSurface");
                if (oldSurface != null) Undo.DestroyObjectImmediate(oldSurface.gameObject);
                Undo.DestroyObjectImmediate(existing);
                sceneChanged = true;
            }

            foreach (AuthoredDesk desk in desks)
            {
                Transform table = FindByPath(scene, desk.Path);
                if (table == null)
                {
                    Debug.LogError($"[AccessKeyAuthoring] Approved table path not found in {sceneName}: {desk.Path}");
                    continue;
                }

                AccessKeySpawnTable marker = table.GetComponent<AccessKeySpawnTable>();
                if (marker == null) marker = Undo.AddComponent<AccessKeySpawnTable>(table.gameObject);

                Transform surface = table.Find("AccessKeySpawnSurface");
                if (surface == null)
                {
                    var anchor = new GameObject("AccessKeySpawnSurface");
                    Undo.RegisterCreatedObjectUndo(anchor, "Create key spawn surface");
                    anchor.transform.SetParent(table, true);
                    surface = anchor.transform;
                }

                Undo.RecordObject(surface, "Set key spawn surface");
                surface.position = desk.SurfacePoint;
                Collider[] surfaceProxies = FindTabletopCollisionProxies(table);
                marker.Configure(surface, 0.12f, surfaceProxies);
                EditorUtility.SetDirty(marker);
                EditorUtility.SetDirty(surface);
                sceneChanged = true;
                markedCount++;
                Debug.Log($"[AccessKeyAuthoring] Marked {sceneName}: {desk.Path} at {desk.SurfacePoint}.");
            }

            if (sceneChanged)
                EditorSceneManager.SaveScene(scene);
        }

        foreach (Scene scene in openedScenes)
            EditorSceneManager.CloseScene(scene, false);
        if (activeScene.IsValid() && activeScene.isLoaded)
            SceneManager.SetActiveScene(activeScene);

        Debug.Log($"[AccessKeyAuthoring] Saved {markedCount} explicit key spawn table markers across {GameplayScenes.Length} levels.");
    }

    [MenuItem("LILO/Validate Key Spawn Tables")]
    public static void ValidateMarkedTables()
    {
        GameConfig config = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/Config/GameConfig.asset");
        if (config == null)
        {
            Debug.LogError("[AccessKeyAuthoring] Could not load Assets/Config/GameConfig.asset.");
            return;
        }

        Scene activeScene = SceneManager.GetActiveScene();
        var openedScenes = new System.Collections.Generic.List<Scene>();
        int checkedCount = 0;
        int failedCount = 0;

        foreach (string sceneName in GameplayScenes)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                scene = EditorSceneManager.OpenScene($"Assets/Scenes/{sceneName}.unity", OpenSceneMode.Additive);
                openedScenes.Add(scene);
            }

            Transform player = FindNamedTransform(scene, "PlayerCharacter");
            bool hasPlayerNav = player != null
                && NavMesh.SamplePosition(player.position, out NavMeshHit playerHit, 2f, NavMesh.AllAreas);
            NavMeshHit resolvedPlayer = default;
            if (hasPlayerNav) NavMesh.SamplePosition(player.position, out resolvedPlayer, 2f, NavMesh.AllAreas);

            AccessKeySpawnTable[] tables = FindSceneComponents<AccessKeySpawnTable>(scene);
            if (tables.Length == 0)
            {
                failedCount++;
                Debug.LogError($"[AccessKeyAuthoring] {sceneName}: no explicit spawn table is marked.");
                continue;
            }

            foreach (AccessKeySpawnTable table in tables)
            {
                checkedCount++;
                Vector3 keyPosition = default;
                bool valid = table != null && player != null && hasPlayerNav
                    && table.isActiveAndEnabled && table.TryGetKeyPosition(out keyPosition);
                float distance = valid ? PlanarDistance(player.position, keyPosition) : -1f;
                bool validDistance = valid && distance >= config.accessKeySpawnMinDistance
                    && distance <= config.accessKeySpawnMaxDistance;
                bool validApproach = false;
                if (validDistance
                    && NavMesh.SamplePosition(keyPosition, out NavMeshHit approach,
                        table.ApproachSearchRadius, NavMesh.AllAreas)
                    && Mathf.Abs(approach.position.y - keyPosition.y) <= table.ApproachSearchRadius
                    && PlanarDistance(approach.position, keyPosition) <= AccessKeyPickup.DefaultInteractionRadius)
                {
                    var path = new NavMeshPath();
                    validApproach = NavMesh.CalculatePath(resolvedPlayer.position, approach.position,
                        NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete;
                }

                string blockage = string.Empty;
                bool clear = validApproach && HasClearance(keyPosition, table, player, out blockage);
                string status = clear ? "VALID" : "INVALID";
                Debug.Log($"[AccessKeyAuthoring] {status} {sceneName}: {GetPath(table.transform)}"
                    + $" key={Format(valid ? keyPosition : Vector3.zero)} distance={distance:0.00}m"
                    + $" playerNav={hasPlayerNav} distanceRule={validDistance} approachPath={validApproach} clearance={clear}"
                    + (string.IsNullOrEmpty(blockage) ? "." : $" blockedBy={blockage}."));
                if (!clear) failedCount++;
            }
        }

        foreach (Scene scene in openedScenes)
            EditorSceneManager.CloseScene(scene, false);
        if (activeScene.IsValid() && activeScene.isLoaded)
            SceneManager.SetActiveScene(activeScene);

        Debug.Log($"[AccessKeyAuthoring] Checked {checkedCount} marked tables. Invalid: {failedCount}.");
    }

    private static T[] FindSceneComponents<T>(Scene scene) where T : Component
    {
        var found = new System.Collections.Generic.List<T>();
        foreach (GameObject root in scene.GetRootGameObjects())
            found.AddRange(root.GetComponentsInChildren<T>(true));
        return found.ToArray();
    }

    private static Collider[] FindTabletopCollisionProxies(Transform table)
    {
        if (table.parent == null) return System.Array.Empty<Collider>();
        foreach (Collider collider in table.parent.GetComponentsInChildren<Collider>(true))
            if (collider != null && collider.gameObject.name == "DeskCollider")
                return new[] { collider };
        return System.Array.Empty<Collider>();
    }

    private static bool HasClearance(Vector3 keyPosition, AccessKeySpawnTable table, Transform player, out string blockage)
    {
        blockage = string.Empty;
        Collider[] overlaps = Physics.OverlapSphere(keyPosition + Vector3.up * 0.1f,
            0.09f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        foreach (Collider overlap in overlaps)
        {
            if (overlap == null) continue;
            Transform other = overlap.transform;
            if (other == player || other.IsChildOf(player)) continue;
            if (other == table.transform || other.IsChildOf(table.transform)
                || table.IsTabletopCollisionProxy(overlap)) continue;
            blockage = $"{GetPath(other)} boundsCenter={Format(overlap.bounds.center)} boundsMax={Format(overlap.bounds.max)}";
            return false;
        }
        return true;
    }

    [MenuItem("LILO/Inspect Key Spawn Table Candidates")]
    public static void InspectCandidates()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        var openedScenes = new System.Collections.Generic.List<Scene>();
        var report = new StringBuilder();

        foreach (string sceneName in GameplayScenes)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                scene = EditorSceneManager.OpenScene($"Assets/Scenes/{sceneName}.unity", OpenSceneMode.Additive);
                openedScenes.Add(scene);
            }

            AppendSceneCandidates(scene, report);
        }

        string reportPath = Path.GetFullPath("Library/AccessKeySpawnTableCandidates.txt");
        File.WriteAllText(reportPath, report.ToString());

        foreach (Scene scene in openedScenes)
            EditorSceneManager.CloseScene(scene, false);
        if (currentScene.IsValid() && currentScene.isLoaded)
            SceneManager.SetActiveScene(currentScene);

        Debug.Log($"[AccessKeyAuthoring] Wrote manual desk candidate report to {reportPath}.");
    }

    private static void AppendSceneCandidates(Scene scene, StringBuilder report)
    {
        report.AppendLine($"=== {scene.name} ===");
        Transform player = FindNamedTransform(scene, "PlayerCharacter");
        NavMeshHit playerHit = default;
        bool hasPlayerNav = player != null
            && NavMesh.SamplePosition(player.position, out playerHit, 2f, NavMesh.AllAreas);
        var candidateCount = 0;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
            {
                string lower = transform.name.ToLowerInvariant();
                if (!lower.Contains("desk") && !lower.Contains("table")) continue;
                if (!transform.gameObject.activeInHierarchy) continue;

                Renderer[] renderers = transform.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length == 0) continue;
                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                    bounds.Encapsulate(renderers[i].bounds);

                float distance = player != null ? PlanarDistance(player.position, bounds.center) : -1f;
                bool navReachable = false;
                Vector3 approachPoint = new Vector3(bounds.center.x, player != null ? player.position.y : bounds.center.y, bounds.center.z);
                if (player != null && NavMesh.SamplePosition(approachPoint, out NavMeshHit approach, 2.5f, NavMesh.AllAreas))
                {
                    approachPoint = approach.position;
                    if (hasPlayerNav)
                    {
                        var path = new NavMeshPath();
                        navReachable = NavMesh.CalculatePath(playerHit.position, approach.position, NavMesh.AllAreas, path)
                            && path.status == NavMeshPathStatus.PathComplete;
                    }
                }

                candidateCount++;
                report.AppendLine($"{GetPath(transform)} | active={transform.gameObject.activeInHierarchy}"
                    + $" | pos={Format(transform.position)} | unionCenter={Format(bounds.center)}"
                    + $" | unionMax={Format(bounds.max)} | planarFromPlayer={distance:0.00}"
                    + $" | navApproach={Format(approachPoint)} | pathComplete={navReachable}"
                    + $" | colliders={transform.GetComponentsInChildren<Collider>(true).Length}");

                foreach (Renderer renderer in renderers)
                {
                    MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                    string meshName = meshFilter != null && meshFilter.sharedMesh != null
                        ? meshFilter.sharedMesh.name : renderer.GetType().Name;
                    report.AppendLine($"  RENDERER {GetPath(renderer.transform)} mesh={meshName}"
                        + $" center={Format(renderer.bounds.center)} max={Format(renderer.bounds.max)}"
                        + $" size={Format(renderer.bounds.size)}");
                }
            }
        }

        report.AppendLine($"Candidate nodes: {candidateCount}");
        report.AppendLine();
    }

    private static Transform FindNamedTransform(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
            {
                if (transform.name == name)
                    return transform;
            }
        }
        return null;
    }

    private static Transform FindByPath(Scene scene, string path)
    {
        string[] segments = path.Split('/');
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name != segments[0]) continue;
            Transform current = root.transform;
            bool found = true;
            for (int i = 1; i < segments.Length; i++)
            {
                current = current.Find(segments[i]);
                if (current == null)
                {
                    found = false;
                    break;
                }
            }

            if (found) return current;
        }

        return null;
    }

    private static string GetPath(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }

    private static float PlanarDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    private static string Format(Vector3 value) => $"({value.x:0.00}, {value.y:0.00}, {value.z:0.00})";
}
