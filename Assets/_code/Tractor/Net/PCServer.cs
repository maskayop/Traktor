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

    // Очередь команд из фонового потока — выполняем в Update
    readonly Queue<System.Action> commandQueue = new Queue<System.Action>();
    readonly object commandLock = new object();

    // Очередь логов, накопленных в фоновом потоке — применяем в Update
    readonly Queue<string> logQueue = new Queue<string>();
    readonly object logLock = new object();

    // Вспомогательный класс: хранит клиента и его буфер для чтения
    class ClientState
    {
        public TcpClient Client;
        public byte[] Buffer = new byte[1024];
    }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    void Start()
    {
        listener = new TcpListener(IPAddress.Any, 8052);
        listener.Start();
        logText.text = "Сервер запущен. Ждём подключения...";
        listener.BeginAcceptTcpClient(OnClientConnected, null);
    }

    void Update()
    {
        // Отправка текста из InputField (как было)
        if (inputField.text != lastMessage)
        {
            lastMessage = inputField.text;
            SendToAllClients(lastMessage);
        }

        // Применяем логи, накопленные в фоновом потоке
        lock (logLock)
        {
            while (logQueue.Count > 0)
                logText.text = logQueue.Dequeue();
        }

        // Выполняем команды, накопленные из OnClientData
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
        logText.text = $"Планшет подключен! Всего клиентов: {clients.Count}";

        // Начинаем слушать ЭТОГО клиента (в фоне)
        ClientState state = new ClientState { Client = client };
        NetworkStream stream = client.GetStream();
        stream.BeginRead(state.Buffer, 0, state.Buffer.Length, OnClientData, state);

        // И принимаем следующего
        listener.BeginAcceptTcpClient(OnClientConnected, null);
    }

    // Срабатывает, когда от планшета приходят байты (в ФОНОВОМ потоке!)
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

                // Продолжаем слушать этого же клиента
                stream.BeginRead(state.Buffer, 0, state.Buffer.Length, OnClientData, state);
            }
            else
            {
                // 0 байт = клиент отключился
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

    // Универсальный лог: можно звать и из главного потока, и из фонового
    void Log(string message)
    {
        lock (logLock)
            logQueue.Enqueue(message);
    }

    void OnApplicationQuit()
    {
        listener?.Stop();
        foreach (var c in clients) c.Close();
    }

    // Разбор команды. Вызывается из фонового потока!
    void HandleCommand(string command)
    {
        // Всё, что трогает Unity API, кладём в очередь и выполним в Update.
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

    void StartTractor()
    {
        Log("Трактор: СТАРТ");
        // логика
    }

    void StopTractor()
    {
        Log("Трактор: СТОП");
        // логика
    }

    void EmergencyStop()
    {
        Log("Трактор: АВАРИЙНАЯ ОСТАНОВКА");
        // логика
    }

#else
    void Start()
    {
        Destroy(window);
        Destroy(this);
    }
#endif
}
