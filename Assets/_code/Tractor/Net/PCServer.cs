using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TMPro;
using UnityEngine;

namespace Tractor.Net
{
    public class PCServer : MonoBehaviour
    {
        public static PCServer Instance;

        public bool destroyOnWrongPlatform = true;

        [Header("UI")]
        public GameObject window;
        public TMP_InputField inputField;
        public TextMeshProUGUI logText;
        public TextMeshProUGUI IPText;

        RouterPC router;

        TcpListener listener;
        List<TcpClient> clients = new List<TcpClient>();
        string lastMessage = "";

        readonly Queue<System.Action> commandQueue = new Queue<System.Action>();
        readonly object commandLock = new object();

        readonly Queue<string> logQueue = new Queue<string>();
        readonly object logLock = new object();

        List<string> allLocalIPs = new List<string>();
        string IP = "";

        class ClientState
        {
            public TcpClient Client;
            public byte[] Buffer = new byte[1024];
        }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Cannot create PCServer");
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
            router = RouterPC.Instance;

            if (Vopere.Common.App.Instance)
            {
                allLocalIPs = Vopere.Common.App.Instance.GetAllLocalIPv4();

                IP = "";

                for (int i = 0; i < allLocalIPs.Count; i++)
                    IP += allLocalIPs[i] + "\n";
            }

            if (!string.IsNullOrEmpty(IP))
                if (IPText)
                    IPText.text = IP;

            listener = new TcpListener(IPAddress.Any, 8052);
            listener.Start();
            Log("Сервер запущен. Ждём подключения...");
            listener.BeginAcceptTcpClient(OnClientConnected, null);
        }

        void Update()
        {
            if (inputField != null && inputField.text != lastMessage)
            {
                lastMessage = inputField.text;
                SendToAllClients(lastMessage);
            }

            if (logText)
            {
                lock (logLock)
                {
                    while (logQueue.Count > 0)
                        logText.text = logQueue.Dequeue();
                }
            }

            lock (commandLock)
            {
                while (commandQueue.Count > 0)
                    commandQueue.Dequeue()?.Invoke();
            }
        }

        void OnClientConnected(System.IAsyncResult ar)
        {
            TcpClient client = listener.EndAcceptTcpClient(ar);
            clients.Add(client);
            Log($"Планшет подключен! Всего клиентов: {clients.Count}");

            ClientState state = new ClientState { Client = client };
            NetworkStream stream = client.GetStream();
            stream.BeginRead(state.Buffer, 0, state.Buffer.Length, OnClientData, state);

            listener.BeginAcceptTcpClient(OnClientConnected, null);
        }

        void OnClientData(System.IAsyncResult ar)
        {
            ClientState state = (ClientState)ar.AsyncState;
            TcpClient client = state.Client;

            try
            {
                NetworkStream stream = client.GetStream();
                int bytesRead = stream.EndRead(ar);

                if (bytesRead > 0)
                {
                    string message = Encoding.UTF8.GetString(state.Buffer, 0, bytesRead);
                    Log($"Получено от планшета: {message}");
                    HandleCommand(message);

                    stream.BeginRead(state.Buffer, 0, state.Buffer.Length, OnClientData, state);
                }
                else
                {
                    Log("Планшет отключился.");
                    client.Close();
                    clients.Remove(client);
                }
            }
            catch (System.Exception e)
            {
                Log($"Ошибка чтения: {e.Message}");
                client.Close();
                clients.Remove(client);
            }
        }

        void SendToAllClients(string message)
        {
            if (clients.Count == 0) return;

            byte[] data = Encoding.UTF8.GetBytes(message);

            foreach (var client in clients.ToArray())
            {
                try
                {
                    if (client.Connected)
                    {
                        NetworkStream stream = client.GetStream();
                        stream.Write(data, 0, data.Length);
                    }
                }
                catch (System.Exception e)
                {
                    Log($"Ошибка отправки клиенту: {e.Message}");
                    client.Close();
                    clients.Remove(client);
                }
            }
        }

        public void SendCommand(string command)
        {
            Log($"Отправка на планшет: {command}");
            SendToAllClients(command);
        }

        // Передаём строку роутеру — он сам знает, что делать
        void HandleCommand(string command)
        {
            lock (commandLock)
                commandQueue.Enqueue(() => router?.Route(command));
        }

        public void Log(string message)
        {
            Debug.Log(message);
            lock (logLock)
                logQueue.Enqueue(message);
        }

        void OnApplicationQuit()
        {
            CleanConnection();
        }

        void CleanConnection()
        {
            listener?.Stop();
            listener = null;
            foreach (var c in clients) c.Close();
            clients.Clear();
        }
    }
}
