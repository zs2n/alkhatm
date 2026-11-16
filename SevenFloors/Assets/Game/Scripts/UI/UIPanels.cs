using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SevenFloors.Core;
using SevenFloors.Data;
using SevenFloors.Dialogue;
using SevenFloors.Exploration;

namespace SevenFloors.UI
{
    /// <summary>
    /// اللوحات المنبثقة: التلفاز (الأخبار)، الهاتف (التواصل الاجتماعي)،
    /// خريطة الطوابق، سجل الحوارات، الإعدادات، الحفظ/التحميل، والتأكيد.
    /// </summary>
    public class UIPanels : MonoBehaviour
    {
        private Canvas _canvas;
        private Image _blocker;
        private RectTransform _window;
        private Text _windowTitle;
        private RectTransform _content;

        public static UIPanels Create()
        {
            var canvas = UIFactory.CreateCanvas("PanelsCanvas", 700);
            UnityEngine.Object.DontDestroyOnLoad(canvas.gameObject);
            var p = canvas.gameObject.AddComponent<UIPanels>();
            p._canvas = canvas;
            p.BuildUI();
            p._canvas.gameObject.SetActive(false);
            return p;
        }

        private void BuildUI()
        {
            var root = _canvas.transform;

            _blocker = UIFactory.CreatePanel(root, "Blocker", new Color(0, 0, 0, 0.55f));
            UIFactory.Fill(_blocker.rectTransform);
            _blocker.raycastTarget = true;
            var closeOnBg = _blocker.gameObject.AddComponent<Button>();
            closeOnBg.transition = Selectable.Transition.None;
            closeOnBg.onClick.AddListener(CloseAll);

            var win = UIFactory.CreatePanel(root, "Window", UIFactory.PanelMid);
            UIFactory.Place(win.rectTransform, new Vector2(0.14f, 0.08f), new Vector2(0.86f, 0.92f), Vector2.zero, Vector2.zero);
            var ol = win.gameObject.AddComponent<Outline>();
            ol.effectColor = new Color(0.45f, 0.12f, 0.14f, 0.9f);
            ol.effectDistance = new Vector2(3, -3);
            _window = win.rectTransform;

            var titleBar = UIFactory.CreatePanel(win.transform, "TitleBar", new Color(0, 0, 0, 0.5f));
            UIFactory.Place(titleBar.rectTransform, new Vector2(0, 0.92f), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            _windowTitle = UIFactory.CreateText(titleBar.transform, "Title", "", 32, TextAnchor.MiddleRight, true, UIFactory.NameGold);
            UIFactory.Fill(_windowTitle.rectTransform);
            _windowTitle.rectTransform.offsetMax = new Vector2(-20, 0);

            var closeBtn = UIFactory.CreateButton(titleBar.transform, "Close", "✕", 30, CloseAll, UIFactory.AccentDim);
            UIFactory.Place(((RectTransform)closeBtn.transform), new Vector2(0, 0.1f), new Vector2(0.08f, 0.9f),
                new Vector2(8, 0), new Vector2(0, 0));

            var content = UIFactory.CreateRect("Content", win.transform);
            UIFactory.Place(content, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.90f), Vector2.zero, Vector2.zero);
            _content = content;
        }

        private void Open(string title)
        {
            foreach (Transform c in _content) Destroy(c.gameObject);
            UIFactory.SetArabic(_windowTitle, title);
            _canvas.gameObject.SetActive(true);
        }

        public void CloseAll() { _canvas.gameObject.SetActive(false); }

        // ================= التلفاز =================

