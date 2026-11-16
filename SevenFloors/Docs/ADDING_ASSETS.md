# دليل إضافة الأصول الفنية والصوتية

كل شيء داخل `Assets/Game/Resources/` — وجود الملف هناك يكفي ليستطيع المحرك تحميله بالاسم.
المسارات المكتوبة في ملفات JSON تكون **نسبية لمجلد Resources وبدون امتداد الملف**.

## 1) صور الشخصيات

المجلد: `Assets/Game/Resources/Art/Characters/<معرف الشخصية>/`

- اسم الملف = اسم التعبير: `default.png`, `angry.png`, `sad.png`, `smile.png`, `scared.png`...
- `default.png` **إلزامي** لكل شخصية (يُستخدم عند عدم تحديد تعبير أو غياب الملف المطلوب).
- الصيغة: PNG بخلفية شفافة، بطول لا يقل عن 1000 بكسل تقريباً.
- الشخصيات الحالية:
  - `hero/` — البطل (يُعرض يميناً ومقلوباً تلقائياً).
  - `old_man/` — الجار العجوز (لديه `angry.png` و`default.png`).
  - `neighbor_woman/` — الجارة.

**لإضافة شخصية جديدة:**
1. أنشئ مجلداً باسم معرفها اللاتيني، مثل `Art/Characters/tanaka_kid/` وضع فيه `default.png`.
2. أضف تعريفها في `Data/characters.json`:
```json
{
  "id": "tanaka_kid",
  "name": "الطفل",
  "nameColor": "#A8D8A8",
  "flipWhenRight": false,
  "flipWhenLeft": false,
  "scale": 1.0,
  "note": "ملاحظة اختيارية"
}
```
- `flipWhenRight`: اجعلها `true` إذا كانت الصورة الأصلية تنظر لليمين وتريد قلبها عند عرضها في الجهة اليمنى (مثل البطل) حتى تواجه محدّثها.
- قاعدة الجهات: البطل (`protagonistId` في `game.json`) يُعرض دائماً يميناً، وكل شخصية أخرى يساراً، ويمكن كسر القاعدة في سطر حوار محدد بحقل `"side": "left"` أو `"right"`.

## 2) خلفيات المواقع

المجلد: `Assets/Game/Resources/Art/Backgrounds/`

- الدقة المقترحة: **1920×1080** (أو أي 16:9).
- اربط الخلفية بالموقع في `Data/locations.json`: `"bg": "Art/Backgrounds/my_room"`.
- الخلفيات الحالية كلها مؤقتة مولدة برمجياً — استبدلها بنفس الأسماء وستعمل فوراً:
  `lobby.png` ، `corridor_3f.png` ، `apartment_hero.png` ، `stairwell_3f.png` ، `rooftop.png`

## 3) صور المشاهد الخاصة (CG)

المجلد: `Assets/Game/Resources/Art/CG/`

- نفس دقة الخلفيات. تُعرض فوق كل شيء أثناء الحوار.
- تُستدعى من سطر حوار: `"cg": "Art/CG/cg_street_incident"` وتُزال بـ `"cg": "-"`.
- الموجود حالياً: `cg_street_incident.png` (مؤقتة — مشهد الحادثة).

## 4) واجهة المستخدم

المجلد: `Assets/Game/Resources/Art/UI/`

- `title_bg.png` خلفية شاشة البداية (مؤقتة حالياً — استبدلها بنفس الاسم).

## 5) الموسيقى

المجلد: `Assets/Game/Resources/Audio/Music/`

- الصيغ المدعومة: `.ogg` (الأفضل) ، `.mp3` ، `.wav`.
- موسيقى موقع: في `locations.json` → `"music": "Audio/Music/corridor_theme"` (تعمل تلقائياً عند دخول الموقع مع تلاشٍ متقاطع).
- موسيقى داخل حوار: `"music": "Audio/Music/tension"` في أي سطر، و`"music": "-"` لإيقافها.

## 6) المؤثرات الصوتية

المجلد: `Assets/Game/Resources/Audio/SFX/`

- من أي سطر حوار: `"sfx": "Audio/SFX/door_slam"`.
- اقتراحات أسماء: `door_open`, `door_slam`, `footsteps`, `glass_break`, `heartbeat`, `scream_far`.

## 7) تسجيلات أصوات الشخصيات (دبلجة)

المجلد: `Assets/Game/Resources/Audio/Voice/`

- نظّمها بمجلد لكل شخصية: `Audio/Voice/hero/line_001.ogg`.
- من سطر الحوار: `"voice": "Audio/Voice/hero/line_001"` — يُشغَّل مع ظهور السطر.

## 8) الخطوط

المجلد: `Assets/Game/Resources/Fonts/`

- المضمّن حالياً: `NotoNaskhArabic-Regular.ttf` و`NotoNaskhArabic-Bold.ttf`.
- لاستبدال الخط ضع ملفي TTF بنفس الاسمين (يجب أن يحتوي الخط على حروف عربية بأشكال العرض Presentation Forms).

## ملاحظات عامة

- غياب أي ملف لا يوقف اللعبة؛ ستجد تحذيراً أصفر في نافذة Console يدلك على المسار الناقص.
- بعد وضع أي ملف جديد، عُد إلى نافذة Unity وانتظر ثانية حتى يستورده (يظهر شريط تقدم صغير).
- لا تغيّر أسماء المجلدات الرئيسية (`Resources`, `Art`, `Audio`, `Data`) — الأنظمة تعتمد عليها.
