using UnityEngine;

namespace Tractor.Net
{
    public class RouterPC : MonoBehaviour
    {
        public static RouterPC Instance;

        public bool destroyOnWrongPlatform = true;
        public PCServer server;

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

        // Вызывается из PCServer в главном потоке (через очередь)
        public void Route(string command)
        {
            switch (command)
            {
                case "START":
                    StartTractor();
                    break;

                case "STOP":
                    StopTractor();
                    break;

                case "EMERGENCY":
                    EmergencyStop();
                    break;

                default:
                    Debug.LogWarning($"[RouterPC] Неизвестная команда: {command}");
                    break;
            }
        }

        public void StartTractor()
        {
            Debug.Log("Трактор: СТАРТ");
            // логика трактора
            server?.SendCommand("TRACTOR_STARTED");
        }

        public void StopTractor()
        {
            Debug.Log("Трактор: СТОП");
            // логика трактора
            server?.SendCommand("TRACTOR_STOPPED");
        }

        public void EmergencyStop()
        {
            Debug.Log("Трактор: АВАРИЙНАЯ ОСТАНОВКА");
            // логика трактора
            server?.SendCommand("EMERGENCY_STOPPED");
        }

        // Обёртки для кнопок
        public void OnShowMenuButton() { server?.SendCommand("ShowMenu"); }
        public void OnHideMenuButton() { server?.SendCommand("HideMenu"); }
    }
}
