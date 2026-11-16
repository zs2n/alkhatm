using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace SevenFloors.Core
{
    /// <summary>
    /// مصنع عناصر الواجهة: كل الواجهات تُبنى برمجياً من هنا،
    /// حتى لا نحتاج إلى Prefabs يدوية في مرحلة النموذج الأولي.
    /// </summary>
    public static class UIFactory
    {
        private static Font _font, _fontBold;
        private static Sprite _white;

        public static readonly Color PanelDark = new Color(0.04f, 0.04f, 0.07f, 0.92f);
        public static readonly Color PanelMid = new Color(0.10f, 0.10f, 0.16f, 0.97f);
        public static readonly Color Accent = new Color(0.62f, 0.13f, 0.16f);      // أحمر دموي خافت
        public static readonly Color AccentDim = new Color(0.36f, 0.10f, 0.12f);
        public static readonly Color TextMain = new Color(0.90f, 0.88f, 0.84f);
        public static readonly Color TextDim = new Color(0.58f, 0.56f, 0.55f);
        public static readonly Color NameGold = new Color(0.91f, 0.84f, 0.63f);

        public static Font FontRegular
        {
            get
            {
                if (_font == null)
                {
                    _font = Resources.Load<Font>("Fonts/NotoNaskhArabic-Regular");
                    if (_font == null) _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                }
                return _font;
            }
        }

        public static Font FontBold
        {
            get
            {
                if (_fontBold == null)
                {
                    _fontBold = Resources.Load<Font>("Fonts/NotoNaskhArabic-Bold");
                    if (_fontBold == null) _fontBold = FontRegular;
                }
                return _fontBold;
            }
        }

        public static Sprite WhiteSprite
        {
            get
            {
                if (_white == null)
                {
                    var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                    var cols = new Color32[16];
                    for (int i = 0; i < 16; i++) cols[i] = new Color32(255, 255, 255, 255);
                    tex.SetPixels32(cols);
                    tex.Apply();
                    _white = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
                }
                return _white;
            }
        }

        public static Canvas CreateCanvas(string name, int sortOrder)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        public static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            return rt;
        }

        public static RectTransform Fill(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        public static Image CreatePanel(Transform parent, string name, Color color)
        {
            var rt = CreateRect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = WhiteSprite;
            img.type = Image.Type.Sliced;
            img.color = color;
            return img;
        }

        /// <summary>نص عربي: يشكّل تلقائياً ويحاذي يميناً.</summary>
        public static Text CreateText(Transform parent, string name, string arabic, int size,
            TextAnchor anchor = TextAnchor.UpperRight, bool bold = false, Color? color = null, int wrapChars = 0)
        {
            var rt = CreateRect(name, parent);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = bold ? FontBold : FontRegular;
            t.fontSize = size;
            t.alignment = anchor;
            t.color = color ?? TextMain;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            SetArabic(t, arabic, wrapChars);
            return t;
        }

        public static void SetArabic(Text t, string arabic, int wrapChars = 0)
        {
            if (string.IsNullOrEmpty(arabic)) { t.text = ""; return; }
            t.text = wrapChars > 0 ? ArabicText.FixWrapped(arabic, wrapChars) : ArabicText.Fix(arabic);
        }

        public static Button CreateButton(Transform parent, string name, string label, int fontSize,
            UnityAction onClick, Color? bg = null, Color? fg = null)
        {
            var img = CreatePanel(parent, name, bg ?? PanelMid);
            var btn = img.gameObject.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.35f, 1.3f, 1.3f, 1f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            colors.fadeDuration = 0.08f;
            btn.colors = colors;
            if (onClick != null) btn.onClick.AddListener(onClick);

            var txt = CreateText(img.transform, "Label", label, fontSize, TextAnchor.MiddleCenter, false, fg ?? TextMain);
            Fill(txt.rectTransform);
            return btn;
        }

        public static void Place(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
        }

        /// <summary>وضع عنصر بإحداثيات منسوبة (0..1) حيث y من الأعلى.</summary>
        public static void PlaceNormalized(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = new Vector2(x, 1f - y - h);
            rt.anchorMax = new Vector2(x + w, 1f - y);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        public static Image CreateImage(Transform parent, string name, Sprite sprite, bool preserveAspect = true)
        {
            var rt = CreateRect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite != null ? sprite : WhiteSprite;
            img.preserveAspect = preserveAspect;
            img.raycastTarget = false;
            return img;
        }

        /// <summary>قائمة قابلة للتمرير؛ تعيد الـ content الذي تُضاف إليه العناصر.</summary>
        public static RectTransform CreateScrollList(Transform parent, string name, out ScrollRect scroll)
        {
            var viewportImg = CreatePanel(parent, name, new Color(0, 0, 0, 0.25f));
            var vpGo = viewportImg.gameObject;
            scroll = vpGo.AddComponent<ScrollRect>();
            vpGo.AddComponent<RectMask2D>();
            viewportImg.raycastTarget = true;

            var content = CreateRect("Content", vpGo.transform);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1f);
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;

            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 10;
            layout.padding = new RectOffset(16, 16, 12, 12);
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = content;
            scroll.viewport = vpGo.GetComponent<RectTransform>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 35;
            return content;
        }
    }
}
