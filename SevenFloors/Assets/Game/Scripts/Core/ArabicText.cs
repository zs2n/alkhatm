using System.Collections.Generic;
using System.Text;

namespace SevenFloors.Core
{
    /// <summary>
    /// تشكيل النص العربي للعرض في Unity UI (أشكال العرض + عكس الاتجاه).
    /// مكوّنات Text في Unity لا تدعم العربية مباشرة، فهذه الأداة تحوّل
    /// النص المنطقي إلى نص بصري جاهز للعرض بمحاذاة يمين.
    /// </summary>
    public static class ArabicText
    {
        // لكل حرف: [منفصل، نهائي، ابتدائي، وسطي] (0 = لا يوجد)
        private static readonly Dictionary<char, int[]> Forms = new Dictionary<char, int[]>
        {
            { 'ء', new[] { 0xFE80, 0, 0, 0 } },          // ء
            { 'آ', new[] { 0xFE81, 0xFE82, 0, 0 } },      // آ
            { 'أ', new[] { 0xFE83, 0xFE84, 0, 0 } },      // أ
            { 'ؤ', new[] { 0xFE85, 0xFE86, 0, 0 } },      // ؤ
            { 'إ', new[] { 0xFE87, 0xFE88, 0, 0 } },      // إ
            { 'ئ', new[] { 0xFE89, 0xFE8A, 0xFE8B, 0xFE8C } }, // ئ
            { 'ا', new[] { 0xFE8D, 0xFE8E, 0, 0 } },      // ا
            { 'ب', new[] { 0xFE8F, 0xFE90, 0xFE91, 0xFE92 } }, // ب
            { 'ة', new[] { 0xFE93, 0xFE94, 0, 0 } },      // ة
            { 'ت', new[] { 0xFE95, 0xFE96, 0xFE97, 0xFE98 } }, // ت
            { 'ث', new[] { 0xFE99, 0xFE9A, 0xFE9B, 0xFE9C } }, // ث
            { 'ج', new[] { 0xFE9D, 0xFE9E, 0xFE9F, 0xFEA0 } }, // ج
            { 'ح', new[] { 0xFEA1, 0xFEA2, 0xFEA3, 0xFEA4 } }, // ح
            { 'خ', new[] { 0xFEA5, 0xFEA6, 0xFEA7, 0xFEA8 } }, // خ
            { 'د', new[] { 0xFEA9, 0xFEAA, 0, 0 } },      // د
            { 'ذ', new[] { 0xFEAB, 0xFEAC, 0, 0 } },      // ذ
            { 'ر', new[] { 0xFEAD, 0xFEAE, 0, 0 } },      // ر
            { 'ز', new[] { 0xFEAF, 0xFEB0, 0, 0 } },      // ز
            { 'س', new[] { 0xFEB1, 0xFEB2, 0xFEB3, 0xFEB4 } }, // س
            { 'ش', new[] { 0xFEB5, 0xFEB6, 0xFEB7, 0xFEB8 } }, // ش
            { 'ص', new[] { 0xFEB9, 0xFEBA, 0xFEBB, 0xFEBC } }, // ص
            { 'ض', new[] { 0xFEBD, 0xFEBE, 0xFEBF, 0xFEC0 } }, // ض
            { 'ط', new[] { 0xFEC1, 0xFEC2, 0xFEC3, 0xFEC4 } }, // ط
            { 'ظ', new[] { 0xFEC5, 0xFEC6, 0xFEC7, 0xFEC8 } }, // ظ
            { 'ع', new[] { 0xFEC9, 0xFECA, 0xFECB, 0xFECC } }, // ع
            { 'غ', new[] { 0xFECD, 0xFECE, 0xFECF, 0xFED0 } }, // غ
            { 'ـ', new[] { 0x0640, 0x0640, 0x0640, 0x0640 } }, // ـ
            { 'ف', new[] { 0xFED1, 0xFED2, 0xFED3, 0xFED4 } }, // ف
            { 'ق', new[] { 0xFED5, 0xFED6, 0xFED7, 0xFED8 } }, // ق
            { 'ك', new[] { 0xFED9, 0xFEDA, 0xFEDB, 0xFEDC } }, // ك
            { 'ل', new[] { 0xFEDD, 0xFEDE, 0xFEDF, 0xFEE0 } }, // ل
            { 'م', new[] { 0xFEE1, 0xFEE2, 0xFEE3, 0xFEE4 } }, // م
            { 'ن', new[] { 0xFEE5, 0xFEE6, 0xFEE7, 0xFEE8 } }, // ن
            { 'ه', new[] { 0xFEE9, 0xFEEA, 0xFEEB, 0xFEEC } }, // ه
            { 'و', new[] { 0xFEED, 0xFEEE, 0, 0 } },      // و
            { 'ى', new[] { 0xFEEF, 0xFEF0, 0, 0 } },      // ى
            { 'ي', new[] { 0xFEF1, 0xFEF2, 0xFEF3, 0xFEF4 } }, // ي
        };

        // الحروف التي تتصل بما بعدها (ثنائية الاتصال)
        private static bool JoinsNext(char c)
        {
            int[] f;
            return Forms.TryGetValue(c, out f) && f[2] != 0;
        }

        // الحروف العربية التي تقبل الاتصال بما قبلها
        private static bool JoinsPrev(char c) { return Forms.ContainsKey(c) && c != 'ء'; }

        private static bool IsTashkeel(char c) { return c >= 'ً' && c <= 'ْ'; }

