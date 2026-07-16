using UnityEngine;

namespace Vopere.Common
{
    [RequireComponent(typeof(Camera))]
    public class CameraFPSLimiter : MonoBehaviour
    {
        [SerializeField] int targetFPS = 30;
        [SerializeField] bool renderOnStart = true;

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
            timer += Time.unscaledDeltaTime;

            if (timer >= 1f / targetFPS)
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
    }
}
