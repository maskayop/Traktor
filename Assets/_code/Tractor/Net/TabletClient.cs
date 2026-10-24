using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
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
        StringBuilder pending = new StringBuilder();

        bool isConnecting = false;

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
            if (isConnecting) return;
            isConnecting = true;

            CleanConnection();
            connectionLost = false;

            string ip = serverIP;
            int port = serverPort;

            Task.Run(() =>
            {
                TcpClient newClient = null;

                try
                {
                    newClient = new TcpClient();
                    var connectTask = newClient.ConnectAsync(ip, port);

                    if (!connectTask.Wait(3000))
                    {
                        newClient.Close();
                        EnqueueLog("Не удалось подключиться: таймаут");
                        EnqueueDisplayText("Ошибка подключения (таймаут)");
                        isConnecting = false;
                        return;
                    }

                    // Подключились — ставим клиент в главный поток
                    lock (actionLock)
                    {
                        actionQueue.Enqueue(() =>
                        {
                            CleanConnection();
                            client = newClient;
                            stream = client.GetStream();
                            stream.BeginRead(receiveBuffer, 0, receiveBuffer.Length, OnDataReceived, null);
                            Log("Подключено к ПК!");
                            isConnecting = false;
                        });
                    }
                }
                catch (System.Exception e)
                {
                    try { newClient?.Close(); } catch { }

                    EnqueueLog($"Не удалось подключиться: {e.Message}");
                    EnqueueDisplayText("Ошибка подключения. Проверьте IP-адрес.");
                    isConnecting = false;
                }
            });
        }

        void EnqueueLog(string message)
        {
            lock (logLock)
                logQueue.Enqueue(message);
        }

        void EnqueueDisplayText(string text)
        {
            lock (actionLock)
                actionQueue.Enqueue(() =>
                {
                    if (displayText) displayText.text = text;
                });
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
                byte[] data = Encoding.UTF8.GetBytes(command + "\n");
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
            NetworkStream s = stream;
            if (s == null) return;

            try
            {
                int bytesRead = s.EndRead(ar);
                if (bytesRead > 0)
                {
                    pending.Append(Encoding.UTF8.GetString(receiveBuffer, 0, bytesRead));

                    string all = pending.ToString();
                    int newlineIndex;

                    while ((newlineIndex = all.IndexOf('\n')) >= 0)
                    {
                        string message = all.Substring(0, newlineIndex);
                        all = all.Substring(newlineIndex + 1);
                        HandleCommand(message);
                    }

                    pending.Clear();
                    pending.Append(all);
                }

                s.BeginRead(receiveBuffer, 0, receiveBuffer.Length, OnDataReceived, null);
            }
            catch (System.Exception e)
            {
                Log($"Соединение потеряно: {e.Message}");
                connectionLost = true;
            }
        }

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
            pending.Clear();
        }
    }
}
