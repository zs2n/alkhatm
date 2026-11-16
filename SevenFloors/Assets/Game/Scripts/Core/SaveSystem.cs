using System;
using System.IO;
using UnityEngine;
using SevenFloors.Data;

namespace SevenFloors.Core
{
    /// <summary>
    /// نظام الحفظ والتحميل: ملفات JSON في مجلد حفظ النظام
    /// (Application.persistentDataPath/saves). 6 خانات + حفظ تلقائي.
    /// </summary>
    public static class SaveSystem
    {
        public const int SlotCount = 6;
        public const int AutoSlot = 0; // الخانة 0 = الحفظ التلقائي

        private static string Dir
        {
            get
            {
                string d = Path.Combine(Application.persistentDataPath, "saves");
                if (!Directory.Exists(d)) Directory.CreateDirectory(d);
                return d;
            }
        }

        private static string PathFor(int slot) { return Path.Combine(Dir, "slot" + slot + ".json"); }

        public static bool HasSave(int slot) { return File.Exists(PathFor(slot)); }

        public static bool AnySaveExists()
        {
            for (int i = 0; i <= SlotCount; i++) if (HasSave(i)) return true;
            return false;
        }

        public static void Save(int slot, SaveData data)
        {
            try
            {
                data.timestamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
                File.WriteAllText(PathFor(slot), JsonUtility.ToJson(data, true));
                Debug.Log("[SevenFloors] تم الحفظ في الخانة " + slot);
            }
            catch (Exception e) { Debug.LogError("[SevenFloors] فشل الحفظ: " + e.Message); }
        }

        public static SaveData Load(int slot)
        {
            try
            {
                if (!HasSave(slot)) return null;
                return JsonUtility.FromJson<SaveData>(File.ReadAllText(PathFor(slot)));
            }
            catch (Exception e)
            {
                Debug.LogError("[SevenFloors] فشل التحميل: " + e.Message);
                return null;
            }
        }

        public static SaveData LoadMostRecent()
        {
            SaveData best = null;
            DateTime bestTime = DateTime.MinValue;
            for (int i = 0; i <= SlotCount; i++)
            {
                if (!HasSave(i)) continue;
                var t = File.GetLastWriteTime(PathFor(i));
                if (t > bestTime) { bestTime = t; best = Load(i); }
            }
            return best;
        }

        public static void Delete(int slot)
        {
            if (HasSave(slot)) File.Delete(PathFor(slot));
        }
    }

    /// <summary>إعدادات اللاعب (أصوات/نص) محفوظة في PlayerPrefs.</summary>
    public static class GameSettings
    {
        public static float MusicVolume = 0.8f;
        public static float SfxVolume = 0.9f;
        public static float VoiceVolume = 1.0f;
        public static float TextSpeed = 40f;   // حرف في الثانية
        public static bool Fullscreen = true;

        public static void Load()
        {
            MusicVolume = PlayerPrefs.GetFloat("sf_music", 0.8f);
            SfxVolume = PlayerPrefs.GetFloat("sf_sfx", 0.9f);
            VoiceVolume = PlayerPrefs.GetFloat("sf_voice", 1.0f);
            TextSpeed = PlayerPrefs.GetFloat("sf_textspeed", 40f);
            Fullscreen = PlayerPrefs.GetInt("sf_fullscreen", 1) == 1;
        }

        public static void Save()
        {
            PlayerPrefs.SetFloat("sf_music", MusicVolume);
            PlayerPrefs.SetFloat("sf_sfx", SfxVolume);
            PlayerPrefs.SetFloat("sf_voice", VoiceVolume);
            PlayerPrefs.SetFloat("sf_textspeed", TextSpeed);
            PlayerPrefs.SetInt("sf_fullscreen", Fullscreen ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void Apply()
        {
            Screen.fullScreen = Fullscreen;
        }
    }
}
