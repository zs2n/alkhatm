using System.Collections.Generic;
using UnityEngine;
using SevenFloors.Data;
using SevenFloors.Dialogue;
using SevenFloors.Exploration;
using SevenFloors.UI;

namespace SevenFloors.Core
{
    /// <summary>حالة اللعبة الجارية: اليوم، الموقع، الرايات.</summary>
    public static class GameState
    {
        public static int Day = 1;
        public static string CurrentLocationId = "";
        public static readonly Dictionary<string, int> Flags = new Dictionary<string, int>();

        public static void Reset()
        {
            Day = 1;
            CurrentLocationId = "";
            Flags.Clear();
        }

        /// <summary>"name" → name=1 ، أو "name=3".</summary>
        public static void SetFlagExpr(string expr)
        {
            if (string.IsNullOrEmpty(expr)) return;
            string key = expr;
            int value = 1;
            int eq = expr.IndexOf('=');
            if (eq >= 0)
            {
                key = expr.Substring(0, eq).Trim();
                int.TryParse(expr.Substring(eq + 1).Trim(), out value);
            }
            Flags[key.Trim()] = value;
        }

        /// <summary>"name" → موجودة وغير صفر ، "name=0" → غير موجودة أو صفر ، "name=2" → تساوي 2.</summary>
        public static bool CheckFlag(string expr)
        {
            if (string.IsNullOrEmpty(expr)) return true;
            string key = expr;
            int expected = -1;
            int eq = expr.IndexOf('=');
            if (eq >= 0)
            {
                key = expr.Substring(0, eq).Trim();
                int.TryParse(expr.Substring(eq + 1).Trim(), out expected);
            }
            int val;
            Flags.TryGetValue(key.Trim(), out val);
            if (eq < 0) return val != 0;
            return val == expected;
        }

        public static SaveData ToSaveData()
        {
            var sd = new SaveData { day = Day, locationId = CurrentLocationId };
            foreach (var kv in Flags) sd.flags.Add(new FlagPair { key = kv.Key, value = kv.Value });
            var loc = GameDatabase.GetLocation(CurrentLocationId);
            var dayDef = GameDatabase.GetDay(Day);
            sd.caption = (dayDef != null ? dayDef.title : "اليوم " + Day) + " — " + (loc != null ? loc.name : "");
            return sd;
        }

        public static void FromSaveData(SaveData sd)
        {
            Reset();
            Day = sd.day;
            CurrentLocationId = sd.locationId;
            foreach (var f in sd.flags) Flags[f.key] = f.value;
        }
    }

    /// <summary>
    /// المدير العام: دورة اللعبة (قائمة ← استكشاف ← حوار)، نظام الأيام السبعة،
    /// الحفظ/التحميل، وفتح اللوحات (تلفاز/هاتف/خريطة...).
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager I { get; private set; }

        public TitleScreen Title { get; private set; }
        public HUD Hud { get; private set; }
        public UIPanels Panels { get; private set; }

        public static GameManager Create()
        {
            var go = new GameObject("GameManager");
            DontDestroyOnLoad(go);
            var gm = go.AddComponent<GameManager>();
            I = gm;
            return gm;
        }

        public void Boot()
        {
            GameSettings.Load();
            GameSettings.Apply();
            GameDatabase.LoadAll();

            AudioManager.Create();
            TransitionFX.Create();
            ExplorationController.Create();
            DialogueManager.Create();
            Panels = UIPanels.Create();
            Hud = HUD.Create();
            Title = TitleScreen.Create();

            DialogueManager.I.OnDialogueEnd += OnDialogueEnded;

            ShowTitle();
            TransitionFX.I.FadeIn(1.2f);
        }

        // ---------------- القائمة الرئيسية ----------------

        public void ShowTitle()
        {
            ExplorationController.I.Hide();
            Hud.Hide();
            Title.Show();
        }

        public void StartNewGame()
        {
            GameState.Reset();
            Title.Hide();
            Hud.Show();
            ExplorationController.I.Show();
            ExplorationController.I.MoveTo(GameDatabase.Config.startLocation, true);
            Hud.Refresh();

            var cfg = GameDatabase.Config;
            if (!string.IsNullOrEmpty(cfg.startDialogue))
                DialogueManager.I.StartDialogue(cfg.startDialogue);
            else
                RunDayIntro();
            Autosave();
        }

        public void ContinueGame()
        {
            var sd = SaveSystem.LoadMostRecent();
            if (sd == null) return;
            LoadFrom(sd);
        }

        public void LoadFrom(SaveData sd)
        {
            GameState.FromSaveData(sd);
            Title.Hide();
            Panels.CloseAll();
            Hud.Show();
            ExplorationController.I.Show();
            ExplorationController.I.MoveTo(
                string.IsNullOrEmpty(sd.locationId) ? GameDatabase.Config.startLocation : sd.locationId, true);
            Hud.Refresh();
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ---------------- نظام الأيام ----------------

        public void AdvanceDay()
        {
            int next = GameState.Day + 1;
            if (next > GameDatabase.Config.totalDays)
            {
                // نهاية الأيام السبعة — نهاية مؤقتة قابلة للاستبدال
                var endingId = "ending_placeholder";
                if (GameDatabase.GetDialogue(endingId) != null)
                    DialogueManager.I.StartDialogue(endingId);
                else
                    ShowTitle();
                return;
            }
            TransitionFX.I.FadeThrough(() =>
            {
                GameState.Day = next;
                Hud.Refresh();
                ExplorationController.I.RefreshHotspots();
            }, 0.8f, 0.8f);
            // شغّل افتتاحية اليوم بعد الانتقال بوهلة قصيرة
            Invoke("RunDayIntro", 1.7f);
            Autosave();
        }

        private void RunDayIntro()
        {
            var dayDef = GameDatabase.GetDay(GameState.Day);
            if (dayDef != null && !string.IsNullOrEmpty(dayDef.intro)
                && GameDatabase.GetDialogue(dayDef.intro) != null)
            {
                DialogueManager.I.StartDialogue(dayDef.intro);
            }
        }

        public void ConfirmSleep()
        {
            Panels.ShowConfirm("هل تريد إنهاء هذا اليوم والنوم حتى الغد؟", AdvanceDay);
        }

        // ---------------- لوحات ----------------

        public void OpenTV() { Panels.OpenTV(GameState.Day); }
        public void OpenPhone() { Panels.OpenPhone(GameState.Day); }
        public void OpenMap() { Panels.OpenMap(); }
        public void OpenBacklog() { Panels.OpenBacklog(); }
        public void OpenSettings() { Panels.OpenSettings(); }
        public void OpenSaveLoad(bool saveMode) { Panels.OpenSaveLoad(saveMode); }

        // ---------------- أحداث ----------------

        private void OnDialogueEnded(DialogueScript script)
        {
            // نهاية من النهايات → العودة للقائمة الرئيسية
            if (script != null && !string.IsNullOrEmpty(script.id) && script.id.StartsWith("ending"))
            {
                TransitionFX.I.FadeThrough(() => ShowTitle(), 1.2f, 1.2f);
                return;
            }
            ExplorationController.I.RefreshHotspots();
            Hud.Refresh();
            Autosave();
        }

        public void NotifyLocationChanged(LocationDef loc)
        {
            if (Hud != null) Hud.Refresh();
        }

        public void Autosave()
        {
            if (!string.IsNullOrEmpty(GameState.CurrentLocationId))
                SaveSystem.Save(SaveSystem.AutoSlot, GameState.ToSaveData());
        }
    }
}
