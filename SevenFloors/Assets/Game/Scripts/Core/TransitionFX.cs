using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SevenFloors.Core
{
    /// <summary>
    /// انتقالات بصرية: تعتيم/إظهار فوق كل شيء، وهزة شاشة.
    /// </summary>
    public class TransitionFX : MonoBehaviour
    {
        public static TransitionFX I { get; private set; }

        private Image _black;
        private CanvasGroup _group;
        public bool Busy { get; private set; }

        public static TransitionFX Create()
        {
            var canvas = UIFactory.CreateCanvas("FXCanvas", 1000);
            DontDestroyOnLoad(canvas.gameObject);
            var fx = canvas.gameObject.AddComponent<TransitionFX>();
            I = fx;
            fx._black = UIFactory.CreatePanel(canvas.transform, "Black", Color.black);
            UIFactory.Fill(fx._black.rectTransform);
            fx._group = fx._black.gameObject.AddComponent<CanvasGroup>();
            fx._group.alpha = 1f; // نبدأ من سواد ثم نكشف
            fx._group.blocksRaycasts = false;
            return fx;
        }

        /// <summary>تعتيم ← تنفيذ ← كشف.</summary>
        public void FadeThrough(Action midAction, float outTime = 0.45f, float inTime = 0.55f)
        {
            StartCoroutine(FadeRoutine(midAction, outTime, inTime));
        }

        public void FadeIn(float time = 0.8f) { StartCoroutine(FadeTo(0f, time)); }
        public void FadeOut(float time = 0.6f) { StartCoroutine(FadeTo(1f, time)); }

        private IEnumerator FadeRoutine(Action mid, float outT, float inT)
        {
            Busy = true;
            _group.blocksRaycasts = true;
            yield return FadeTo(1f, outT);
            if (mid != null) mid();
            yield return null;
            yield return FadeTo(0f, inT);
            _group.blocksRaycasts = false;
            Busy = false;
        }

        private IEnumerator FadeTo(float target, float time)
        {
            float start = _group.alpha;
            float t = 0;
            while (t < time)
            {
                t += Time.unscaledDeltaTime;
                _group.alpha = Mathf.Lerp(start, target, Mathf.Clamp01(t / time));
                yield return null;
            }
            _group.alpha = target;
        }

        /// <summary>هزة لعنصر واجهة (مثل نافذة الحوار أو الخلفية).</summary>
        public void Shake(RectTransform target, float strength = 18f, float duration = 0.45f)
        {
            if (target != null) StartCoroutine(ShakeRoutine(target, strength, duration));
        }

        private IEnumerator ShakeRoutine(RectTransform rt, float strength, float duration)
        {
            Vector2 orig = rt.anchoredPosition;
            float t = 0;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float damper = 1f - Mathf.Clamp01(t / duration);
                rt.anchoredPosition = orig + UnityEngine.Random.insideUnitCircle * strength * damper;
                yield return null;
            }
            rt.anchoredPosition = orig;
        }
    }
}