        public void OpenTV(int day)
        {
            Open("التلفاز — نشرة الأخبار");
            ScrollRect sr;
            var list = UIFactory.CreateScrollList(_content, "NewsList", out sr);
            UIFactory.Fill((RectTransform)list.parent);

            var items = GameDatabase.GetNews(day);
            if (items.Count == 0)
                AddParagraph(list, "لا يوجد بث حالياً... الشاشة تعرض تشويشاً فقط.", UIFactory.TextDim);

            foreach (var n in items)
            {
                var card = AddCard(list);
                var ch = UIFactory.CreateText(card.transform, "Channel", n.channel, 22, TextAnchor.UpperLeft, true, UIFactory.Accent);
                SetCardText(ch, 10, -40, 10, -8);
                float y = 44;
                foreach (var h in n.headlines)
                {
                    var head = UIFactory.CreateText(card.transform, "Headline", "⬤ " + h, 28, TextAnchor.UpperRight, true, UIFactory.TextMain, 52);
                    head.rectTransform.anchorMin = new Vector2(0, 1);
                    head.rectTransform.anchorMax = new Vector2(1, 1);
                    head.rectTransform.offsetMin = new Vector2(14, -y - 70);
                    head.rectTransform.offsetMax = new Vector2(-14, -y);
                    y += 74;
                }
                if (!string.IsNullOrEmpty(n.body))
                {
                    var body = UIFactory.CreateText(card.transform, "Body", n.body, 24, TextAnchor.UpperRight, false, UIFactory.TextDim, 60);
                    body.rectTransform.anchorMin = new Vector2(0, 1);
                    body.rectTransform.anchorMax = new Vector2(1, 1);
                    body.rectTransform.offsetMin = new Vector2(14, -y - 140);
                    body.rectTransform.offsetMax = new Vector2(-14, -y);
                    y += 140;
                }
                SetCardHeight(card, y + 16);
            }
        }

        // ================= الهاتف =================

        public void OpenPhone(int day)
        {
            Open("الهاتف — شبكة التواصل");
            ScrollRect sr;
            var list = UIFactory.CreateScrollList(_content, "Feed", out sr);
            UIFactory.Fill((RectTransform)list.parent);

            var posts = GameDatabase.GetSocial(day);
            if (posts.Count == 0)
                AddParagraph(list, "لا توجد منشورات… الشبكة ضعيفة.", UIFactory.TextDim);

            foreach (var p in posts)
            {
                var card = AddCard(list);
                var user = UIFactory.CreateText(card.transform, "User", p.user, 24, TextAnchor.UpperRight, true, new Color(0.6f, 0.75f, 0.9f));
                SetCardText(user, 14, -40, 14, -6);
                var handle = UIFactory.CreateText(card.transform, "Handle", "", 20, TextAnchor.UpperLeft, false, UIFactory.TextDim);
                handle.text = p.handle ?? "";
                SetCardText(handle, 14, -40, 14, -6);
                var txt = UIFactory.CreateText(card.transform, "Text", p.text, 25, TextAnchor.UpperRight, false, UIFactory.TextMain, 58);
                txt.rectTransform.anchorMin = new Vector2(0, 1);
                txt.rectTransform.anchorMax = new Vector2(1, 1);
                txt.rectTransform.offsetMin = new Vector2(14, -170);
                txt.rectTransform.offsetMax = new Vector2(-14, -46);
                SetCardHeight(card, 180);
            }
        }

        // ================= الخريطة =================

