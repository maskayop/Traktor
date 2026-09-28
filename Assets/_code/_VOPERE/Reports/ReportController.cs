using UnityEngine;

namespace Vopere
{
    public class ReportController : MonoBehaviour
    {
        public static ReportController Instance;

        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Cannot create ReportController");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        void Start()
        {
            Init();
        }

        public void Init() { }
    }
}
