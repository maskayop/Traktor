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

    void Start()
    {
        listener = new TcpListener(IPAddress.Any, 8052);
        listener.Start();
        logText.text = "Сервер запущен. Ждём подключения...";
        listener.BeginAcceptTcpClient(OnClientConnected, null);
    }

    void Update()
    {
        if (inputField.text != lastMessage)
        {
            lastMessage = inputField.text;
            SendToAllClients(lastMessage);
        }
    }

    void OnClientConnected(System.IAsyncResult ar)
    {
        TcpClient client = listener.EndAcceptTcpClient(ar);
        clients.Add(client);
        logText.text = $"Планшет подключен! Всего клиентов: {clients.Count}";
        listener.BeginAcceptTcpClient(OnClientConnected, null);
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
                logText.text = $"Ошибка отправки клиенту: {e.Message}";
                client.Close();
                clients.Remove(client);
            }
        }
    }

    void OnApplicationQuit()
    {
        listener?.Stop();
        foreach (var c in clients) c.Close();
    }
}
