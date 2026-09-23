using System;
using UnityEngine;
using Lilo.Config;
using Lilo.MonoBehaviours.Audio;
using Lilo.MonoBehaviours.Monster;
using Lilo.Systems.Battery;

namespace Lilo.MonoBehaviours.Battery
{
    /// <summary>
    /// Thin MonoBehaviour adapter over <see cref="KeyboardBatteryRoster"/>: on floor
    /// entry, loads a configured number of keyboard slots with batteries (3 of 7)
    /// and serves Take requests with spare-slot, SFX, and noise-pulse handling
    /// mirrored from BatteryPickup.
    /// </summary>
    public class KeyboardBatteryDirector : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private MonsterAIController monster;
        [SerializeField] private SfxController sfx;

        private KeyboardBatterySlot[] _slots = Array.Empty<KeyboardBatterySlot>();
        private Transform _player;

        public int LoadedCount
        {
            get
            {
                int n = 0;
                foreach (var slot in _slots)
                    if (slot != null && slot.IsLoaded) n++;
                return n;
            }
        }

        private void Awake()
        {
            if (config == null)
                config = GameManager.Instance?.Config;

            _slots = FindObjectsByType<KeyboardBatterySlot>();

            Material glow = BuildGlowMaterial();
            foreach (var slot in _slots)
            {
                if (slot == null) continue;
                Renderer rend = slot.GetComponent<MeshRenderer>();
                if (rend == null) rend = slot.GetComponentInChildren<Renderer>();
                slot.Configure(rend != null ? rend.sharedMaterial : null, glow);
                slot.SetLoaded(false);
            }

            int count = config != null ? config.keyboardBatteryCountPerFloor : 3;
            int[] loaded = KeyboardBatteryRoster.PickLoadedIndices(count, _slots.Length, new System.Random());
            foreach (int i in loaded)
                _slots[i].SetLoaded(true);

            Debug.Log($"[BatteryKeyboards] Loaded {loaded.Length} of {_slots.Length} keyboard slots.");
        }

        /// <summary>
        /// Runtime-only glow material cloned from a real keyboard material so the
        /// emissive signal never depends on a serialized asset surviving setup.
        /// </summary>
        private Material BuildGlowMaterial()
        {
            Material source = null;
            foreach (var slot in _slots)
            {
                if (slot == null) continue;
                Renderer rend = slot.GetComponent<MeshRenderer>();
                if (rend == null) rend = slot.GetComponentInChildren<Renderer>();
                if (rend != null && rend.sharedMaterial != null)
                {
                    source = rend.sharedMaterial;
                    break;
                }
            }

            Material glow;
            if (source != null)
                glow = new Material(source);
            else
                glow = new Material(Shader.Find("Universal Render Pipeline/Lit"));

            glow.name = "KeyboardBatteryGlowRuntime";
            glow.EnableKeyword("_EMISSION");
            glow.SetColor("_EmissionColor", new Color(0.25f, 1f, 0.45f) * 2.5f);
            return glow;
        }

        private void Start()
        {
            if (monster == null)
            {
                var monsterGo = GameObject.Find("MonsterPlaceholder");
                if (monsterGo != null) monster = monsterGo.GetComponent<MonsterAIController>();
            }
            if (sfx == null)
            {
                var sfxGo = GameObject.Find("SfxController");
                if (sfxGo != null) sfx = sfxGo.GetComponent<SfxController>();
            }

            var playerGo = GameObject.FindWithTag("Player");
            if (playerGo == null)
                playerGo = GameObject.Find("PlayerCharacter");
            if (playerGo != null)
                _player = playerGo.transform;
        }

        private void Update()
        {
            if (_player == null) return;

            var slot = NearestLoadedSlot(_player.position);
            if (slot != null)
                TryTake(slot);
        }

        /// <summary>Nearest loaded slot within its interaction radius, or null.</summary>
        public KeyboardBatterySlot NearestLoadedSlot(Vector3 position)
        {
            KeyboardBatterySlot best = null;
            float bestDistance = float.MaxValue;

            foreach (var slot in _slots)
            {
                if (slot == null || !slot.IsLoaded) continue;

                float dist = Vector3.Distance(position, slot.transform.position);
                if (dist <= slot.InteractionRadius && dist < bestDistance)
                {
                    bestDistance = dist;
                    best = slot;
                }
            }
            return best;
        }

        public bool TryTake(KeyboardBatterySlot slot)
        {
            if (slot == null || !slot.IsLoaded) return false;

            // Keyboard takes charge the lamp directly: +fraction, clamped at full.
            var state = GameManager.Instance?.State;
            if (state != null && config != null)
            {
                float gain = config.keyboardBatteryChargeFraction * config.batteryDuration;
                state.SetInstalledBatteryCharge(
                    KeyboardBatteryRoster.TopUpCharge(state.InstalledBatteryCharge, gain, config.batteryDuration));
            }

            slot.SetLoaded(false);

            Debug.Log("[BatteryKeyboards] Took battery from keyboard.");
            if (sfx != null) sfx.PlayBatteryPickup();
            if (monster != null && config != null)
                monster.EmitPulse(slot.transform.position, config.noiseBatterySwap);
            return true;
        }
    }
}
