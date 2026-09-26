using Lilo.Config;
using Lilo.MonoBehaviours;
using UnityEngine;
using UnityEngine.AI;

namespace Lilo.MonoBehaviours.Interaction
{
    /// <summary>Ensures a floor-specific key exists at one of that scene's authored table points.</summary>
    public static class AccessKeyBootstrap
    {
        public static void Ensure()
        {
            GameManager manager = GameManager.Instance;
            if (manager == null || manager.State == null || manager.Config == null) return;

            FloorId floor = manager.State.CurrentFloor;
            if (manager.Config.GetLockedDoorCount(floor) <= 0) return;
            string keyId = AccessKeyPickup.KeyIdForFloor(floor);
            if (manager.State.HasKey(keyId)) return;

            GameObject playerObject = GameObject.Find("PlayerCharacter");
            if (playerObject == null) return;

            AccessKeyPickup pickup = FindExistingPickup();
            GameObject keyObject = pickup != null ? pickup.gameObject : FindExistingKeyObject();
            GameObject prefab = null;
            if (keyObject == null)
            {
                prefab = Resources.Load<GameObject>("access-key");
                if (prefab == null)
                {
                    Debug.LogError("[AccessKey] Missing Assets/Resources/access-key.prefab; locked exits will have no key pickup.");
                    return;
                }
            }

            Vector3 spawnPosition;
            if (!manager.State.TryGetAccessKeySpawnPosition(floor, out spawnPosition))
            {
                if (!TryFindAuthoredTableSpawn(manager.Config, playerObject.transform, out spawnPosition, out string tableName))
                {
                    if (TryFindSafeFallback(manager.Config, playerObject.transform, out spawnPosition))
                    {
                        Debug.LogWarning("[AccessKey] No authored table passed surface, NavMesh reachability, distance, and clearance checks; using a validated reachable floor fallback.");
                    }
                    else
                    {
                        spawnPosition = playerObject.transform.position + Vector3.up * 0.3f;
                        Debug.LogWarning("[AccessKey] No valid table or reachable floor fallback was available; placing the key at the player start so it remains recoverable.");
                    }
                }
                else
                {
                    Debug.Log($"[AccessKey] Spawned on authored table '{tableName}' at {spawnPosition}.");
                }

                // GameState survives a same-floor death reload, so the key stays on
                // the same table until a new run resets the spawn cache.
                manager.State.SetAccessKeySpawnPosition(floor, spawnPosition);
            }

            if (keyObject == null)
            {
                keyObject = Object.Instantiate(prefab, spawnPosition, prefab.transform.rotation);
                keyObject.name = "access-key";
            }
            else
            {
                keyObject.transform.position = spawnPosition;
            }

            if (pickup == null) pickup = keyObject.GetComponent<AccessKeyPickup>();
            if (pickup == null) pickup = keyObject.AddComponent<AccessKeyPickup>();
            pickup.Configure(keyId, manager.Config);
        }

        private static AccessKeyPickup FindExistingPickup()
        {
            AccessKeyPickup[] pickups = Object.FindObjectsByType<AccessKeyPickup>();
            return pickups.Length > 0 ? pickups[0] : null;
        }

        private static GameObject FindExistingKeyObject()
        {
            foreach (Transform candidate in Object.FindObjectsByType<Transform>())
            {
                if (string.Equals(candidate.name, "access-key", System.StringComparison.OrdinalIgnoreCase))
                    return candidate.gameObject;
            }
            return null;
        }

