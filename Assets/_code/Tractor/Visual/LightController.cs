using UnityEngine;

namespace Tractor
{
    public class LightController : MonoBehaviour
    {
        public static LightController Instance;

        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Cannot create LightController");
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
        }

        void Update()
        {
        }
    }
}
