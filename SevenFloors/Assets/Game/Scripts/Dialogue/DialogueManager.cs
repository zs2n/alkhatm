using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SevenFloors.Core;
using SevenFloors.Data;

namespace SevenFloors.Dialogue
{
    /// <summary>
    /// مشغّل الحوارات بأسلوب الرواية المرئية:
    /// نص تدريجي، أسماء، تعبيرات، خيارات متفرعة، أوامر (خلفية/CG/موسيقى/هزة/رايات).
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager I { get; private set; }

        public event Action<DialogueScript> OnDialogueEnd;

        private Canvas _canvas;
        private Image _bgOverride;     // خلفية يغيرها الحوار
        private Image _cg;             // صورة مشهد CG
        private CharacterStage _stage;
        private Image _box;
        private Text _nameText;
        private Text _bodyText;
        private Image _namePlate;
        private Text _continueHint;
        private RectTransform _choicesRoot;
        private Image _dimmer;

        private DialogueScript _script;
        private int _index;
        private bool _running;
        private bool _typing;
        private bool _waitingChoice;
        private string _fullTextShaped;
        private Coroutine _typeRoutine;
        private bool _advanceDayAfter;

        public bool IsRunning { get { return _running; } }

        public readonly List<KeyValuePair<string, string>> Backlog = new List<KeyValuePair<string, string>>();

        public static DialogueManager Create()
        {
            var canvas = UIFactory.CreateCanvas("DialogueCanvas", 500);
            DontDestroyOnLoad(canvas.gameObject);
            var dm = canvas.gameObject.AddComponent<DialogueManager>();
            I = dm;
            dm._canvas = canvas;
            dm.BuildUI();
            dm._canvas.gameObject.SetActive(false);
            return dm;
        }

        private void BuildUI()
        {
            var root = _canvas.transform;

            // خلفية يتحكم بها الحوار (فوق خلفية الاستكشاف)
            _bgOverride = UIFactory.CreateImage(root, "DialogueBG", null, false);
            UIFactory.Fill(_bgOverride.rectTransform);
            _bgOverride.gameObject.SetActive(false);

            // معتم خفيف يفصل الاستكشاف عن الحوار
            _dimmer = UIFactory.CreatePanel(root, "Dimmer", new Color(0, 0, 0, 0.25f));
            UIFactory.Fill(_dimmer.rectTransform);
            _dimmer.raycastTarget = false;

            _stage = CharacterStage.Create(root);

            // صورة CG فوق الشخصيات وتحت صندوق النص
            _cg = UIFactory.CreateImage(root, "CG", null, false);
            UIFactory.Fill(_cg.rectTransform);
            _cg.gameObject.SetActive(false);

            // صندوق الحوار
            _box = UIFactory.CreatePanel(root, "DialogueBox", UIFactory.PanelDark);
            UIFactory.Place(_box.rectTransform, new Vector2(0.08f, 0.02f), new Vector2(0.92f, 0.30f), Vector2.zero, Vector2.zero);
            var outline = _box.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.45f, 0.12f, 0.14f, 0.8f);
            outline.effectDistance = new Vector2(2, -2);

            // لوحة الاسم
            _namePlate = UIFactory.CreatePanel(_box.transform, "NamePlate", UIFactory.AccentDim);
            UIFactory.Place(_namePlate.rectTransform, new Vector2(0.72f, 1f), new Vector2(0.995f, 1f),
                new Vector2(0, -8), new Vector2(0, 44));
            _nameText = UIFactory.CreateText(_namePlate.transform, "Name", "", 28, TextAnchor.MiddleCenter, true, UIFactory.NameGold);
            UIFactory.Fill(_nameText.rectTransform);

            // نص الحوار
            _bodyText = UIFactory.CreateText(_box.transform, "Body", "", 34, TextAnchor.UpperRight, false, UIFactory.TextMain);
            UIFactory.Place(_bodyText.rectTransform, new Vector2(0.03f, 0.10f), new Vector2(0.97f, 0.80f), Vector2.zero, Vector2.zero);
            _bodyText.lineSpacing = 1.15f;

            // مؤشر المتابعة
            _continueHint = UIFactory.CreateText(_box.transform, "Hint", "▼", 26, TextAnchor.LowerLeft, false, UIFactory.TextDim);
            UIFactory.Place(_continueHint.rectTransform, new Vector2(0.02f, 0.02f), new Vector2(0.12f, 0.22f), Vector2.zero, Vector2.zero);
            _continueHint.text = "▼";

