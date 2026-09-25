using System.Collections.Generic;
using UnityEngine;

namespace Lilo.MonoBehaviours.Audio
{
    /// <summary>
    /// Room-tone bed (always looping) plus a shuffle-bag of one-shot ambience
    /// stings at randomized intervals. Shuffle-bag guarantees every sting plays
    /// once per cycle before any repeats — no redundancy. All tails are baked
    /// with fade-outs; the bed loops untouched for a seamless seam.
    /// Persists across scenes (DontDestroyOnLoad, singleton guard).
    /// </summary>
    public class AmbienceDirector : MonoBehaviour
    {
        [Header("Bed (always on)")]
        [SerializeField] private AudioClip roomTone;
        [SerializeField] private AudioSource bedSource;
        [SerializeField, Range(0f, 1f)] private float bedVolume = 0.6f;

        [Header("One-shots (shuffle-bag, random gaps)")]
        [SerializeField] private AudioClip[] stings = new AudioClip[0];
        [SerializeField, Range(0f, 1f)] private float stingVolume = 0.9f;
        [Tooltip("Seconds between stings.")]
        [SerializeField] private float minGapSeconds = 8f;
        [SerializeField] private float maxGapSeconds = 18f;

        private AudioSource _bedSource;
        private AudioSource _stingSource;
        private readonly List<int> _bag = new List<int>();
        private float _timer;
        private float _nextBedPlayAttempt;
        private float _nextRoomToneLoadAttempt;
        private Transform _player;
        private bool _started;
        private bool _warnedMissingRoomTone;

        private static AmbienceDirector _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            _bedSource = bedSource != null ? bedSource : GetComponent<AudioSource>();
            if (_bedSource == null)
            {
                Debug.LogError("[Ambience] No bed AudioSource is assigned.", this);
                enabled = false;
                return;
            }

            _bedSource.playOnAwake = false;
            _bedSource.spatialBlend = 0f;
            _bedSource.loop = true;
            _bedSource.volume = bedVolume;

            if (HasStings())
            {
                _stingSource = gameObject.AddComponent<AudioSource>();
                _stingSource.playOnAwake = false;
                _stingSource.spatialBlend = 0f;
                _stingSource.loop = false;
                _stingSource.volume = stingVolume;
            }

            RefillBag();
            _timer = Random.Range(minGapSeconds, maxGapSeconds);
        }

        private void Update()
        {
            // Ambience belongs to the run, not the menu: start only after the
            // player has spawned. Re-resolves after floor restarts, since the
            // director outlives scenes but the player does not.
            if (_player == null)
            {
                var playerGo = GameObject.FindWithTag("Player");
                if (playerGo == null)
                    playerGo = GameObject.Find("PlayerCharacter");
                if (playerGo == null || !playerGo.activeInHierarchy)
                    return;
                _player = playerGo.transform;
            }

            if (!_started)
            {
                _started = true;
                if (roomTone == null && !_warnedMissingRoomTone)
                {
                    Debug.LogWarning("[Ambience] No room tone assigned — bed silent.");
                    _warnedMissingRoomTone = true;
                }
            }

            TryStartBed();
            if (_bag.Count == 0) return;

            _timer -= Time.deltaTime;
            if (_timer > 0f) return;

            _timer = Random.Range(minGapSeconds, maxGapSeconds);

            int last = _bag.Count - 1;
            int pick = Random.Range(0, _bag.Count);
            int clipIndex = _bag[pick];
            _bag[pick] = _bag[last];
            _bag.RemoveAt(last);
            if (_bag.Count == 0)
                RefillBag();

            AudioClip clip = clipIndex >= 0 && clipIndex < stings.Length ? stings[clipIndex] : null;
            if (clip == null) return;

            _stingSource.volume = stingVolume;
            _stingSource.PlayOneShot(clip);
        }

        private void RefillBag()
        {
            _bag.Clear();
            if (stings == null) return;
            for (int i = 0; i < stings.Length; i++)
            {
                if (stings[i] != null)
                    _bag.Add(i);
            }
        }

        private bool HasStings()
        {
            if (stings == null) return false;
            for (int i = 0; i < stings.Length; i++)
            {
                if (stings[i] != null) return true;
            }
            return false;
        }

        private void TryStartBed()
        {
            if (roomTone == null || _bedSource == null || _bedSource.isPlaying)
                return;

            if (roomTone.loadState == AudioDataLoadState.Unloaded)
            {
                if (Time.unscaledTime < _nextRoomToneLoadAttempt)
                    return;

                roomTone.LoadAudioData();
                _nextRoomToneLoadAttempt = Time.unscaledTime + 1f;
                return;
            }

            if (roomTone.loadState != AudioDataLoadState.Loaded)
                return;

            if (Time.unscaledTime < _nextBedPlayAttempt)
                return;

            _bedSource.clip = roomTone;
            _bedSource.Play();
            _nextBedPlayAttempt = Time.unscaledTime + 1f;
        }
    }
}
