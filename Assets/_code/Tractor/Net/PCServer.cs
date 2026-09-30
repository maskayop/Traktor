using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TMPro;
using UnityEngine;

public class PCServer : MonoBehaviour
{
    [Header("UI")]
    public GameObject window;
    public TMP_InputField inputField;
    public TextMeshProUGUI logText;

    TcpListener listener;
    List<TcpClient> clients = new List<TcpClient>();
    string lastMessage = "";

    readonly Queue<System.Action> commandQueue = new Queue<System.Action>();
    readonly object commandLock = new object();

    readonly Queue<string> logQueue = new Queue<string>();
    readonly object logLock = new object();

    class ClientState
    {
        public TcpClient Client;
        public byte[] Buffer = new byte[1024];
    }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    void Awake()
    {
    }
#else
    void Awake()
    {
        DestroyImmediate(window);
        DestroyImmediate(this);
    }
#endif

    void Start()
    {
        listener = new TcpListener(IPAddress.Any, 8052);
        listener.Start();
        Log("Сервер запущен. Ждём подключения...");
        listener.BeginAcceptTcpClient(OnClientConnected, null);
    }

    void Update()
    {
        // Отправка текста из InputField
        if (inputField != null)
        {
            if (inputField.text != lastMessage)
            {
                lastMessage = inputField.text;
                SendToAllClients(lastMessage);
            }
        }

        // Логи из фонового потока
        lock (logLock)
        {
            while (logQueue.Count > 0)
                logText.text = logQueue.Dequeue();
        }

        // Команды из фонового потока
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

    // Отправка команды
    public void SendCommand(string command)
    {
        Log($"Отправка на планшет: {command}");
        SendToAllClients(command);
    }

    void Log(string message)
    {
        Debug.Log(message);
        lock (logLock)
            logQueue.Enqueue(message);
    }

    void OnApplicationQuit()
    {
        listener?.Stop();
        foreach (var c in clients) c.Close();
    }

    void HandleCommand(string command)
    {
        switch (command)
        {
            case "START":
                lock (commandLock) commandQueue.Enqueue(StartTractor);
                break;
            case "STOP":
                lock (commandLock) commandQueue.Enqueue(StopTractor);
                break;
            case "EMERGENCY":
                lock (commandLock) commandQueue.Enqueue(EmergencyStop);
                break;
            default:
                Log($"Неизвестная команда: {command}");
                break;
        }
    }

    // Обёртки для кнопок
    public void OnShowMenuButton() { SendCommand("ShowMenu"); }
    public void OnHideMenuButton() { SendCommand("HideMenu"); }

    // Обёртки для вызова методов
    void StartTractor()
    {
        Log("Трактор: СТАРТ");
        // логика
        SendCommand("TRACTOR_STARTED");
    }

    void StopTractor()
    {
        Log("Трактор: СТОП");
        // логика
        SendCommand("TRACTOR_STOPPED");
    }

    void EmergencyStop()
    {
        Log("Трактор: АВАРИЙНАЯ ОСТАНОВКА");
        // логика
        SendCommand("EMERGENCY_STOPPED");
    }
}
