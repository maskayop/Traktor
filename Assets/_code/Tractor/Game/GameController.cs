using UnityEngine;

namespace Tractor
{
    public class GameController : MonoBehaviour
    {
        public static GameController Instance;

        public enum GameState { Menu, Game }
        public GameState gameState = GameState.Menu;

        [SerializeField] Transform mainSpawnPoint;

        TractorController tractorController;

        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Cannot create GameController");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        void Start()
        {
            Init();
        }

        void Update()
        {

        }

        public void Init()
        {
            SetGameState(GameState.Menu);
        }

        public void SetTractorController(TractorController tc)
        {
            if (!tc)
                return;

            tractorController = tc;
            SetGameState(GameState.Menu);
        }

        public void SetGameState(GameState gs)
        {
            gameState = gs;

            if (gs == GameState.Menu)
                if (tractorController)
                    tractorController.PlaceTractor(mainSpawnPoint);
        }

        public void DetachTrailer()
        {
            tractorController?.DetachTrailer();
        }

        public bool HasTrailer()
        {
            if (!tractorController)
                return false;

            return tractorController.HasTrailer();
        }

        public void EndTrailerTask()
        {
            if (!tractorController)
                return;

            var trailer = tractorController.tractorTrailer.CurrentTrailer;

            // Если CurrentTrailer null, пробуем найти через RCCP-менеджер
            if (trailer == null && tractorController.CarController != null &&
                tractorController.CarController.OtherAddonsManager != null &&
                tractorController.CarController.OtherAddonsManager.TrailAttacher != null)
            {
                trailer = tractorController.CarController.OtherAddonsManager.TrailAttacher.attachedTrailer;
            }

            // Если прицеп найден — отцепляем и отключаем
            if (trailer != null)
            {
                tractorController.ForceDetachTrailer();
                trailer.gameObject.SetActive(false);
            }
        }
    }
}
