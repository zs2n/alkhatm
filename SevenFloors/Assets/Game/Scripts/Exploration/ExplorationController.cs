using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SevenFloors.Core;
using SevenFloors.Data;
using SevenFloors.Dialogue;

namespace SevenFloors.Exploration
{
    /// <summary>
    /// نظام الاستكشاف بأسلوب Point-and-Click:
    /// خلفية مرسومة لكل موقع + نقاط تفاعل فوقها (أبواب، سلالم، أشياء، شخصيات).
    /// الضغط على باب/درج ينقل لموقع آخر، وعلى شيء يفحصه، وعلى شخصية يبدأ حواراً.
    /// </summary>
    public class ExplorationController : MonoBehaviour
    {
        public static ExplorationController I { get; private set; }

        private Canvas _canvas;
        private Image _bg;
        private RectTransform _hotspotRoot;
        private Text _locationLabel;
        private Image _inspectPanel;
        private Text _inspectText;

        public string CurrentLocationId { get; private set; }

        public static ExplorationController Create()
        {
            var canvas = UIFactory.CreateCanvas("ExplorationCanvas", 100);
            DontDestroyOnLoad(canvas.gameObject);
            var ec = canvas.gameObject.AddComponent<ExplorationController>();
            I = ec;
            ec._canvas = canvas;
            ec.BuildUI();
            ec._canvas.gameObject.SetActive(false);
            return ec;
        }

        private void BuildUI()
        {
            var root = _canvas.transform;

            _bg = UIFactory.CreateImage(root, "Background", null, false);
            UIFactory.Fill(_bg.rectTransform);

            var hr = UIFactory.CreateRect("Hotspots", root);
            UIFactory.Fill(hr);
            _hotspotRoot = hr;

            // اسم الموقع أعلى اليمين
            var plate = UIFactory.CreatePanel(root, "LocationPlate", new Color(0, 0, 0, 0.55f));
            UIFactory.Place(plate.rectTransform, new Vector2(0.70f, 0.92f), new Vector2(0.995f, 0.985f), Vector2.zero, Vector2.zero);
            _locationLabel = UIFactory.CreateText(plate.transform, "LocationName", "", 30, TextAnchor.MiddleRight, true);
            UIFactory.Fill(_locationLabel.rectTransform);
            _locationLabel.rectTransform.offsetMin = new Vector2(10, 0);
            _locationLabel.rectTransform.offsetMax = new Vector2(-16, 0);

            // نافذة الفحص
            _inspectPanel = UIFactory.CreatePanel(root, "InspectPanel", UIFactory.PanelDark);
            UIFactory.Place(_inspectPanel.rectTransform, new Vector2(0.2f, 0.36f), new Vector2(0.8f, 0.64f), Vector2.zero, Vector2.zero);
            var ol = _inspectPanel.gameObject.AddComponent<Outline>();
            ol.effectColor = new Color(0.45f, 0.12f, 0.14f, 0.9f);
            _inspectText = UIFactory.CreateText(_inspectPanel.transform, "InspectText", "", 30, TextAnchor.MiddleRight, false, UIFactory.TextMain);
            UIFactory.Fill(_inspectText.rectTransform);
            _inspectText.rectTransform.offsetMin = new Vector2(24, 20);
            _inspectText.rectTransform.offsetMax = new Vector2(-24, -20);
            var closeBtn = _inspectPanel.gameObject.AddComponent<Button>();
            closeBtn.transition = Selectable.Transition.None;
            closeBtn.onClick.AddListener(() => _inspectPanel.gameObject.SetActive(false));
            _inspectText.raycastTarget = false;
            _inspectPanel.gameObject.SetActive(false);
        }

        public void Show() { _canvas.gameObject.SetActive(true); }
        public void Hide() { _canvas.gameObject.SetActive(false); }

        /// <summary>الانتقال إلى موقع (مع تلاشي).</summary>
        public void MoveTo(string locationId, bool instant = false)
        {
            var loc = GameDatabase.GetLocation(locationId);
            if (loc == null)
            {
                Debug.LogWarning("[SevenFloors] موقع غير موجود: " + locationId);
                return;
            }
            if (instant)
            {
                ApplyLocation(loc);
                return;
            }
            TransitionFX.I.FadeThrough(() => ApplyLocation(loc));
        }

