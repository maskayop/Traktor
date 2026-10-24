using System.Collections.Generic;
using Enviro;
using UnityEngine;

namespace Tractor
{
    public class TractorCameraController : MonoBehaviour
    {
        [SerializeField] Camera menuCamera;
        [SerializeField] EnviroQuality menuQuality;
        [SerializeField] Camera gameCamera;
        [SerializeField] List<Camera> mirrorCameras = new List<Camera>();
        [SerializeField] EnviroQuality mirrorsQuality;

        GameController gameController;
        GameController.GameState gameState;

        void Start()
        {
            Init();
        }

        void Update()
        {
            if (!gameController)
                return;

            if (gameState != gameController.gameState)
            {
                gameState = gameController.gameState;
                SetCameras();
            }
        }

        public void Init()
        {
            gameController = GameController.Instance;

            RegisterEnviroCameras();
            SetCameras();
        }

        void SetCameras()
        {
            if (gameState == GameController.GameState.Menu)
            {
                menuCamera.gameObject.SetActive(true);
                gameCamera.gameObject.SetActive(false);

                foreach (Camera c in mirrorCameras)
                    c.gameObject.SetActive(false);
            }
            else if (gameState == GameController.GameState.Game)
            {
                menuCamera.gameObject.SetActive(false);
                gameCamera.gameObject.SetActive(true);

                foreach (Camera c in mirrorCameras)
                    c.gameObject.SetActive(true);
            }
        }

        void RegisterEnviroCameras()
        {
            if (EnviroManager.instance == null)
                return;

            AddEnviroCamera(menuCamera, menuQuality);
            AddEnviroCamera(gameCamera, null);

            foreach (Camera camera in mirrorCameras)
                AddEnviroCamera(camera, mirrorsQuality);
        }

        void AddEnviroCamera(Camera camera, EnviroQuality quality)
        {
            if (!camera) return;

            EnviroManager.instance.AddAdditionalCamera(camera);

            var cameras = EnviroManager.instance.Cameras;

            for (int i = 0; i < cameras.Count; i++)
            {
                if (cameras[i].camera != camera)
                    continue;
                    
                cameras[i].quality = quality;
                return;
            }
        }
    }
}
