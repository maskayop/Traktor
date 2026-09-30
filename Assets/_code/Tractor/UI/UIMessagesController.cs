using UnityEngine;
using static Tractor.ErrorDetector;

namespace Tractor.UI
{
    public class UIMessagesController : MonoBehaviour
    {
        public static UIMessagesController Instance;

        [SerializeField] Transform messagesContainer;
        [SerializeField] GameObject warningMessagePrefab;
        [SerializeField] GameObject errorMessagePrefab;
        [SerializeField] GameObject penaltyMessagePrefab;

        ErrorDetector errorDetector;
        ErrorMessage errorMessage;

        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Cannot create UIMessagesController");
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
            if (!errorDetector)
                return;

            if (errorMessage != errorDetector.ErrorMessage)
                ShowErrorMessage();

            errorMessage = errorDetector.ErrorMessage;
        }

        public void Init()
        {
            errorDetector = ErrorDetector.Instance;

            if (!errorDetector)
                return;

            errorMessage = errorDetector.ErrorMessage;

            foreach (Transform t in messagesContainer)
                Destroy(t.gameObject);
        }

        void ShowErrorMessage()
        {
            if (errorMessage == null)
                return;

            if (errorMessage.errorState == ErrorState.Warning)
                CreateMessage(warningMessagePrefab);
            else if (errorMessage.errorState == ErrorState.Error)
                CreateMessage(errorMessagePrefab);
            else if (errorMessage.errorState == ErrorState.Penalty)
                CreateMessage(penaltyMessagePrefab);

            errorDetector.ErrorMessage = null;
        }

        void CreateMessage(GameObject prefab)
        {
            GameObject go = Instantiate(prefab, messagesContainer);
            UIMessage m = go.GetComponent<UIMessage>();
            m.Init(errorMessage.message);
        }
    }
}
