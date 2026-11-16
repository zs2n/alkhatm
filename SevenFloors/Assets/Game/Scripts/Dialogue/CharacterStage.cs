using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SevenFloors.Core;
using SevenFloors.Data;

namespace SevenFloors.Dialogue
{
    /// <summary>
    /// مسرح الشخصيات في الحوار:
    /// - البطل يظهر دائماً في الجهة اليمنى (وتُقلب صورته ليواجه الطرف الآخر).
    /// - بقية الشخصيات تظهر في الجهة اليسرى.
    /// - المتحدث الحالي مضيء وغيره معتم قليلاً.
    /// </summary>
    public class CharacterStage : MonoBehaviour
    {
        private RectTransform _root;
        private readonly Dictionary<string, Image> _slots = new Dictionary<string, Image>(); // "left"/"right" -> صورة
        private readonly Dictionary<string, string> _slotChar = new Dictionary<string, string>(); // الشخصية المعروضة في كل جهة

        private static readonly Color LitColor = Color.white;
        private static readonly Color DimColor = new Color(0.45f, 0.42f, 0.48f, 1f);

        public static CharacterStage Create(Transform parent)
        {
            var rootRt = UIFactory.CreateRect("CharacterStage", parent);
            UIFactory.Fill(rootRt);
            var stage = rootRt.gameObject.AddComponent<CharacterStage>();
            stage._root = rootRt;

            foreach (var side in new[] { "left", "right" })
            {
                var img = UIFactory.CreateImage(rootRt, "Slot_" + side, null, true);
                img.gameObject.SetActive(false);
                var rt = img.rectTransform;
                if (side == "left")
                    UIFactory.Place(rt, new Vector2(0.02f, 0f), new Vector2(0.42f, 0.92f), Vector2.zero, Vector2.zero);
                else
                    UIFactory.Place(rt, new Vector2(0.58f, 0f), new Vector2(0.98f, 0.92f), Vector2.zero, Vector2.zero);
                stage._slots[side] = img;
                stage._slotChar[side] = "";
            }
            return stage;
        }

        /// <summary>تحديد الجهة: البطل يمين، الآخرون يسار (إلا إذا حُدّد side صراحة).</summary>
        public static string ResolveSide(string charId, string side)
        {
            if (side == "left" || side == "right") return side;
            return charId == GameDatabase.Config.protagonistId ? "right" : "left";
        }

        public void Show(string charId, string expression, string side)
        {
            var def = GameDatabase.GetCharacter(charId);
            if (def == null) return;
            side = ResolveSide(charId, side);
            var img = _slots[side];
            var sprite = GameDatabase.LoadCharacterSprite(charId, expression);
            if (sprite == null) return;

            img.sprite = sprite;
            img.gameObject.SetActive(true);
            _slotChar[side] = charId;

            // القلب: البطل في اليمين يُعكس ليواجه يسار الشاشة (الطرف المقابل)
            bool flip = side == "right" ? def.flipWhenRight : def.flipWhenLeft;
            var s = img.rectTransform.localScale;
            img.rectTransform.localScale = new Vector3(Mathf.Abs(s.x) * (flip ? -1f : 1f), s.y, s.z);

            // محاذاة الصورة لأسفل الفتحة مع حفظ النسبة
            img.preserveAspect = true;
        }

        public void Hide(string charId)
        {
            foreach (var side in new[] { "left", "right" })
            {
                if (_slotChar[side] == charId)
                {
                    _slots[side].gameObject.SetActive(false);
                    _slotChar[side] = "";
                }
            }
        }

        public void HideAll()
        {
            foreach (var side in new[] { "left", "right" })
            {
                _slots[side].gameObject.SetActive(false);
                _slotChar[side] = "";
            }
        }

        /// <summary>إضاءة المتحدث وتعتيم الآخر.</summary>
        public void Highlight(string speakerId)
        {
            foreach (var side in new[] { "left", "right" })
            {
                if (!_slots[side].gameObject.activeSelf) continue;
                _slots[side].color = (_slotChar[side] == speakerId || string.IsNullOrEmpty(speakerId))
                    ? LitColor : DimColor;
                // المتحدث فوق
                if (_slotChar[side] == speakerId) _slots[side].transform.SetAsLastSibling();
            }
        }

        public RectTransform Root { get { return _root; } }
    }
}
