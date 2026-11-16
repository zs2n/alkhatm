using UnityEngine;
using UnityEngine.UI;
using SevenFloors.Core;

namespace SevenFloors.UI
{
    /// <summary>شاشة البداية والقائمة الرئيسية.</summary>
    public class TitleScreen : MonoBehaviour
    {
        private Canvas _canvas;

        public static TitleScreen Create()
        {
            var canvas = UIFactory.CreateCanvas("TitleCanvas", 800);
            Object.DontDestroyOnLoad(canvas.gameObject);
            var ts = canvas.gameObject.AddComponent<TitleScreen>();
            ts._canvas = canvas;
            ts.BuildUI();
            return ts;
        }

        private void BuildUI()
        {
            var root = _canvas.transform;

            var bgSprite = GameDatabase.LoadSprite("Art/UI/title_bg");
            var bg = UIFactory.CreateImage(root, "TitleBG", bgSprite, false);
            UIFactory.Fill(bg.rectTransform);
            if (bgSprite == null) { bg.sprite = UIFactory.WhiteSprite; bg.color = new Color(0.03f, 0.03f, 0.05f); }

            // العنوان
            var cfg = GameDatabase.Config;
            var titleAr = UIFactory.CreateText(root, "TitleAr", cfg.titleAr, 110, TextAnchor.MiddleCenter, true, UIFactory.TextMain);
            UIFactory.Place(titleAr.rectTransform, new Vector2(0.1f, 0.68f), new Vector2(0.9f, 0.92f), Vector2.zero, Vector2.zero);
            var shadow = titleAr.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.5f, 0.08f, 0.1f, 0.85f);
            shadow.effectDistance = new Vector2(4, -4);

            var titleEn = UIFactory.CreateText(root, "TitleEn", "", 34, TextAnchor.MiddleCenter, false, UIFactory.TextDim);
            titleEn.text = cfg.titleEn; // لاتيني: بدون تشكيل
            UIFactory.Place(titleEn.rectTransform, new Vector2(0.1f, 0.62f), new Vector2(0.9f, 0.70f), Vector2.zero, Vector2.zero);

            var subtitle = UIFactory.CreateText(root, "Subtitle", "عمل قيد التطوير — الاسم غير نهائي", 22, TextAnchor.MiddleCenter, false, UIFactory.TextDim);
            UIFactory.Place(subtitle.rectTransform, new Vector2(0.1f, 0.57f), new Vector2(0.9f, 0.62f), Vector2.zero, Vector2.zero);

            // الأزرار
            var menu = UIFactory.CreateRect("Menu", root);
            UIFactory.Place(menu, new Vector2(0.36f, 0.08f), new Vector2(0.64f, 0.52f), Vector2.zero, Vector2.zero);
            var layout = menu.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.spacing = 18;
            layout.childAlignment = TextAnchor.UpperCenter;

            AddMenuButton(menu, "بداية جديدة", () => GameManager.I.StartNewGame());
            var cont = AddMenuButton(menu, "متابعة", () => GameManager.I.ContinueGame());
            cont.interactable = SaveSystem.AnySaveExists();
            AddMenuButton(menu, "تحميل", () => GameManager.I.OpenSaveLoad(false));
            AddMenuButton(menu, "الإعدادات", () => GameManager.I.OpenSettings());
            AddMenuButton(menu, "خروج", () => GameManager.I.QuitGame());

            var ver = UIFactory.CreateText(root, "Version", "", 18, TextAnchor.LowerLeft, false, UIFactory.TextDim);
            ver.text = "Prototype v0.1";
            UIFactory.Place(ver.rectTransform, new Vector2(0.01f, 0.01f), new Vector2(0.3f, 0.05f), Vector2.zero, Vector2.zero);
        }

        private Button AddMenuButton(Transform parent, string label, UnityEngine.Events.UnityAction action)
        {
            var btn = UIFactory.CreateButton(parent, "Btn_" + label, label, 32, action, new Color(0.08f, 0.08f, 0.12f, 0.92f));
            var le = btn.gameObject.AddComponent<LayoutElement>();
            le.minHeight = 70;
            var ol = btn.gameObject.AddComponent<Outline>();
            ol.effectColor = new Color(0.45f, 0.12f, 0.14f, 0.8f);
            ol.effectDistance = new Vector2(2, -2);
            return btn;
        }

        public void Show()
        {
            // حدّث زر المتابعة
            _canvas.gameObject.SetActive(true);
        }

        public void Hide() { _canvas.gameObject.SetActive(false); }
    }
}
