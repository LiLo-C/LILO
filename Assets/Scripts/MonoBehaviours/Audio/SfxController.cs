using UnityEngine;

namespace Lilo.MonoBehaviours.Audio
{
    /// <summary>
    /// One-shot SFX player for event-driven sounds (movement start, chase start).
    /// Each clip plays once per event; no looping, no stacking.
    /// </summary>
    public class SfxController : MonoBehaviour
    {
        [SerializeField] private AudioClip behindYouClip;
        [SerializeField] private AudioClip horrorChaseClip;
        [SerializeField, Range(0f, 1f)] private float volume = 0.8f;

        private AudioSource _source;

        private void Awake()
        {
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0f; // 2D
        }

        public void PlayBehindYou()
        {
            if (behindYouClip != null)
                _source.PlayOneShot(behindYouClip, volume);
        }

        public void PlayHorrorChase()
        {
            if (horrorChaseClip != null)
                _source.PlayOneShot(horrorChaseClip, volume);
        }
    }
}
