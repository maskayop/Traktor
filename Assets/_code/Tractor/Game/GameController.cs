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
    }
}