        public void OpenMap()
        {
            Open("العمارة — اختيار الطابق");
            // 7 طوابق من الأعلى إلى الأسفل
            var layoutRt = UIFactory.CreateRect("Floors", _content);
            UIFactory.Fill(layoutRt);
            var layout = layoutRt.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.spacing = 8;
            layout.padding = new RectOffset(30, 30, 10, 10);

            // اجمع المواقع حسب الطابق
            var byFloor = new Dictionary<int, List<LocationDef>>();
            foreach (var loc in GameDatabase.AllLocations())
            {
                if (!byFloor.ContainsKey(loc.floor)) byFloor[loc.floor] = new List<LocationDef>();
                byFloor[loc.floor].Add(loc);
            }

            for (int f = 7; f >= 1; f--)
            {
                var row = UIFactory.CreatePanel(layoutRt, "Floor" + f, new Color(0.07f, 0.07f, 0.11f, 0.95f));
                var rowOl = row.gameObject.AddComponent<Outline>();
                rowOl.effectColor = new Color(0.3f, 0.28f, 0.4f, 0.7f);

                var fl = UIFactory.CreateText(row.transform, "Num", "الطابق " + f, 26, TextAnchor.MiddleRight, true,
                    f == 7 ? UIFactory.NameGold : UIFactory.TextMain);
                UIFactory.Place(fl.rectTransform, new Vector2(0.80f, 0), new Vector2(0.985f, 1), Vector2.zero, Vector2.zero);

                List<LocationDef> locs;
                if (byFloor.TryGetValue(f, out locs))
                {
                    var btnRow = UIFactory.CreateRect("Locs", row.transform);
                    UIFactory.Place(btnRow, new Vector2(0.01f, 0.12f), new Vector2(0.78f, 0.88f), Vector2.zero, Vector2.zero);
                    var h = btnRow.gameObject.AddComponent<HorizontalLayoutGroup>();
                    h.childControlWidth = true;
                    h.childControlHeight = true;
                    h.childForceExpandWidth = true;
                    h.childForceExpandHeight = true;
                    h.spacing = 8;
                    foreach (var loc in locs)
                    {
                        var captured = loc;
                        bool here = loc.id == GameState.CurrentLocationId;
                        var b = UIFactory.CreateButton(btnRow, "Go_" + loc.id,
                            here ? "⬤ " + loc.name : loc.name, 22, () =>
                            {
                                CloseAll();
                                if (captured.id != GameState.CurrentLocationId)
                                    ExplorationController.I.MoveTo(captured.id);
                            }, here ? UIFactory.AccentDim : new Color(0.13f, 0.13f, 0.2f, 0.95f));
                        b.interactable = !here;
                    }
                }
                else
                {
                    var na = UIFactory.CreateText(row.transform, "NA", "— لا يمكن الوصول حالياً —", 20, TextAnchor.MiddleCenter, false, UIFactory.TextDim);
                    UIFactory.Place(na.rectTransform, new Vector2(0.05f, 0), new Vector2(0.78f, 1), Vector2.zero, Vector2.zero);
                }
            }
        }

        // ================= السجل =================

        public void OpenBacklog()
        {
            Open("سجل الحوارات");
            ScrollRect sr;
            var list = UIFactory.CreateScrollList(_content, "Log", out sr);
            UIFactory.Fill((RectTransform)list.parent);

            var backlog = DialogueManager.I.Backlog;
            if (backlog.Count == 0)
                AddParagraph(list, "لا يوجد شيء في السجل بعد.", UIFactory.TextDim);

            foreach (var entry in backlog)
            {
                var card = AddCard(list);
                float top = -8;
                if (!string.IsNullOrEmpty(entry.Key))
                {
                    var nm = UIFactory.CreateText(card.transform, "Who", entry.Key, 22, TextAnchor.UpperRight, true, UIFactory.NameGold);
                    SetCardText(nm, 12, -38, 12, (int)top);
                    top -= 34;
                }
                var tx = UIFactory.CreateText(card.transform, "What", entry.Value, 24, TextAnchor.UpperRight, false, UIFactory.TextMain, 64);
                tx.rectTransform.anchorMin = new Vector2(0, 1);
                tx.rectTransform.anchorMax = new Vector2(1, 1);
                tx.rectTransform.offsetMin = new Vector2(12, top - 76);
                tx.rectTransform.offsetMax = new Vector2(-12, top);
                SetCardHeight(card, -top + 86);
            }
            // مرّر لآخر السجل
            Canvas.ForceUpdateCanvases();
            sr.verticalNormalizedPosition = 0f;
        }

        // ================= الإعدادات =================

