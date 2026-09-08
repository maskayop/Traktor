using UnityEngine;

namespace Tractor
{
    public class UIExerciseWindow : MonoBehaviour
    {
        public static UIExerciseWindow Instance;

        [SerializeField] GameObject window;

        bool isOpen = false;
        public bool IsOpen { get { return isOpen; } }

        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Cannot create UIExerciseWindow");
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
            Close();
        }

        public void Open()
        {
            isOpen = true;
            window.SetActive(true);
        }

        public void Close()
        {
            isOpen = false;
            window.SetActive(false);
        }
    }
}
