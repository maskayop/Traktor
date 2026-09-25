using TMPro;
using UnityEngine;
using Vopere.Common;
using static Tractor.GameController;

namespace Tractor.UI
{
    public class UIMainCanvas : MonoBehaviour
    {
        public static UIMainCanvas Instance;

        [Header("Windows")]
        [SerializeField] GameObject mainMenuWindow;
        [SerializeField] GameObject HUDWindow;
        [Header("Test Info")]
        public bool showTestInfoWindow = true;
        [SerializeField] UITestInfoWindow testInfoWindow;
        [Header("Version")]
        [SerializeField] TextMeshProUGUI versionText;

        GameController gameController;
        TractorController tractorController;

        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Cannot create UIMainCanvas");
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
            gameController = GameController.Instance;
            tractorController = FindAnyObjectByType<TractorController>();

            if (!tractorController || !gameController)
                return;

            OpenMainMenuWindow();
            ShowTestInfoWindow(showTestInfoWindow);

            if (versionText)
                versionText.text = Application.version;
        }

        public void ExitGame()
        {
            App.Instance?.ExitGame();
        }

        public void OpenMainMenuWindow()
        {
            mainMenuWindow.SetActive(true);
            HUDWindow.SetActive(false);

            gameController.SetGameState(GameState.Menu);
        }

        public void CloseMainMenuWindow()
        {
            mainMenuWindow.SetActive(false);
            HUDWindow.SetActive(true);

            gameController.SetGameState(GameState.Game);
        }

        public void StartGame()
        {
            CloseMainMenuWindow();
        }

        public void ShowTestInfoWindow(bool state)
        {
            showTestInfoWindow = state;
            testInfoWindow.gameObject.SetActive(state);
        }
    }
}