        public void OpenSettings()
        {
            Open("الإعدادات");
            var col = UIFactory.CreateRect("Col", _content);
            UIFactory.Place(col, new Vector2(0.08f, 0.05f), new Vector2(0.92f, 0.95f), Vector2.zero, Vector2.zero);
            var layout = col.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.spacing = 22;

            AddSlider(col, "صوت الموسيقى", GameSettings.MusicVolume, v => { GameSettings.MusicVolume = v; AudioManager.I.RefreshVolumes(); });
            AddSlider(col, "صوت المؤثرات", GameSettings.SfxVolume, v => { GameSettings.SfxVolume = v; });
            AddSlider(col, "صوت الشخصيات", GameSettings.VoiceVolume, v => { GameSettings.VoiceVolume = v; });
            AddSlider(col, "سرعة النص", Mathf.InverseLerp(15f, 90f, GameSettings.TextSpeed),
                v => { GameSettings.TextSpeed = Mathf.Lerp(15f, 90f, v); });

            // ملء الشاشة
            var fsRow = UIFactory.CreateRect("FullscreenRow", col);
            var le0 = fsRow.gameObject.AddComponent<LayoutElement>();
            le0.minHeight = 64;
            var fsLabel = UIFactory.CreateText(fsRow, "L", "ملء الشاشة", 28, TextAnchor.MiddleRight, false);
            UIFactory.Place(fsLabel.rectTransform, new Vector2(0.45f, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            Button fsBtn = null;
            fsBtn = UIFactory.CreateButton(fsRow, "Toggle", GameSettings.Fullscreen ? "نعم" : "لا", 26, () =>
            {
                GameSettings.Fullscreen = !GameSettings.Fullscreen;
                GameSettings.Apply();
                var t = fsBtn.GetComponentInChildren<Text>();
                UIFactory.SetArabic(t, GameSettings.Fullscreen ? "نعم" : "لا");
            }, UIFactory.PanelDark);
            UIFactory.Place((RectTransform)fsBtn.transform, new Vector2(0.0f, 0.05f), new Vector2(0.18f, 0.95f), Vector2.zero, Vector2.zero);

            var save = UIFactory.CreateButton(col, "SaveBtn", "حفظ الإعدادات", 28, () =>
            {
                GameSettings.Save();
                CloseAll();
            }, UIFactory.AccentDim);
            var le = save.gameObject.AddComponent<LayoutElement>();
            le.minHeight = 70;
        }

        private void AddSlider(Transform parent, string label, float initial, Action<float> onChange)
        {
            var row = UIFactory.CreateRect("Row_" + label, parent);
            var le = row.gameObject.AddComponent<LayoutElement>();
            le.minHeight = 64;

            var lt = UIFactory.CreateText(row, "L", label, 28, TextAnchor.MiddleRight, false);
            UIFactory.Place(lt.rectTransform, new Vector2(0.62f, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);

            // شريط تمرير مبني يدوياً
            var bgImg = UIFactory.CreatePanel(row, "SliderBG", new Color(0.05f, 0.05f, 0.08f, 1));
            UIFactory.Place(bgImg.rectTransform, new Vector2(0.0f, 0.3f), new Vector2(0.58f, 0.7f), Vector2.zero, Vector2.zero);
            var slider = bgImg.gameObject.AddComponent<Slider>();

            var fillArea = UIFactory.CreateRect("FillArea", bgImg.transform);
            UIFactory.Fill(fillArea);
            var fill = UIFactory.CreatePanel(fillArea, "Fill", UIFactory.Accent);
            UIFactory.Fill(fill.rectTransform);
            fill.raycastTarget = false;

            var handleArea = UIFactory.CreateRect("HandleArea", bgImg.transform);
            UIFactory.Fill(handleArea);
            var handle = UIFactory.CreatePanel(handleArea, "Handle", new Color(0.85f, 0.82f, 0.75f));
            handle.rectTransform.sizeDelta = new Vector2(22, 0);

            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = Mathf.Clamp01(initial);
            slider.onValueChanged.AddListener(v => onChange(v));
        }

        // ================= الحفظ/التحميل =================

        public void OpenSaveLoad(bool saveMode)
        {
            Open(saveMode ? "حفظ اللعبة" : "تحميل اللعبة");
            var grid = UIFactory.CreateRect("Slots", _content);
            UIFactory.Place(grid, new Vector2(0.03f, 0.03f), new Vector2(0.97f, 0.97f), Vector2.zero, Vector2.zero);
            var layout = grid.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(560, 150);
            layout.spacing = new Vector2(20, 20);
            layout.childAlignment = TextAnchor.UpperCenter;

            for (int slot = 0; slot <= SaveSystem.SlotCount; slot++)
            {
                int captured = slot;
                var data = SaveSystem.Load(slot);
                string title = slot == SaveSystem.AutoSlot ? "حفظ تلقائي" : "خانة " + slot;
                string caption = data != null
                    ? (data.caption ?? "") + "\n" + data.timestamp
                    : "— فارغة —";

                bool canClick = saveMode ? slot != SaveSystem.AutoSlot : data != null;
                var card = UIFactory.CreateButton(grid, "Slot" + slot, "", 24, () =>
                {
                    if (saveMode)
                    {
                        SaveSystem.Save(captured, GameState.ToSaveData());
                        OpenSaveLoad(true); // تحديث العرض
                    }
                    else
                    {
                        var d = SaveSystem.Load(captured);
                        if (d != null) GameManager.I.LoadFrom(d);
                    }
                }, data != null ? new Color(0.12f, 0.12f, 0.19f, 0.97f) : new Color(0.07f, 0.07f, 0.1f, 0.9f));
                card.interactable = canClick;
                var ol = card.gameObject.AddComponent<Outline>();
                ol.effectColor = data != null ? new Color(0.5f, 0.14f, 0.16f, 0.9f) : new Color(0.25f, 0.25f, 0.3f, 0.6f);

                var tt = UIFactory.CreateText(card.transform, "T", title, 26, TextAnchor.UpperRight, true, UIFactory.NameGold);
                UIFactory.Place(tt.rectTransform, new Vector2(0.03f, 0.55f), new Vector2(0.97f, 0.95f), Vector2.zero, Vector2.zero);
                var cc = UIFactory.CreateText(card.transform, "C", caption, 20, TextAnchor.UpperRight, false, UIFactory.TextDim, 50);
                UIFactory.Place(cc.rectTransform, new Vector2(0.03f, 0.06f), new Vector2(0.97f, 0.55f), Vector2.zero, Vector2.zero);
            }
        }

        // ================= تأكيد =================

        public void ShowConfirm(string message, Action onYes)
        {
            Open("تأكيد");
            var msg = UIFactory.CreateText(_content, "Msg", message, 32, TextAnchor.MiddleCenter, false, UIFactory.TextMain, 48);
            UIFactory.Place(msg.rectTransform, new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.9f), Vector2.zero, Vector2.zero);

            var yes = UIFactory.CreateButton(_content, "Yes", "نعم", 30, () =>
            {
                CloseAll();
                if (onYes != null) onYes();
            }, UIFactory.AccentDim);
            UIFactory.Place((RectTransform)yes.transform, new Vector2(0.54f, 0.15f), new Vector2(0.78f, 0.30f), Vector2.zero, Vector2.zero);

            var no = UIFactory.CreateButton(_content, "No", "لا", 30, CloseAll, UIFactory.PanelDark);
            UIFactory.Place((RectTransform)no.transform, new Vector2(0.22f, 0.15f), new Vector2(0.46f, 0.30f), Vector2.zero, Vector2.zero);
        }

        // ================= أدوات =================

        private Image AddCard(RectTransform list)
        {
            var card = UIFactory.CreatePanel(list, "Card", new Color(0.08f, 0.08f, 0.13f, 0.95f));
            var ol = card.gameObject.AddComponent<Outline>();
            ol.effectColor = new Color(0.28f, 0.26f, 0.36f, 0.7f);
            SetCardHeight(card, 120);
            return card;
        }

        private void SetCardHeight(Image card, float h)
        {
            var le = card.GetComponent<LayoutElement>();
            if (le == null) le = card.gameObject.AddComponent<LayoutElement>();
            le.minHeight = h;
            le.preferredHeight = h;
        }

        private void SetCardText(Text t, int left, int bottomFromTop, int right, int topOffset)
        {
            t.rectTransform.anchorMin = new Vector2(0, 1);
            t.rectTransform.anchorMax = new Vector2(1, 1);
            t.rectTransform.offsetMin = new Vector2(left, bottomFromTop + topOffset);
            t.rectTransform.offsetMax = new Vector2(-right, topOffset);
        }

        private void AddParagraph(RectTransform list, string text, Color color)
        {
            var t = UIFactory.CreateText(list, "P", text, 26, TextAnchor.MiddleCenter, false, color);
            var le = t.gameObject.AddComponent<LayoutElement>();
            le.minHeight = 90;
        }
    }
}
