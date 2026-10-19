using UnityEngine;

namespace Tractor.Net
{
    public class RouterPC : Router_Base
    {
        public static RouterPC Instance;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Cannot create RouterPC");
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