        private void ApplyLocation(LocationDef loc)
        {
            CurrentLocationId = loc.id;
            GameState.CurrentLocationId = loc.id;
            var s = GameDatabase.LoadSprite(loc.bg);
            if (s != null) _bg.sprite = s;
            UIFactory.SetArabic(_locationLabel, loc.name);
            if (!string.IsNullOrEmpty(loc.music)) AudioManager.I.PlayMusic(loc.music);
            _inspectPanel.gameObject.SetActive(false);
            RebuildHotspots(loc);
            if (GameManager.I != null) GameManager.I.NotifyLocationChanged(loc);
        }

        /// <summary>إعادة بناء نقاط التفاعل (تُستدعى أيضاً بعد تغيّر الرايات/اليوم).</summary>
        public void RefreshHotspots()
        {
            var loc = GameDatabase.GetLocation(CurrentLocationId);
            if (loc != null) RebuildHotspots(loc);
        }

        private void RebuildHotspots(LocationDef loc)
        {
            foreach (Transform c in _hotspotRoot) Destroy(c.gameObject);
            int day = GameState.Day;
            foreach (var h in loc.hotspots)
            {
                if (h.minDay > 0 && day < h.minDay) continue;
                if (h.maxDay > 0 && day > h.maxDay) continue;
                if (!string.IsNullOrEmpty(h.requireFlag) && !GameState.CheckFlag(h.requireFlag)) continue;
                CreateHotspot(h);
            }
        }

        private void CreateHotspot(Hotspot h)
        {
            var captured = h;
            var panel = UIFactory.CreatePanel(_hotspotRoot, "HS_" + h.label, new Color(0.9f, 0.85f, 0.7f, 0.06f));
            UIFactory.PlaceNormalized(panel.rectTransform, h.x, h.y, h.w, h.h);
            panel.raycastTarget = true;

            var outline = panel.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.95f, 0.85f, 0.55f, 0.35f);
            outline.effectDistance = new Vector2(2, -2);

            var btn = panel.gameObject.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = new Color(1, 1, 1, 0.35f);
            colors.highlightedColor = Color.white;
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            colors.fadeDuration = 0.1f;
            btn.colors = colors;
            btn.onClick.AddListener(() => OnHotspotClicked(captured));

            // الشارة: أيقونة نوع + التسمية
            string icon = IconFor(h.type);
            var badge = UIFactory.CreatePanel(panel.transform, "Badge", new Color(0, 0, 0, 0.72f));
            UIFactory.Place(badge.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(-110, 6), new Vector2(110, 48));
            badge.raycastTarget = false;
            var label = UIFactory.CreateText(badge.transform, "Label", icon + " " + h.label, 24, TextAnchor.MiddleCenter, false, new Color(0.95f, 0.92f, 0.8f));
            UIFactory.Fill(label.rectTransform);
        }

        private static string IconFor(string type)
        {
            switch (type)
            {
                case "door": return "◻";
                case "stairs": return "▤";
                case "elevator": return "▣";
                case "character": return "●";
                case "tv": return "▢";
                case "bed": return "◆";
                case "exit": return "◀";
                default: return "✦";
            }
        }

        private void OnHotspotClicked(Hotspot h)
        {
            if (TransitionFX.I.Busy || (DialogueManager.I != null && DialogueManager.I.IsRunning)) return;
            if (!string.IsNullOrEmpty(h.setFlag)) GameState.SetFlagExpr(h.setFlag);

            switch (h.type)
            {
                case "door":
                case "stairs":
                case "elevator":
                case "exit":
                    if (!string.IsNullOrEmpty(h.dialogue)) DialogueManager.I.StartDialogue(h.dialogue);
                    else if (!string.IsNullOrEmpty(h.target)) MoveTo(h.target);
                    break;

                case "character":
                    if (!string.IsNullOrEmpty(h.dialogue)) DialogueManager.I.StartDialogue(h.dialogue);
                    break;

                case "tv":
                    GameManager.I.OpenTV();
                    break;

                case "bed":
                    GameManager.I.ConfirmSleep();
                    break;

                default: // object
                    if (!string.IsNullOrEmpty(h.dialogue)) DialogueManager.I.StartDialogue(h.dialogue);
                    else if (!string.IsNullOrEmpty(h.text)) ShowInspect(h.text);
                    break;
            }
            RefreshHotspots();
        }

        private void ShowInspect(string text)
        {
            UIFactory.SetArabic(_inspectText, text, 44);
            _inspectPanel.gameObject.SetActive(true);
            _inspectPanel.transform.SetAsLastSibling();
        }
    }
}
