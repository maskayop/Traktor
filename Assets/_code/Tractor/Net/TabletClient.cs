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

    // Флаг, чтобы OnDataReceived не пытался писать в UI из фонового потока
    bool connectionLost = false;

#if UNITY_ANDROID
    void Start()
    {
        ConnectToServer();
    }

    void ConnectToServer()
    {
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

    void Update()
    {
        // Применяем накопленные сообщения в главном потоке
        lock (queueLock)
        {
            while (messageQueue.Count > 0)
            {
                displayText.text = messageQueue.Dequeue();
            }
        }

        if (connectionLost)
        {
            displayText.text = "Соединение потеряно.";
            connectionLost = false;
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
