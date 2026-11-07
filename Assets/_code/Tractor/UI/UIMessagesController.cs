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

        public void Init()
        {
            errorDetector = ErrorDetector.Instance;

            if (!errorDetector)
                return;

            foreach (Transform t in messagesContainer)
                Destroy(t.gameObject);
        }

        void OnEnable()
        {
            if (!errorDetector)
                errorDetector = ErrorDetector.Instance;

            if (errorDetector)
                errorDetector.OnError += ShowErrorMessage;
        }

        void OnDisable()
        {
            if (errorDetector)
                errorDetector.OnError -= ShowErrorMessage;
        }

        void ShowErrorMessage(ErrorMessage msg)
        {
            if (msg == null) return;

            if (msg.errorState == ErrorState.Warning)
                CreateMessage(warningMessagePrefab, msg.message);
            else if (msg.errorState == ErrorState.Error)
                CreateMessage(errorMessagePrefab, msg.message);
            else if (msg.errorState == ErrorState.Penalty)
                CreateMessage(penaltyMessagePrefab, msg.message);
        }

        void CreateMessage(GameObject prefab, string message)
        {
            GameObject go = Instantiate(prefab, messagesContainer);
            UIMessage m = go.GetComponent<UIMessage>();
            m.Init(message);
        }
    }
}
