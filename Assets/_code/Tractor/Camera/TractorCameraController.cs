using Enviro;
using System.Collections.Generic;
using UnityEngine;

namespace Tractor
{
    public class TractorCameraController : MonoBehaviour
    {
        [Header("Menu Camera")]
        [SerializeField] Camera menuCamera;
        [SerializeField] EnviroQuality menuQuality;

        [Header("Game Camera")]
        [SerializeField] Camera gameCamera;
        [SerializeField] EnviroQuality gameQuality;
        [SerializeField] List<Transform> mainCameraContainers = new List<Transform>();

        [Header("Mirrors")]
        [SerializeField] List<Camera> mirrorCameras = new List<Camera>();
        List<GameObject> mirrorCamerasObjects = new List<GameObject>();

        [SerializeField] List<int> disableMirrorsIds = new List<int>();
        [SerializeField] EnviroQuality mirrorsQuality;

        GameController gameController;
        GameController.GameState gameState;
        EnviroManager enviroManager;

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
            enviroManager = EnviroManager.instance;

            mirrorCamerasObjects.Clear();

            for (int i = 0; i < mirrorCameras.Count; i++)
                mirrorCamerasObjects.Add(mirrorCameras[i].gameObject);

            RegisterEnviroCameras();
            SetCameras();
        }

        void SetCameras()
        {
            if (gameState == GameController.GameState.Menu)
            {
                menuCamera.gameObject.SetActive(true);
                gameCamera.gameObject.SetActive(false);
                EnableMirrorCameras(false);
            }
            else if (gameState == GameController.GameState.Game)
            {
                menuCamera.gameObject.SetActive(false);
                gameCamera.gameObject.SetActive(true);
                SetGameCameraContainer(0);
            }
        }

        void RegisterEnviroCameras()
        {
            if (!enviroManager)
                return;

            AddEnviroCamera(menuCamera, menuQuality);
            AddEnviroCamera(gameCamera, gameQuality);

            foreach (Camera camera in mirrorCameras)
                AddEnviroCamera(camera, mirrorsQuality);
        }

        void AddEnviroCamera(Camera camera, EnviroQuality quality)
        {
            if (!enviroManager)
                return;

            if (!camera)
                return;

            enviroManager?.AddAdditionalCamera(camera);

            var cameras = enviroManager.Cameras;

            for (int i = 0; i < cameras.Count; i++)
            {
                if (cameras[i].camera != camera)
                    continue;

                cameras[i].quality = quality;
                return;
            }
        }

        void EnableMirrorCameras(bool state)
        {
            foreach (GameObject go in mirrorCamerasObjects)
                go.SetActive(state);
        }

        public void SetGameCameraContainer(int id)
        {
            if (mainCameraContainers.Count == 0)
                return;

            gameCamera.transform.parent = mainCameraContainers[id];
            gameCamera.transform.localPosition = Vector3.zero;
            gameCamera.transform.localRotation = Quaternion.identity;

            EnableMirrorCameras(!disableMirrorsIds.Contains(id));
        }
    }
}
