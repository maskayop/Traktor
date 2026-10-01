using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TMPro;
using UnityEngine;

namespace Tractor.Net
{
    public class TabletClient : MonoBehaviour
    {
        public static TabletClient Instance;

        public bool destroyOnWrongPlatform = true;

        [Header("Настройки подключения")]
        public string serverIP = "";
        public int serverPort = 8052;

        [Header("UI")]
        public GameObject window;
        public TMP_InputField IP_inputField;
        public TextMeshProUGUI displayText;
        public TextMeshProUGUI logText;

        RouterTablet router;

        TcpClient client;
        NetworkStream stream;
        byte[] receiveBuffer = new byte[1024];

        readonly Queue<System.Action> actionQueue = new Queue<System.Action>();
        readonly object actionLock = new object();

        readonly Queue<string> logQueue = new Queue<string>();
        readonly object logLock = new object();

        bool connectionLost = false;

#if UNITY_ANDROID
        void Awake()
        {

            if (Instance != null)
            {
                Debug.LogWarning("Cannot create TabletClient");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }
#else
        void Awake()
        {
            if (destroyOnWrongPlatform)
            {
                if (window)
                    DestroyImmediate(window);

                DestroyImmediate(this);
            }
        }
#endif

        void Start()
        {
            router = RouterTablet.Instance;

            string savedIP = PlayerPrefs.GetString("ServerIP", "");

            if (!string.IsNullOrEmpty(savedIP))
            {
                serverIP = savedIP;

                if (IP_inputField)
                    IP_inputField.text = savedIP;

                ConnectToServer();
            }
        }

        void Update()
        {
            if (logText)
            {
                lock (logLock)
                {
                    while (logQueue.Count > 0)
                        logText.text = logQueue.Dequeue();
                }
            }

            lock (actionLock)
            {
                while (actionQueue.Count > 0)
                    actionQueue.Dequeue()?.Invoke();
            }

            if (displayText)
            {
                if (connectionLost)
                {
                    displayText.text = "Соединение потеряно.";
                    connectionLost = false;
                }
            }
        }

        public void OnConnectButton()
        {
            string ip = IP_inputField.text.Trim();
            if (string.IsNullOrEmpty(ip)) return;

            if (!IPAddress.TryParse(ip, out _))
            {
                Log($"Некорректный IP: {ip}");
                if (displayText) displayText.text = "Неверный формат IP";
                return;
            }

            PlayerPrefs.SetString("ServerIP", ip);
            PlayerPrefs.Save();

            serverIP = ip;
            ConnectToServer();
        }

        public void ConnectToServer()
        {
            CleanConnection();
            connectionLost = false;

            try
            {
                client = new TcpClient();
                client.Connect(serverIP, serverPort);
                stream = client.GetStream();
                stream.BeginRead(receiveBuffer, 0, receiveBuffer.Length, OnDataReceived, null);
                Log("Подключено к ПК!");
            }
            catch (System.Exception e)
            {
                Log($"Не удалось подключиться: {e.Message}");

                if (displayText)
                    displayText.text = "Ошибка подключения. Проверьте IP-адрес.";
            }
        }

        public void SendCommand(string command)
        {
            if (client == null || !client.Connected || stream == null)
            {
                Log($"Не отправлено (нет связи): {command}");
                return;
            }

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(command);
                stream.Write(data, 0, data.Length);
                Log($"Отправлено: {command}");
            }
            catch (System.Exception e)
            {
                Log($"Ошибка отправки: {e.Message}");
            }
        }

        public void Log(string message)
        {
            Debug.Log(message);
            lock (logLock)
                logQueue.Enqueue(message);
        }

        void OnDataReceived(System.IAsyncResult ar)
        {
            try
            {
                int bytesRead = stream.EndRead(ar);
                if (bytesRead > 0)
                {
                    string message = Encoding.UTF8.GetString(receiveBuffer, 0, bytesRead);
                    Log($"Получено от ПК: {message}");
                    HandleCommand(message);
                }

                stream.BeginRead(receiveBuffer, 0, receiveBuffer.Length, OnDataReceived, null);
            }
            catch (System.Exception e)
            {
                Log($"Соединение потеряно: {e.Message}");
                connectionLost = true;
            }
        }

        // Просто передаём строку роутеру
        void HandleCommand(string command)
        {
            lock (actionLock)
                actionQueue.Enqueue(() => router?.Route(command));
        }

        void OnApplicationQuit()
        {
            CleanConnection();
        }

        void CleanConnection()
        {
            stream?.Close();
            client?.Close();
            stream = null;
            client = null;
        }
    }
}
