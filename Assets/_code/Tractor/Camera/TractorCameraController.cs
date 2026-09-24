using UnityEngine;

namespace Tractor
{
    public class TractorCameraController : MonoBehaviour
    {
        [SerializeField] Camera menuCamera;
        [SerializeField] Camera gameCamera;

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
                ChangeCameras();
            }
        }

        public void Init()
        {
            gameController = GameController.Instance;
        }

        void ChangeCameras()
        {
            if (gameState == GameController.GameState.Menu)
            {
                menuCamera.gameObject.SetActive(true);
                gameCamera.gameObject.SetActive(false);
            }
            else if (gameState == GameController.GameState.Game)
            {
                menuCamera.gameObject.SetActive(false);
                gameCamera.gameObject.SetActive(true);
            }
        }
    }
}
