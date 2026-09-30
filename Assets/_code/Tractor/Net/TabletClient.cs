using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using TMPro;
using UnityEngine;

public class TabletClient : MonoBehaviour
{

    [Header("Настройки подключения")]
    public string serverIP = "192.168.0.101";
    public int serverPort = 8052;

    [Header("UI")]
    public GameObject window;
    public TextMeshProUGUI displayText;
    public TextMeshProUGUI logText;

    TcpClient client;
    NetworkStream stream;
    byte[] receiveBuffer = new byte[1024];

    // Очередь сообщений между фоновым потоком и главным
    readonly Queue<string> messageQueue = new Queue<string>();
    readonly object queueLock = new object();

    readonly Queue<string> logQueue = new Queue<string>();
    readonly object logLock = new object();

    // Флаг, чтобы OnDataReceived не пытался писать в UI из фонового потока
    public bool connectionLost = false;

#if UNITY_ANDROID
    void Start()
    {
        ConnectToServer();
    }

    void Update()
    {
        // displayText — входящие данные (как было)
        lock (queueLock)
        {
            while (messageQueue.Count > 0)
                displayText.text = messageQueue.Dequeue();
        }

        // logText — свои логи отправки
        lock (logLock)
        {
            while (logQueue.Count > 0)
                logText.text = logQueue.Dequeue();
        }
    }
#endif

    public void ConnectToServer()
    {
#if UNITY_ANDROID
        try
        {
            client = new TcpClient();
            client.Connect(serverIP, serverPort);
            stream = client.GetStream();
            stream.BeginRead(receiveBuffer, 0, receiveBuffer.Length, OnDataReceived, null);
            logText.text = "Подключено к ПК!";
        }
        catch (System.Exception e)
        {
            logText.text = $"Не удалось подключиться: {e.Message}";
            displayText.text = "Ошибка подключения. Проверьте IP-адрес.";
        }
#endif
    }

#if UNITY_ANDROID
    // Обёртки для кнопок — OnClick в инспекторе умеет звать только методы без параметров
    public void OnStartButton() { SendCommand("START"); }
    public void OnStopButton() { SendCommand("STOP"); }
    public void OnEmergencyButton() { SendCommand("EMERGENCY"); }

    // Отправка команды на ПК
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

    void Log(string message)
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
                lock (queueLock)
                {
                    messageQueue.Enqueue(message);
                }
            }

            stream.BeginRead(receiveBuffer, 0, receiveBuffer.Length, OnDataReceived, null);
        }
        catch (System.Exception e)
        {
            logText.text = $"Соединение потеряно: {e.Message}";
            connectionLost = true;
        }
    }

    void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
    }

#else
    void Start()
    {
        Destroy(window);
        Destroy(this);
    }
#endif
}
