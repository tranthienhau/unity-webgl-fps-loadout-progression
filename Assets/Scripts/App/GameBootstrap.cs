using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Breachpoint.UI;

namespace Breachpoint.App
{
    // Runtime entry point. Builds the camera, canvas and the whole UI in code so the
    // scene needs no manual wiring (and the headless capture harness reuses Construct()).
    public class GameBootstrap : MonoBehaviour
    {
        public static readonly Vector2 Reference = new(1600, 900);

        void Start()
        {
            Construct(out _, out _);
        }

        // Builds camera + canvas + AppController. Returns them so the editor capture path
        // can render to a RenderTexture and drive navigation.
        public static AppController Construct(out Camera cam, out RectTransform canvasRect)
        {
            // Camera
            var camGo = new GameObject("UICamera");
            cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Theme.Surface;
            cam.orthographic = true;
            cam.cullingMask = ~0;
            cam.transform.position = new Vector3(0, 0, -100);

            // Canvas
            var canvasGo = new GameObject("UICanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 100;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = Reference;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasRect = (RectTransform)canvasGo.transform;

            // EventSystem (only needed for live interaction)
            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                if (Application.isPlaying) Object.DontDestroyOnLoad(es);
            }

            var app = canvasGo.AddComponent<AppController>();
            app.Build(canvasRect);
            return app;
        }
    }
}