        private static bool TryFindAuthoredTableSpawn(
            GameConfig config, Transform player, out Vector3 keyPosition, out string tableName)
        {
            keyPosition = default;
            tableName = string.Empty;
            AccessKeySpawnTable[] tables = Object.FindObjectsByType<AccessKeySpawnTable>();
            if (tables.Length == 0) return false;

            // Shuffle only explicitly marked scene tables; invalid choices fall
            // through to the remaining authored candidates.
            for (int i = tables.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (tables[i], tables[j]) = (tables[j], tables[i]);
            }

            float minDistance = Mathf.Max(1f, config.accessKeySpawnMinDistance);
            float maxDistance = Mathf.Max(minDistance, config.accessKeySpawnMaxDistance);
            if (!NavMesh.SamplePosition(player.position, out NavMeshHit playerHit, 2f, NavMesh.AllAreas))
                return false;

            var path = new NavMeshPath();
            foreach (AccessKeySpawnTable table in tables)
            {
                if (table == null || !table.isActiveAndEnabled
                    || !table.TryGetKeyPosition(out Vector3 candidate))
                    continue;

                float distance = PlanarDistance(player.position, candidate);
                if (distance < minDistance || distance > maxDistance)
                    continue;

                if (!NavMesh.SamplePosition(candidate, out NavMeshHit approach,
                        table.ApproachSearchRadius, NavMesh.AllAreas))
                    continue;
                if (Mathf.Abs(approach.position.y - candidate.y) > table.ApproachSearchRadius
                    || PlanarDistance(approach.position, candidate) > AccessKeyPickup.DefaultInteractionRadius)
                    continue;

                if (!NavMesh.CalculatePath(playerHit.position, approach.position, NavMesh.AllAreas, path)
                    || path.status != NavMeshPathStatus.PathComplete)
                    continue;

                if (!HasPickupClearance(candidate, table, player))
                    continue;

                keyPosition = candidate;
                tableName = table.gameObject.name;
                return true;
            }

            return false;
        }

        private static bool TryFindSafeFallback(GameConfig config, Transform player, out Vector3 position)
        {
            position = default;
            float minDistance = Mathf.Max(1f, config.accessKeySpawnMinDistance);
            float maxDistance = Mathf.Max(minDistance, config.accessKeySpawnMaxDistance);
            if (!NavMesh.SamplePosition(player.position, out NavMeshHit start, 2f, NavMesh.AllAreas))
                return false;

            Vector3 forward = Vector3.ProjectOnPlane(player.forward, Vector3.up).normalized;
            if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
            float startingAngle = Random.Range(0f, 360f);
            var path = new NavMeshPath();

            for (float distance = maxDistance; distance >= minDistance; distance -= 0.5f)
            {
                for (int i = 0; i < 36; i++)
                {
                    Vector3 direction = Quaternion.AngleAxis(startingAngle + i * 10f, Vector3.up) * forward;
                    Vector3 target = start.position + direction * distance;
                    if (!NavMesh.SamplePosition(target, out NavMeshHit hit, 1.1f, NavMesh.AllAreas))
                        continue;
                    if (PlanarDistance(player.position, hit.position) < minDistance
                        || PlanarDistance(player.position, hit.position) > maxDistance)
                        continue;
                    if (!NavMesh.CalculatePath(start.position, hit.position, NavMesh.AllAreas, path)
                        || path.status != NavMeshPathStatus.PathComplete)
                        continue;
                    if (!HasPickupClearance(hit.position + Vector3.up * 0.18f, null, player))
                        continue;
                    position = hit.position + Vector3.up * 0.18f;
                    return true;
                }
            }

            return false;
        }

        private static bool HasPickupClearance(Vector3 keyPosition, AccessKeySpawnTable table, Transform player)
        {
            Collider[] overlaps = Physics.OverlapSphere(keyPosition + Vector3.up * 0.1f,
                0.09f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            foreach (Collider overlap in overlaps)
            {
                if (overlap == null) continue;
                Transform other = overlap.transform;
                if (other == player || other.IsChildOf(player)) continue;
                if (table != null && (other == table.transform || other.IsChildOf(table.transform)
                    || table.IsTabletopCollisionProxy(overlap))) continue;
                return false;
            }
            return true;
        }

        private static float PlanarDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }
    }
}
