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
    public GameObject additionalMenu;
    public TextMeshProUGUI displayText;
    public TextMeshProUGUI logText;

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
        ConnectToServer();
    }

    void Update()
    {
        // logText — свои логи отправки
        lock (logLock)
        {
            while (logQueue.Count > 0)
                logText.text = logQueue.Dequeue();
        }

        // Выполняем действия из очереди (главный поток)
        lock (actionLock)
        {
            while (actionQueue.Count > 0)
                actionQueue.Dequeue()?.Invoke();
        }

        if (connectionLost)
        {
            displayText.text = "Соединение потеряно.";
            connectionLost = false;
        }
    }

    public void ConnectToServer()
    {
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
            displayText.text = "Ошибка подключения. Проверьте IP-адрес.";
        }
    }

    // Отправка команды
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

    void HandleCommand(string command)
    {
        // Всё, что трогает Unity API, кладём в очередь и выполним в Update.
        switch (command)
        {
            case "TRACTOR_STARTED":
                lock (actionLock) actionQueue.Enqueue(() => ShowMessage("Трактор запущен"));
                break;
            case "TRACTOR_STOPPED":
                lock (actionLock) actionQueue.Enqueue(() => ShowMessage("Трактор остановлен"));
                break;
            case "EMERGENCY_STOPPED":
                lock (actionLock) actionQueue.Enqueue(() => ShowMessage("АВАРИЙНАЯ ОСТАНОВКА!"));
                break;
            case "ShowMenu":
                lock (actionLock) actionQueue.Enqueue(OnShowMenu);
                break;
            case "HideMenu":
                lock (actionLock) actionQueue.Enqueue(OnHideMenu);
                break;
            default:
                // Всё остальное — просто текст из InputField на ПК
                lock (actionLock) actionQueue.Enqueue(() => ShowMessage(command));
                break;
        }
    }

    void ShowMessage(string text)
    {
        displayText.text = text;
    }

    void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
    }

    // Обёртки для кнопок
    public void OnStartButton() { SendCommand("START"); }
    public void OnStopButton() { SendCommand("STOP"); }
    public void OnEmergencyButton() { SendCommand("EMERGENCY"); }

    // Обёртки для вызова методов
    void OnShowMenu()
    {
        Log("Показать меню");
        additionalMenu.SetActive(true);
    }

    void OnHideMenu()
    {
        Log("Скрыть меню");
        additionalMenu.SetActive(false);
    }
}
