using Lilo.Config;
using Lilo.MonoBehaviours;
using Lilo.MonoBehaviours.Battery;
using UnityEngine;

namespace Lilo.MonoBehaviours.Interaction
{
    /// <summary>Ensures a collectible access key exists on every floor with a locked exit.</summary>
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

            AccessKeyPickup[] existingPickups = Object.FindObjectsByType<AccessKeyPickup>();
            if (existingPickups.Length > 0)
            {
                existingPickups[0].Configure(keyId, manager.Config);
                return;
            }

            GameObject existingKey = null;
            foreach (Transform candidate in Object.FindObjectsByType<Transform>())
            {
                if (string.Equals(candidate.name, "access-key", System.StringComparison.OrdinalIgnoreCase))
                {
                    existingKey = candidate.gameObject;
                    break;
                }
            }

            if (existingKey == null)
            {
                GameObject prefab = Resources.Load<GameObject>("access-key");
                if (prefab == null)
                {
                    Debug.LogError("[AccessKey] Missing Assets/Resources/access-key.prefab; locked exits will have no key pickup.");
                    return;
                }

                Vector3 spawnPosition = FindDeskPickupPosition();
                existingKey = Object.Instantiate(prefab, spawnPosition, prefab.transform.rotation);
                existingKey.name = "access-key";
            }

            AccessKeyPickup pickup = existingKey.GetComponent<AccessKeyPickup>();
            if (pickup == null) pickup = existingKey.AddComponent<AccessKeyPickup>();
            pickup.Configure(keyId, manager.Config);
        }

        private static Vector3 FindDeskPickupPosition()
        {
            KeyboardBatterySlot[] slots = Object.FindObjectsByType<KeyboardBatterySlot>();
            KeyboardBatterySlot selected = null;
            foreach (KeyboardBatterySlot slot in slots)
            {
                if (slot != null && !slot.IsLoaded)
                {
                    selected = slot;
                    break;
                }
            }
            if (selected == null && slots.Length > 0) selected = slots[0];

            if (selected != null)
            {
                Renderer renderer = selected.GetComponentInChildren<Renderer>();
                Vector3 position = renderer != null ? renderer.bounds.center : selected.transform.position;
                if (renderer != null) position.y = renderer.bounds.max.y + 0.12f;
                else position.y += 0.8f;
                return position + selected.transform.right * 0.32f;
            }

            GameObject player = GameObject.Find("PlayerCharacter");
            return player != null
                ? player.transform.position + player.transform.forward * 2f + Vector3.up * 0.2f
                : Vector3.up * 0.5f;
        }
    }
}