            // الخيارات
            var choicesRt = UIFactory.CreateRect("Choices", root);
            UIFactory.Place(choicesRt, new Vector2(0.25f, 0.32f), new Vector2(0.75f, 0.85f), Vector2.zero, Vector2.zero);
            var layout = choicesRt.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.spacing = 16;
            layout.childAlignment = TextAnchor.MiddleCenter;
            _choicesRoot = choicesRt;
            _choicesRoot.gameObject.SetActive(false);

            // استقبال النقر على كامل الشاشة للتقدم
            var clickCatcher = UIFactory.CreatePanel(root, "ClickCatcher", new Color(0, 0, 0, 0));
            UIFactory.Fill(clickCatcher.rectTransform);
            clickCatcher.raycastTarget = true;
            var btn = clickCatcher.gameObject.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(Advance);
            clickCatcher.transform.SetSiblingIndex(_box.transform.GetSiblingIndex()); // تحت الصندوق والخيارات
            _box.transform.SetAsLastSibling();
            _choicesRoot.SetAsLastSibling();
        }

        public void StartDialogue(string id)
        {
            var script = GameDatabase.GetDialogue(id);
            if (script == null)
            {
                Debug.LogWarning("[SevenFloors] حوار غير موجود: " + id);
                return;
            }
            _script = script;
            _index = 0;
            _running = true;
            _advanceDayAfter = false;
            _canvas.gameObject.SetActive(true);
            _stage.HideAll();
            _bgOverride.gameObject.SetActive(false);
            SetCg(null);
            ShowCurrent();
        }

        private void Update()
        {
            if (!_running || _waitingChoice) return;
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                Advance();
        }

        private void Advance()
        {
            if (!_running || _waitingChoice) return;
            if (_typing)
            {
                // إكمال النص فوراً
                if (_typeRoutine != null) StopCoroutine(_typeRoutine);
                _typing = false;
                _bodyText.text = _fullTextShaped;
                _continueHint.gameObject.SetActive(true);
                return;
            }
            var line = CurrentLine();
            if (line == null) { EndDialogue(); return; }

            // بعد عرض السطر: نفّذ إخفاءات وقفزات ثم انتقل
            if (line.hideAll) _stage.HideAll();
            else if (line.hide && !string.IsNullOrEmpty(line.sp)) _stage.Hide(line.sp);

            if (line.end) { EndDialogue(); return; }

            if (!string.IsNullOrEmpty(line.gotoLabel) && PassesIf(line.ifFlag))
            {
                JumpTo(line.gotoLabel);
                return;
            }
            _index++;
            ShowCurrent();
        }

        private bool PassesIf(string cond)
        {
            if (string.IsNullOrEmpty(cond)) return true;
            return GameState.CheckFlag(cond);
        }

        private DialogueLine CurrentLine()
        {
            if (_script == null || _index < 0 || _index >= _script.lines.Count) return null;
            return _script.lines[_index];
        }

        private void JumpTo(string label)
        {
            for (int i = 0; i < _script.lines.Count; i++)
            {
                if (_script.lines[i].label == label)
                {
                    _index = i;
                    ShowCurrent();
                    return;
                }
            }
            Debug.LogWarning("[SevenFloors] علامة غير موجودة: " + label);
            EndDialogue();
        }

        private void ShowCurrent()
        {
            var line = CurrentLine();
            if (line == null) { EndDialogue(); return; }

            // أوامر السطر
            if (!string.IsNullOrEmpty(line.bg))
            {
                var s = GameDatabase.LoadSprite(line.bg);
                if (s != null)
                {
                    _bgOverride.sprite = s;
                    _bgOverride.gameObject.SetActive(true);
                }
            }
            if (line.cg != null && line.cg.Length > 0)
                SetCg(line.cg == "-" ? null : GameDatabase.LoadSprite(line.cg));
            if (!string.IsNullOrEmpty(line.music)) AudioManager.I.PlayMusic(line.music == "-" ? "" : "" + line.music);
            if (!string.IsNullOrEmpty(line.sfx)) AudioManager.I.PlaySfx(line.sfx);
            if (!string.IsNullOrEmpty(line.voice)) AudioManager.I.PlayVoice(line.voice);
            if (!string.IsNullOrEmpty(line.setFlag)) GameState.SetFlagExpr(line.setFlag);
            if (line.shake) TransitionFX.I.Shake(_box.rectTransform, 22f, 0.5f);
            if (line.advanceDay) _advanceDayAfter = true;

            // الشخصية
            string displayName = "";
            if (!string.IsNullOrEmpty(line.sp))
            {
                var def = GameDatabase.GetCharacter(line.sp);
                displayName = def != null ? def.name : line.sp;
                _stage.Show(line.sp, line.exp, line.side);
            }
            _stage.Highlight(line.sp);

            // الاسم
            bool hasName = !string.IsNullOrEmpty(displayName);
            _namePlate.gameObject.SetActive(hasName);
            if (hasName) UIFactory.SetArabic(_nameText, displayName);

            // النص
            Backlog.Add(new KeyValuePair<string, string>(displayName, line.text ?? ""));
            if (Backlog.Count > 300) Backlog.RemoveAt(0);

            _fullTextShaped = ArabicText.FixWrapped(line.text ?? "", 62);
            if (_typeRoutine != null) StopCoroutine(_typeRoutine);
            _typeRoutine = StartCoroutine(TypeText());

            // الخيارات تُعرض بعد انتهاء النص
        }

        private void SetCg(Sprite s)
        {
            _cg.sprite = s;
            _cg.gameObject.SetActive(s != null);
        }

        private IEnumerator TypeText()
        {
            _typing = true;
            _continueHint.gameObject.SetActive(false);
            _bodyText.text = "";

            // النص مُشكَّل ومعكوس بصرياً: الكتابة التدريجية تكون من نهاية
            // السلسلة (بداية الجملة بصرياً في اليمين).
            string s = _fullTextShaped;
            float cps = Mathf.Max(10f, GameSettings.TextSpeed);
            float shown = 0;
            // نعرض دائماً الأسطر كاملة البنية: نبني من اليمين لكل سطر
            string[] lines = s.Split('\n');
            var sb = new System.Text.StringBuilder();
            for (int li = 0; li < lines.Length && _typing; li++)
            {
                string lineStr = lines[li];
                for (int i = lineStr.Length - 1; i >= 0 && _typing; i--)
                {
                    shown += 1;
                    sb.Length = 0;
                    for (int k = 0; k < li; k++) { sb.Append(lines[k]); sb.Append('\n'); }
                    sb.Append(lineStr.Substring(i));
                    _bodyText.text = sb.ToString();
                    yield return new WaitForSecondsRealtime(1f / cps);
                }
                if (_typing && li < lines.Length - 1)
                {
                    sb.Append('\n');
                }
            }
            _bodyText.text = s;
            _typing = false;
            _continueHint.gameObject.SetActive(true);
            ShowChoicesIfAny();
        }

        private void ShowChoicesIfAny()
        {
            var line = CurrentLine();
            if (line == null || line.choices == null || line.choices.Count == 0) return;
            _waitingChoice = true;
            _continueHint.gameObject.SetActive(false);
            foreach (Transform c in _choicesRoot) Destroy(c.gameObject);
            _choicesRoot.gameObject.SetActive(true);
            foreach (var choice in line.choices)
            {
                var captured = choice;
                var btn = UIFactory.CreateButton(_choicesRoot, "Choice", choice.text, 30, () =>
                {
                    if (!string.IsNullOrEmpty(captured.setFlag)) GameState.SetFlagExpr(captured.setFlag);
                    _choicesRoot.gameObject.SetActive(false);
                    _waitingChoice = false;
                    Backlog.Add(new KeyValuePair<string, string>("◆", captured.text));
                    if (!string.IsNullOrEmpty(captured.gotoLabel)) JumpTo(captured.gotoLabel);
                    else { _index++; ShowCurrent(); }
                }, UIFactory.PanelMid);
                var le = btn.gameObject.AddComponent<LayoutElement>();
                le.minHeight = 64;
                var ol = btn.gameObject.AddComponent<Outline>();
                ol.effectColor = new Color(0.5f, 0.14f, 0.16f, 0.9f);
            }
        }

        private void EndDialogue()
        {
            _running = false;
            _waitingChoice = false;
            var finished = _script;
            _script = null;
            _stage.HideAll();
            SetCg(null);
            _bgOverride.gameObject.SetActive(false);
            _canvas.gameObject.SetActive(false);
            bool adv = _advanceDayAfter;
            _advanceDayAfter = false;
            if (OnDialogueEnd != null) OnDialogueEnd(finished);
            if (adv) GameManager.I.AdvanceDay();
        }
    }
}
