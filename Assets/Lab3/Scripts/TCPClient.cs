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
    Thread disconnectThread;
    Thread receiveThread;
    Thread notifyConnection;

    public TextManager textManager;

    public InputField inputField;

    public Button exitButton;

    public string username = "MyUser 1";

    string message = "";

    // Start is called before the first frame update
    void Start()
    {
        tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        serverIp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6969);

        Thread connect = new Thread(Connect);
        connect.Start();

        exitButton.onClick.AddListener(OnExit);
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

    void NotifyConnection() //Sends user name to server
    {
        tcpSocket.Send(ASCIIEncoding.ASCII.GetBytes(username));
    }

    public void SubmitText() //When input box is triggered send message to server
    {
        if (inputField.text.Length > 0)
        {
            sendThread = new Thread(() => SendThread(inputField.text));
            sendThread.Start();

            inputField.text = "";
        }
    }

    void SendThread(string _message) //Send current message to server
    {
        tcpSocket.Send(ASCIIEncoding.ASCII.GetBytes(_message));
        Debug.Log(string.Concat("Client ", username, " sent: ", _message));
    }

    void ReceiveMessagesThread() //Constant thread tyhat receives all messages froms erver and adds it to its chat box
    {
        while (!exit)
        {
            byte[] buffer = new byte[256];
            tcpSocket.Receive(buffer); //Awaits and receives message
            textManager.Say(string.Concat(DateTime.Now.ToString()," ", ASCIIEncoding.ASCII.GetString(buffer)));

        }
    }

    public void OnExit() //Notify Server of client's disconnection
    {
        disconnectThread = new Thread(() => SendThread("/disconnect"));
        disconnectThread.Start();
        exit = true;
    }



    void OnDestroy()
    {
        exit = true;
        sendThread.Abort();
        receiveThread.Abort();
    }
}
