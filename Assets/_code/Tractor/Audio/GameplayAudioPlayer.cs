using UnityEngine;
using Vopere;

namespace Tractor
{
    public class GameplayAudioPlayer : MonoBehaviour
    {
        public static GameplayAudioPlayer Instance;

        AudioController audioController;

        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Cannot create GameplayAudioPlayer");
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
            audioController = AudioController.Instance;
        }
    }
}
