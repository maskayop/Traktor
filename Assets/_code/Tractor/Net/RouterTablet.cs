using UnityEngine;

namespace Tractor.Net
{
    public class RouterTablet : Router_Base
    {
        public static RouterTablet Instance;

#if UNITY_ANDROID
        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Cannot create RouterTablet");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }
#else
        void Awake()
        {
            if (destroyOnWrongPlatform)
                DestroyImmediate(gameObject);
        }
#endif
    }
}
