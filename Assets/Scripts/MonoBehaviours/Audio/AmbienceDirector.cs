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
        [SerializeField, Range(0f, 1f)] private float bedVolume = 0.6f;

        [Header("One-shots (shuffle-bag, random gaps)")]
        [SerializeField] private AudioClip[] stings = new AudioClip[0];
        [SerializeField, Range(0f, 1f)] private float stingVolume = 0.9f;
        [Tooltip("Seconds between stings.")]
        [SerializeField] private float minGapSeconds = 18f;
        [SerializeField] private float maxGapSeconds = 38f;

        private AudioSource _bedSource;
        private AudioSource _stingSource;
        private readonly List<int> _bag = new List<int>();
        private float _timer;

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

            _bedSource = gameObject.AddComponent<AudioSource>();
            _bedSource.playOnAwake = false;
            _bedSource.spatialBlend = 0f;
            _bedSource.loop = true;
            _bedSource.volume = bedVolume;

            _stingSource = gameObject.AddComponent<AudioSource>();
            _stingSource.playOnAwake = false;
            _stingSource.spatialBlend = 0f;
            _stingSource.loop = false;
            _stingSource.volume = stingVolume;

            if (roomTone != null)
            {
                _bedSource.clip = roomTone;
                _bedSource.Play();
            }
            else
            {
                Debug.LogWarning("[Ambience] No room tone assigned — bed silent.");
            }

            RefillBag();
            _timer = Random.Range(minGapSeconds, maxGapSeconds);
        }

        private void Update()
        {
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
    }
}
