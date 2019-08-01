using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

namespace Loju.Audio
{

    public sealed class AudioController : Singleton<AudioController>
    {

        public AudioMixer audioMixer { get { return _audioMixer; } }

        [SerializeField] private AudioMixer _audioMixer = null;
        [SerializeField] private int _audioSourceCacheSize = 4;

        private Queue<AudioSource> _sourcePool;

        protected override void Awake()
        {
            base.Awake();

            if (_sourcePool != null) return;

            // setup audio source pool
            _sourcePool = new Queue<AudioSource>();
            for (int i = 0; i < _audioSourceCacheSize; ++i)
            {
                _sourcePool.Enqueue(CreateAudioSource(string.Format("AudioSource ({0})", i)));
            }
        }

        public float MasterVolume
        {
            get
            {
                return GetVolume("MasterVolume");
            }
            set
            {
                SetVolume("MasterVolume", value);
            }
        }

        public float GetVolume(string group)
        {
#if UNITY_EDITOR
            if (_audioMixer == null) return 0;
#endif

            float value = 0;
            _audioMixer.GetFloat(group, out value);
            return DBToLinear(value);
        }

        public void SetVolume(string group, float value)
        {
#if UNITY_EDITOR
            if (_audioMixer == null) return;
#endif

            _audioMixer.SetFloat(group, LinearToDB(value));
        }

        public void SetValue(string key, float value)
        {
#if UNITY_EDITOR
            if (_audioMixer == null) return;
#endif

            bool success = _audioMixer.SetFloat(key, value);
        }

        public float GetValue(string key)
        {
#if UNITY_EDITOR
            if (_audioMixer == null) return 0;
#endif

            float value = -1;
            _audioMixer.GetFloat(key, out value);

            return value;
        }

        public void Play(AudioClip clip, AudioMixerGroup group, float delay = 0, float volumeScale = 1)
        {
            StartCoroutine(RoutinePlay(clip, group, delay, volumeScale));
        }

        private IEnumerator RoutinePlay(AudioClip clip, AudioMixerGroup group, float delay, float volumeScale)
        {
            if (delay > 0) yield return new WaitForSeconds(delay);

            AudioSource source = GetAudioSource(group);
            source.clip = clip;
            source.volume = volumeScale;
            source.Play();

            while (source.isPlaying) yield return null;

            ReleaseAudioSource(source);
        }

        public AudioSource GetAudioSource(AudioMixerGroup group = null)
        {
            if (_sourcePool == null) Awake();

            AudioSource source = _sourcePool.Count > 0 ? _sourcePool.Dequeue() : CreateAudioSource("AudioSource");
            source.outputAudioMixerGroup = group;
            source.volume = 1;
            source.pitch = 1;
            source.time = 0;
            source.playOnAwake = false;
            source.loop = false;
#if UNITY_EDITOR
            source.name = string.Concat(source.name, " (Allocated)");
#endif

            return source;
        }

        public void ReleaseAudioSource(AudioSource source, bool waitTillComplete = false)
        {
            if (source == null) return;

            if (waitTillComplete && source.isPlaying && !source.loop)
            {
                StartCoroutine(RoutineWaitToRelease(source));
            }
            else
            {
                InternalReleaseAudioSource(source);
            }
        }

        private void InternalReleaseAudioSource(AudioSource source)
        {
            source.Stop();
            source.clip = null;
#if UNITY_EDITOR
            source.name = source.name.Replace(" (Allocated)", "");
#endif

            _sourcePool.Enqueue(source);
        }

        private IEnumerator RoutineWaitToRelease(AudioSource source)
        {
            while (source.isPlaying) yield return null;

            InternalReleaseAudioSource(source);
        }

        private AudioSource CreateAudioSource(string sourceName)
        {
            GameObject go = new GameObject(sourceName);
            go.transform.SetParent(transform, false);

            AudioSource audioSource = go.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

            return audioSource;
        }

        public static float LinearToDB(float linear)
        {
            return linear <= 0 ? -144.0f : 20f * Mathf.Log10(linear);
        }

        public static float DBToLinear(float dB)
        {
            return Mathf.Clamp01(Mathf.Pow(10.0f, dB / 20.0f));
        }

    }

}