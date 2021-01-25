using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

namespace Loju.Audio
{

    [CreateAssetMenu(menuName = "Audio Library")]
    public class AudioLibrary : ScriptableObject, ISerializationCallbackReceiver
    {

        public bool IsMaster { get { return _isMaster; } }
        public bool HasParent { get { return !_isMaster && parent != null; } }
        public AudioLibrary parent { get { return _parent; } }

        [SerializeField] private bool _isMaster = true;
        [SerializeField] private AudioLibrary _parent = null;
        [SerializeField] private AudioLibraryItem[] _items = null;

        private Dictionary<string, AudioLibraryItem> _lookup;

        public bool IsOverride(string key)
        {
            bool containsKey = _lookup.ContainsKey(key);
            bool parentHasKey = HasParent && parent.ContainsKey(key, true);
            return containsKey && parentHasKey;
        }

        public bool ContainsKey(string key, bool includeInherited = false)
        {
            if (_lookup.ContainsKey(key))
                return true;
            else if (includeInherited && HasParent)
                return parent.ContainsKey(key, true);
            else
                return false;
        }

        public void SetupAudioSource(AudioSource source, string key)
        {
            if (_lookup.ContainsKey(key))
            {
                AudioLibraryItem item = _lookup[key];
                if (item.mixerGroup != null) source.outputAudioMixerGroup = item.mixerGroup;
                source.clip = item.GetClip();
                source.volume = item.volumeScale;
            }
        }

        public float GetDelayForKey(string key)
        {
            AudioLibraryItem item;
            if (_lookup.TryGetValue(key, out item)) return item.delay;
            else if (HasParent) return parent.GetDelayForKey(key);
            else return 0;
        }

        public AudioMixerGroup GetMixerGroupForKey(string key)
        {
            AudioLibraryItem item;
            if (_lookup.TryGetValue(key, out item)) return item.mixerGroup;
            else if (HasParent) return parent.GetMixerGroupForKey(key);
            else return null;
        }

        public AudioClip GetClipForKey(string key)
        {
            AudioLibraryItem item;
            if (_lookup.TryGetValue(key, out item)) return item.GetClip();
            else if (HasParent) return parent.GetClipForKey(key);
            else return null;
        }

        public float GetVolumeForKey(string key)
        {
            AudioLibraryItem item;
            if (_lookup.TryGetValue(key, out item)) return item.volumeScale;
            else if (HasParent) return parent.GetVolumeForKey(key);
            else return 1f;
        }

        public string[] GetKeys(bool includeInherited = false)
        {
            HashSet<string> keys = new HashSet<string>();
            InternalGetKeys(keys, null, includeInherited);

            string[] results = new string[keys.Count];
            keys.CopyTo(results);

            return results;
        }

        public string[] GetInheritedKeys()
        {
            if (HasParent)
            {
                HashSet<string> exclude = new HashSet<string>();
                InternalGetKeys(exclude, null, false);

                HashSet<string> keys = new HashSet<string>();
                InternalGetKeys(keys, exclude, true);

                string[] results = new string[keys.Count];
                keys.CopyTo(results);

                return results;
            }
            else
            {
                return null;
            }
        }

        private void InternalGetKeys(HashSet<string> keys, HashSet<string> exclude, bool includeInherited)
        {
            int i = 0, l = _items.Length;
            for (; i < l; ++i)
            {
                string key = _items[i].key;
                if (!keys.Contains(key) && (exclude == null || !exclude.Contains(key))) keys.Add(key);
            }

            if (includeInherited && HasParent)
            {
                parent.InternalGetKeys(keys, exclude, true);
            }
        }

        public void OnAfterDeserialize()
        {
            _lookup = new Dictionary<string, AudioLibraryItem>();
            int i = 0, l = _items.Length;
            for (; i < l; ++i)
            {
                AudioLibraryItem item = _items[i];
                if (!_lookup.ContainsKey(item.key)) _lookup.Add(item.key, item);
            }
        }

        public void OnBeforeSerialize()
        {

        }

        public static AudioLibrary Create(AudioLibraryItem[] items, bool isMaster, AudioLibrary parent)
        {
            AudioLibrary library = ScriptableObject.CreateInstance<AudioLibrary>();
            library._isMaster = isMaster;
            library._parent = parent;
            library._items = items;
            library._lookup = new Dictionary<string, AudioLibraryItem>();

            int i = 0, l = items.Length;
            for (; i < l; ++i)
            {
                library._lookup.Add(items[i].key, items[i]);
            }

            return library;
        }

    }

    [System.Serializable]
    public class AudioLibraryItem
    {

        public string key;
        public AudioMixerGroup mixerGroup;
        public float delay = 0;
        public AudioClip[] clips;
        public bool selectAtRandom = true;
        [Range(0f, 1f)] public float volumeScale = 1f;

        private int _lastIndex = -1;

        public AudioLibraryItem()
        {
            this.selectAtRandom = true;
            this.volumeScale = 1f;
            this.delay = 0;
        }

        public AudioLibraryItem(string key, AudioClip[] clips) : this()
        {
            this.key = key;
            this.clips = clips;
        }

        public AudioClip GetClip()
        {
            int l = clips.Length;
            if (l == 0) return null;
            else if (l == 1) return clips[0];

            if (selectAtRandom)
            {
                int index = Random.Range(0, l);
                while (index == _lastIndex) index = Random.Range(0, l);
                _lastIndex = index;

                return clips[index];
            }
            else
            {
                _lastIndex = (_lastIndex + 1) % l;
                AudioClip clip = clips[_lastIndex];
                return clip;
            }
        }

    }

}