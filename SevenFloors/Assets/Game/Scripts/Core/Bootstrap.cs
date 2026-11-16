using UnityEngine;
using UnityEngine.EventSystems;

namespace SevenFloors.Core
{
    /// <summary>
    /// نقطة انطلاق اللعبة — هذا هو المكوّن الوحيد الموجود في المشهد؛
    /// كل شيء آخر (كاميرا، واجهات، مدراء) يُبنى برمجياً عند التشغيل.
    /// </summary>
    public class Bootstrap : MonoBehaviour
    {
        private void Awake()
        {
            Application.targetFrameRate = 60;

            // كاميرا
            if (Camera.main == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                var cam = camGo.AddComponent<Camera>();
                cam.orthographic = true;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.black;
                DontDestroyOnLoad(camGo);
            }

            // نظام الإدخال للواجهات
            if (UnityEngine.Object.FindObjectOfType<EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                DontDestroyOnLoad(esGo);
            }

            var gm = GameManager.Create();
            gm.Boot();
        }
    }
}
