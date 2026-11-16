using System.Collections.Generic;
using UnityEngine;
using SevenFloors.Data;

namespace SevenFloors.Core
{
    /// <summary>
    /// قاعدة بيانات اللعبة: تقرأ كل المحتوى من Resources/Data (ملفات JSON)
    /// والأصول الفنية من Resources/Art و Resources/Audio.
    /// أضف ملفاتك في تلك المجلدات ثم أشر إليها من JSON — بدون تعديل أكواد.
    /// </summary>
    public static class GameDatabase
    {
        public static GameConfig Config { get; private set; }

        private static readonly Dictionary<string, CharacterDef> _characters = new Dictionary<string, CharacterDef>();
        private static readonly Dictionary<string, LocationDef> _locations = new Dictionary<string, LocationDef>();
        private static readonly Dictionary<int, DayDef> _days = new Dictionary<int, DayDef>();
        private static readonly List<NewsItem> _news = new List<NewsItem>();
        private static readonly List<SocialPost> _social = new List<SocialPost>();
        private static readonly Dictionary<string, DialogueScript> _dialogueCache = new Dictionary<string, DialogueScript>();
        private static readonly Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();
        private static bool _loaded;

        public static void LoadAll()
        {
            if (_loaded) return;
            _loaded = true;

            Config = LoadJson<GameConfig>("Data/game") ?? new GameConfig();

            var chars = LoadJson<CharacterList>("Data/characters");
            if (chars != null)
                foreach (var c in chars.characters)
                    _characters[c.id] = c;

            var locs = LoadJson<LocationList>("Data/locations");
            if (locs != null)
                foreach (var l in locs.locations)
                    _locations[l.id] = l;

            var days = LoadJson<DayList>("Data/days");
            if (days != null)
                foreach (var d in days.days)
                    _days[d.day] = d;

            var news = LoadJson<NewsList>("Data/news");
            if (news != null) _news.AddRange(news.items);

            var social = LoadJson<SocialList>("Data/social");
            if (social != null) _social.AddRange(social.posts);

            Debug.Log(string.Format("[SevenFloors] DB loaded: {0} characters, {1} locations, {2} days",
                _characters.Count, _locations.Count, _days.Count));
        }

        private static T LoadJson<T>(string path) where T : class
        {
            var ta = Resources.Load<TextAsset>(path);
            if (ta == null)
            {
                Debug.LogWarning("[SevenFloors] JSON غير موجود: Resources/" + path);
                return null;
            }
            try { return JsonUtility.FromJson<T>(ta.text); }
            catch (System.Exception e)
            {
                Debug.LogError("[SevenFloors] خطأ في قراءة " + path + ": " + e.Message);
                return null;
            }
        }

        public static CharacterDef GetCharacter(string id)
        {
            CharacterDef c;
            if (!string.IsNullOrEmpty(id) && _characters.TryGetValue(id, out c)) return c;
            return null;
        }

        public static LocationDef GetLocation(string id)
        {
            LocationDef l;
            if (!string.IsNullOrEmpty(id) && _locations.TryGetValue(id, out l)) return l;
            return null;
        }

        public static IEnumerable<LocationDef> AllLocations() { return _locations.Values; }

        public static DayDef GetDay(int day)
        {
            DayDef d;
            return _days.TryGetValue(day, out d) ? d : null;
        }

        public static List<NewsItem> GetNews(int day)
        {
            var list = new List<NewsItem>();
            foreach (var n in _news) if (n.day == day || n.day == 0) list.Add(n);
            return list;
        }

        public static List<SocialPost> GetSocial(int day)
        {
            var list = new List<SocialPost>();
            foreach (var p in _social) if (p.day == day || p.day == 0) list.Add(p);
            return list;
        }

        /// <summary>تحميل حوار من Resources/Data/Dialogues/{id}.json</summary>
        public static DialogueScript GetDialogue(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            DialogueScript cached;
            if (_dialogueCache.TryGetValue(id, out cached)) return cached;
            var script = LoadJson<DialogueScript>("Data/Dialogues/" + id);
            if (script != null) _dialogueCache[id] = script;
            return script;
        }

        /// <summary>تحميل صورة: المسار نسبي لمجلد Resources مثل Art/Characters/hero/default</summary>
        public static Sprite LoadSprite(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            Sprite s;
            if (_spriteCache.TryGetValue(path, out s)) return s;
            s = Resources.Load<Sprite>(path);
            if (s == null)
            {
                // قد تكون الصورة مستوردة كـ Texture2D فقط
                var tex = Resources.Load<Texture2D>(path);
                if (tex != null)
                    s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            }
            if (s == null) Debug.LogWarning("[SevenFloors] صورة غير موجودة: Resources/" + path);
            _spriteCache[path] = s;
            return s;
        }

        /// <summary>صورة شخصية: Art/Characters/{id}/{expression} مع رجوع إلى default.</summary>
        public static Sprite LoadCharacterSprite(string charId, string expression)
        {
            if (string.IsNullOrEmpty(expression)) expression = "default";
            var s = LoadSprite("Art/Characters/" + charId + "/" + expression);
            if (s == null && expression != "default")
                s = LoadSprite("Art/Characters/" + charId + "/default");
            return s;
        }
    }
}
