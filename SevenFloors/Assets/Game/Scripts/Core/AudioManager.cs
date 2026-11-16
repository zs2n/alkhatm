using System.Collections;
using UnityEngine;

namespace SevenFloors.Core
{
    /// <summary>
    /// مدير الصوت: موسيقى (مع تلاشي متقاطع) + مؤثرات + أصوات شخصيات.
    /// المسارات نسبية لمجلد Resources مثل: Audio/Music/main_theme
    /// غياب الملف لا يسبب خطأ — يتم تجاهله مع تحذير في الـ Console.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager I { get; private set; }

        private AudioSource _musicA, _musicB, _sfx, _voice;
        private bool _usingA = true;
        private string _currentMusic = "";
        private Coroutine _fade;

        public static AudioManager Create()
        {
            var go = new GameObject("AudioManager");
            DontDestroyOnLoad(go);
            var am = go.AddComponent<AudioManager>();
            I = am;
            am._musicA = go.AddComponent<AudioSource>();
            am._musicB = go.AddComponent<AudioSource>();
            am._sfx = go.AddComponent<AudioSource>();
            am._voice = go.AddComponent<AudioSource>();
            am._musicA.loop = am._musicB.loop = true;
            return am;
        }

        public void PlayMusic(string path, float fadeTime = 1.2f)
        {
            if (path == _currentMusic) return;
            if (string.IsNullOrEmpty(path) || path == "-") { StopMusic(fadeTime); return; }
            var clip = Resources.Load<AudioClip>(path);
            if (clip == null)
            {
                Debug.LogWarning("[SevenFloors] موسيقى غير موجودة: Resources/" + path);
                return;
            }
            _currentMusic = path;
            var from = _usingA ? _musicA : _musicB;
            var to = _usingA ? _musicB : _musicA;
            _usingA = !_usingA;
            to.clip = clip;
            to.volume = 0;
            to.Play();
            if (_fade != null) StopCoroutine(_fade);
            _fade = StartCoroutine(CrossFade(from, to, fadeTime));
        }

        public void StopMusic(float fadeTime = 1.0f)
        {
            _currentMusic = "";
            var cur = _usingA ? _musicA : _musicB;
            if (_fade != null) StopCoroutine(_fade);
            _fade = StartCoroutine(CrossFade(cur, null, fadeTime));
        }

        private IEnumerator CrossFade(AudioSource from, AudioSource to, float time)
        {
            float t = 0;
            float fromStart = from != null ? from.volume : 0;
            while (t < time)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / time);
                if (from != null) from.volume = Mathf.Lerp(fromStart, 0, k);
                if (to != null) to.volume = Mathf.Lerp(0, GameSettings.MusicVolume, k);
                yield return null;
            }
            if (from != null) { from.Stop(); from.clip = null; }
        }

        public void PlaySfx(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            var clip = Resources.Load<AudioClip>(path);
            if (clip == null) { Debug.LogWarning("[SevenFloors] مؤثر غير موجود: Resources/" + path); return; }
            _sfx.PlayOneShot(clip, GameSettings.SfxVolume);
        }

        public void PlayVoice(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            var clip = Resources.Load<AudioClip>(path);
            if (clip == null) { Debug.LogWarning("[SevenFloors] صوت غير موجود: Resources/" + path); return; }
            _voice.Stop();
            _voice.clip = clip;
            _voice.volume = GameSettings.VoiceVolume;
            _voice.Play();
        }

        public void RefreshVolumes()
        {
            var cur = _usingA ? _musicA : _musicB;
            if (cur.isPlaying) cur.volume = GameSettings.MusicVolume;
        }
    }
}
