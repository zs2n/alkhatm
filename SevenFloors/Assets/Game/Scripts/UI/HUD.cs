using UnityEngine;
using UnityEngine.UI;
using SevenFloors.Core;

namespace SevenFloors.UI
{
    /// <summary>
    /// الشريط العلوي أثناء الاستكشاف: عداد الأيام السبعة + أزرار سريعة
    /// (الهاتف، الخريطة، السجل، الحفظ، الإعدادات، القائمة الرئيسية).
    /// </summary>
    public class HUD : MonoBehaviour
    {
        private Canvas _canvas;
        private Text _dayText;
        private Text _countdownText;

        public static HUD Create()
        {
            var canvas = UIFactory.CreateCanvas("HUDCanvas", 300);
            Object.DontDestroyOnLoad(canvas.gameObject);
            var hud = canvas.gameObject.AddComponent<HUD>();
            hud._canvas = canvas;
            hud.BuildUI();
            hud._canvas.gameObject.SetActive(false);
            return hud;
        }

        private void BuildUI()
        {
            var root = _canvas.transform;

            var bar = UIFactory.CreatePanel(root, "TopBar", new Color(0, 0, 0, 0.6f));
            UIFactory.Place(bar.rectTransform, new Vector2(0f, 0.935f), new Vector2(0.56f, 1f), Vector2.zero, Vector2.zero);

            // عداد اليوم
            _dayText = UIFactory.CreateText(bar.transform, "Day", "اليوم 1 من 7", 30, TextAnchor.MiddleRight, true, UIFactory.NameGold);
            UIFactory.Place(_dayText.rectTransform, new Vector2(0.70f, 0f), new Vector2(0.985f, 1f), Vector2.zero, Vector2.zero);

            _countdownText = UIFactory.CreateText(bar.transform, "Countdown", "", 22, TextAnchor.MiddleRight, false, UIFactory.Accent);
            UIFactory.Place(_countdownText.rectTransform, new Vector2(0.44f, 0f), new Vector2(0.69f, 1f), Vector2.zero, Vector2.zero);

            // أزرار
            var btns = UIFactory.CreateRect("Buttons", bar.transform);
            UIFactory.Place(btns, new Vector2(0.005f, 0.08f), new Vector2(0.43f, 0.92f), Vector2.zero, Vector2.zero);
            var layout = btns.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.spacing = 6;

            AddBtn(btns, "الهاتف", () => GameManager.I.OpenPhone());
            AddBtn(btns, "الخريطة", () => GameManager.I.OpenMap());
            AddBtn(btns, "السجل", () => GameManager.I.OpenBacklog());
            AddBtn(btns, "حفظ", () => GameManager.I.OpenSaveLoad(true));
            AddBtn(btns, "الإعدادات", () => GameManager.I.OpenSettings());
            AddBtn(btns, "القائمة", () => GameManager.I.Panels.ShowConfirm(
                "العودة إلى القائمة الرئيسية؟ (يوجد حفظ تلقائي)", () => GameManager.I.ShowTitle()));
        }

        private void AddBtn(Transform parent, string label, UnityEngine.Events.UnityAction action)
        {
            UIFactory.CreateButton(parent, "HudBtn_" + label, label, 20, action, new Color(0.12f, 0.12f, 0.18f, 0.9f));
        }

        public void Show() { _canvas.gameObject.SetActive(true); Refresh(); }
        public void Hide() { _canvas.gameObject.SetActive(false); }

        public void Refresh()
        {
            int day = GameState.Day;
            int total = GameDatabase.Config.totalDays;
            var dayDef = GameDatabase.GetDay(day);
            string title = dayDef != null && !string.IsNullOrEmpty(dayDef.title)
                ? dayDef.title : "اليوم " + day;
            UIFactory.SetArabic(_dayText, title + "  ( " + day + " / " + total + " )");
            int remaining = total - day;
            UIFactory.SetArabic(_countdownText,
                remaining <= 0 ? "اليوم الأخير" : "المتبقي قبل الانهيار: " + remaining + " أيام");
        }
    }
}
