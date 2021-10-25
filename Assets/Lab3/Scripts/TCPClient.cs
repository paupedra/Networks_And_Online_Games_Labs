using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using UnityEngine.UI;

public class TCPClient : MonoBehaviour //TCP client for exercice 2
{

    Socket tcpSocket;

    EndPoint serverIp;

    bool exit = false;

    Thread sendThread;
    Thread receiveThread;
    Thread notifyConnection;

    public TextManager textManager;

    public InputField inputField;

    public string username = "MyUser 1";

    string message = "";

    // Start is called before the first frame update
    void Start()
    {
        tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        serverIp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6969);

        Thread connect = new Thread(Connect);
        connect.Start();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Connect()
    {

        tcpSocket.Connect(serverIp);

        notifyConnection = new Thread(NotifyConnection);
        notifyConnection.Start();

        receiveThread = new Thread(ReceiveMessagesThread);
        receiveThread.Start();
    }

    void NotifyConnection() //Sends user name
    {
        tcpSocket.Send(ASCIIEncoding.ASCII.GetBytes(string.Concat("User ", username)));
    }

    public void SubmitText() //When input box is triggered send message to server
    {
        if (inputField.text.Length > 0)
        {
            message = inputField.text;
            sendThread = new Thread(SendTrhead);
            sendThread.Start();

            inputField.text = "";
        }
    }

    void SendTrhead() //Send message to server
    {
        tcpSocket.Send(ASCIIEncoding.ASCII.GetBytes(message));
        Debug.Log(string.Concat("Client ", username, " sent: ", message));
    }

    void ReceiveMessagesThread()
    {
        while (!exit)
        {
            byte[] buffer = new byte[256];
            tcpSocket.Receive(buffer); //Awaits and receives message
            textManager.Say(string.Concat(DateTime.Now.ToString(), " Client received: ", ASCIIEncoding.ASCII.GetString(buffer)));
            Debug.Log(string.Concat("Client received: ", ASCIIEncoding.ASCII.GetString(buffer)));
        }
    }

    void OnDestroy()
    {
        exit = true;
        sendThread.Abort();
        receiveThread.Abort();
    }
}
