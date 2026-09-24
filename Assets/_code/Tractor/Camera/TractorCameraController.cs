using System.Collections.Generic;
using UnityEngine;

namespace Tractor
{
    public class TractorCameraController : MonoBehaviour
    {
        [SerializeField] Camera menuCamera;
        [SerializeField] Camera gameCamera;
        [SerializeField] List<Camera> mirrorCameras = new List<Camera>();

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
    }
}
