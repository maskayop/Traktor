using UnityEngine;

namespace Vopere.Common
{
    [RequireComponent(typeof(Camera))]
    public class CameraFPSLimiter : MonoBehaviour
    {
        public enum FPSMode { Static, DynamicFromApp }

        [SerializeField] bool renderOnStart = true;

        [SerializeField] FPSMode fpsMode = FPSMode.Static;

        [Header("FPS Mode")]
        [SerializeField] float staticTargetFPS = 30;

        [Header("Dynamic (App FPS → Mirror FPS)")]
        [SerializeField] AnimationCurve appFPSToMirrorFPS = AnimationCurve.Linear(0f, 0f, 120f, 30f);
        [SerializeField] float minMirrorFPS = 5f;
        [SerializeField] float maxMirrorFPS = 30f;

        [Header("Debug")]
        [SerializeField] float currentTargetFPS;

        Camera targetCamera;
        float timer = 0f;

        void Start()
        {
            targetCamera = GetComponent<Camera>();
            targetCamera.enabled = false;

            if (renderOnStart)
                Render();
        }

        void Update()
        {
            currentTargetFPS = GetTargetFPS();

            if (currentTargetFPS <= 0f) return;

            timer += Time.unscaledDeltaTime;

            if (timer >= 1f / currentTargetFPS)
            {
                Render();
                timer = 0f;
            }
        }

        private void Render()
        {
            targetCamera.Render();
        }

        public void ForceRender()
        {
            Render();
        }

        float GetTargetFPS()
        {
            if (fpsMode == FPSMode.Static)
                return staticTargetFPS;

            if (App.Instance == null)
                return staticTargetFPS;

            float appFPS = App.Instance.CurrentFPS;
            float mirrorFPS = appFPSToMirrorFPS.Evaluate(appFPS);
            return Mathf.Clamp(mirrorFPS, minMirrorFPS, maxMirrorFPS);
        }
    }
}
