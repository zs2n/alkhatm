using System;
using System.Collections.Generic;

namespace SevenFloors.Data
{
    // ============================================================
    //  نماذج البيانات — كل محتوى اللعبة يُقرأ من ملفات JSON
    //  Resources/Data/*.json  — عدّل البيانات بدون تعديل الأكواد
    // ============================================================

    [Serializable]
    public class GameConfig
    {
        public string titleAr = "سبعة طوابق";
        public string titleEn = "Seven Floors";
        public string protagonistId = "hero";
        public string startLocation = "apartment_hero";
        public int totalDays = 7;
        public string startDialogue = "";
        public string dayIntroPrefix = "";
    }

    [Serializable]
    public class CharacterDef
    {
        public string id;
        public string name;          // الاسم الظاهر (عربي)
        public string nameColor = "#E8D5A0";
        public bool flipWhenRight = true;   // اقلب الصورة عندما تعرض يميناً
        public bool flipWhenLeft = false;   // اقلب الصورة عندما تعرض يساراً
        public float scale = 1f;
        public string note;          // ملاحظة للمطور فقط
    }

    [Serializable]
    public class CharacterList { public List<CharacterDef> characters = new List<CharacterDef>(); }

    [Serializable]
    public class Hotspot
    {
        // إحداثيات منسوبة لحجم الشاشة: x,y أعلى-يسار (0..1)، w,h (0..1)
        public float x, y, w, h;
        public string label;         // النص الظاهر على النقطة
        public string type;          // door | stairs | elevator | object | character | tv | bed | exit
        public string target;        // معرف الموقع الهدف (door/stairs/elevator)
        public string dialogue;      // معرف حوار يُشغَّل عند الضغط
        public string text;          // نص فحص يظهر في نافذة صغيرة (object)
        public string requireFlag;   // لا يظهر إلا إذا كانت الراية مضبوطة  مثل: "met_oldman" أو "met_oldman=0"
        public string setFlag;       // راية تُضبط عند التفاعل
        public int minDay = 0;       // لا يظهر قبل هذا اليوم (0 = دائماً)
        public int maxDay = 0;       // لا يظهر بعد هذا اليوم (0 = دائماً)
    }

    [Serializable]
    public class LocationDef
    {
        public string id;
        public string name;          // الاسم الظاهر (عربي)
        public int floor;            // رقم الطابق 1..7 (0 = خارج التصنيف)
        public string bg;            // مسار الخلفية داخل Resources مثل Art/Backgrounds/lobby
        public string music;         // موسيقى المكان (اختياري) مثل Audio/Music/ambient1
        public List<Hotspot> hotspots = new List<Hotspot>();
    }

    [Serializable]
    public class LocationList { public List<LocationDef> locations = new List<LocationDef>(); }

    [Serializable]
    public class DayDef
    {
        public int day;
        public string title;         // «اليوم الأول»
        public string intro;         // معرف حوار يعمل تلقائياً عند بداية اليوم
    }

    [Serializable]
    public class DayList { public List<DayDef> days = new List<DayDef>(); }

    [Serializable]
    public class NewsItem
    {
        public int day;
        public string channel = "NHN أخبار";
        public List<string> headlines = new List<string>();
        public string body;
    }

    [Serializable]
    public class NewsList { public List<NewsItem> items = new List<NewsItem>(); }

    [Serializable]
    public class SocialPost
    {
        public int day;
        public string user;
        public string handle;
        public string text;
    }

    [Serializable]
    public class SocialList { public List<SocialPost> posts = new List<SocialPost>(); }

    // ---------------- الحوارات ----------------

    [Serializable]
    public class DialogueChoice
    {
        public string text;
        public string gotoLabel;
        public string setFlag;
    }

    [Serializable]
    public class DialogueLine
    {
        public string label;         // علامة للقفز إليها
        public string sp;            // معرف المتحدث ("" = سرد)
        public string exp;           // تعبير الوجه (default إن فرغ)
        public string side;          // auto | left | right  (auto: البطل يمين والبقية يسار)
        public string text;          // نص السطر
        public bool hide;            // أخفِ المتحدث بعد السطر
        public bool hideAll;         // أخفِ كل الشخصيات
        public string bg;            // غيّر الخلفية  مثل Art/Backgrounds/lobby
        public string cg;            // اعرض مشهد CG فوق الخلفية مثل Art/CG/cg_street_incident ("" يزيله، "-" يزيله)
        public string music;         // شغّل موسيقى ("-" لإيقافها)
        public string sfx;           // مؤثر صوتي
        public string voice;         // تسجيل صوتي للسطر
        public bool shake;           // هزة شاشة
        public string setFlag;       // "name" أو "name=2"
        public string gotoLabel;     // قفزة غير مشروطة بعد هذا السطر
        public string ifFlag;        // نفّذ القفزة فقط إذا تحققت الراية "name" / "name=0"
        public bool end;             // إنهاء الحوار
        public bool advanceDay;      // الانتقال لليوم التالي بعد الحوار
        public List<DialogueChoice> choices = new List<DialogueChoice>();
    }

    [Serializable]
    public class DialogueScript
    {
        public string id;
        public List<DialogueLine> lines = new List<DialogueLine>();
    }

    // ---------------- الحفظ ----------------

    [Serializable]
    public class FlagPair { public string key; public int value; }

    [Serializable]
    public class SaveData
    {
        public int day = 1;
        public string locationId;
        public List<FlagPair> flags = new List<FlagPair>();
        public string timestamp;
        public string caption;       // وصف يظهر في شاشة التحميل
    }
}