        private static bool IsArabicLetter(char c)
        {
            return (c >= '؀' && c <= 'ۿ') || (c >= 'ﭐ' && c <= 'ﻼ');
        }

        /// <summary>تحويل نص منطقي إلى نص بصري جاهز للعرض (سطر واحد).</summary>
        public static string Fix(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            string shaped = Shape(input);
            return ReverseVisual(shaped);
        }

        /// <summary>
    /// تحويل مع لفّ أسطر يدوي — مطلوب لأن Unity يلفّ النص المعكوس بترتيب خاطئ.
        /// maxChars: عدد تقريبي للحروف في السطر الواحد.
        /// </summary>
        public static string FixWrapped(string input, int maxChars)
        {
            if (string.IsNullOrEmpty(input)) return input;
            if (maxChars <= 8) maxChars = 8;
            var lines = new List<string>();
            foreach (string hardLine in input.Replace("\r", "").Split('\n'))
            {
                var current = new StringBuilder();
                int count = 0;
                foreach (string word in hardLine.Split(' '))
                {
                    int wlen = word.Length;
                    if (count > 0 && count + 1 + wlen > maxChars)
                    {
                        lines.Add(current.ToString());
                        current.Length = 0;
                        count = 0;
                    }
                    if (count > 0) { current.Append(' '); count++; }
                    current.Append(word);
                    count += wlen;
                }
                lines.Add(current.ToString());
            }
            var sb = new StringBuilder();
            for (int i = 0; i < lines.Count; i++)
            {
                if (i > 0) sb.Append('\n');
                sb.Append(Fix(lines[i]));
            }
            return sb.ToString();
        }

        private static string Shape(string s)
        {
            var sb = new StringBuilder(s.Length);
            int n = s.Length;
            for (int i = 0; i < n; i++)
            {
                char c = s[i];
                int[] f;
                if (!Forms.TryGetValue(c, out f)) { sb.Append(c); continue; }

                // لام-ألف
                if (c == 'ل' && i + 1 < n)
                {
                    char nx = NextLetter(s, i);
                    int lig = 0;
                    if (nx == 'ا') lig = 0xFEFB;
                    else if (nx == 'أ') lig = 0xFEF7;
                    else if (nx == 'إ') lig = 0xFEF9;
                    else if (nx == 'آ') lig = 0xFEF5;
                    if (lig != 0)
                    {
                        bool prevJoins0 = PrevJoins(s, i);
                        sb.Append((char)(prevJoins0 ? lig + 1 : lig));
                        // تخطَّ الألف (مع أي تشكيل بينهما يُنقل)
                        int j = i + 1;
                        while (j < n && IsTashkeel(s[j])) { sb.Append(s[j]); j++; }
                        i = j; // موضع الألف نفسه يُستهلك
                        continue;
                    }
                }

                bool prevJoins = PrevJoins(s, i);
                bool nextJoins = NextJoins(s, i) && JoinsNext(c);
                int form;
                if (prevJoins && nextJoins) form = f[3] != 0 ? f[3] : (f[1] != 0 ? f[1] : f[0]);
                else if (prevJoins) form = f[1] != 0 ? f[1] : f[0];
                else if (nextJoins) form = f[2] != 0 ? f[2] : f[0];
                else form = f[0];
                sb.Append((char)form);
            }
            return sb.ToString();
        }

        private static char NextLetter(string s, int i)
        {
            for (int j = i + 1; j < s.Length; j++)
            {
                if (IsTashkeel(s[j])) continue;
                return s[j];
            }
            return '\0';
        }

        private static bool PrevJoins(string s, int i)
        {
            for (int j = i - 1; j >= 0; j--)
            {
                char p = s[j];
                if (IsTashkeel(p)) continue;
                return JoinsNext(p);
            }
            return false;
        }

        private static bool NextJoins(string s, int i)
        {
            for (int j = i + 1; j < s.Length; j++)
            {
                char nx = s[j];
                if (IsTashkeel(nx)) continue;
                return JoinsPrev(nx);
            }
            return false;
        }

        private static readonly Dictionary<char, char> Mirror = new Dictionary<char, char>
        {
            { '(', ')' }, { ')', '(' }, { '[', ']' }, { ']', '[' },
            { '{', '}' }, { '}', '{' }, { '<', '>' }, { '>', '<' },
        };

        private static bool IsLtrChar(char c)
        {
            return (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')
                   || c == '.' && false;
        }

        /// <summary>عكس السلسلة بصرياً مع إبقاء أجزاء اللاتينية والأرقام باتجاهها.</summary>
        private static string ReverseVisual(string s)
        {
            var chars = new List<char>(s.Length);
            int i = 0;
            var tokens = new List<string>();
            while (i < s.Length)
            {
                char c = s[i];
                if (IsLtrChar(c))
                {
                    int j = i;
                    while (j < s.Length && (IsLtrChar(s[j]) || (s[j] == '.' || s[j] == ':' || s[j] == '/') && j + 1 < s.Length && IsLtrChar(s[j + 1])))
                        j++;
                    tokens.Add(s.Substring(i, j - i));
                    i = j;
                }
                else
                {
                    char m;
                    tokens.Add((Mirror.TryGetValue(c, out m) ? m : c).ToString());
                    i++;
                }
            }
            tokens.Reverse();
            var sb = new StringBuilder(s.Length);
            foreach (var t in tokens) sb.Append(t);
            return sb.ToString();
        }
    }
}
