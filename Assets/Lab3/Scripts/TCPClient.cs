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
    public InputField usernameInput;

    public Button exitButton;

    User user;

    // Start is called before the first frame update
    void Start()
    {
        tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        serverIp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6969);

        exitButton.onClick.AddListener(OnExit);

        user.username = "DefaultName";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SubmitConnect() //When user submits its name connect to server
    {
        user.username = usernameInput.text;

        Thread connect = new Thread(Connect);
        connect.Start();
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
        tcpSocket.Send(ASCIIEncoding.ASCII.GetBytes(user.username));
    }

    public void SubmitText() //When input box is triggered send message to server
    {
        if (inputField.text.Length > 0)
        {
            Debug.Log(inputField.text);
            string buffer = inputField.text;
            sendThread = new Thread(() => SendThread(buffer));
            sendThread.Start();

            inputField.text = "";
        }
    }

    void SendThread(string _message) //Send current message to server
    {
        tcpSocket.Send(ASCIIEncoding.ASCII.GetBytes(_message));
        Debug.Log(string.Concat("Client ", user.username, " sent: ", _message));
    }

    void ReceiveMessagesThread() //Constant thread tyhat receives all messages froms erver and adds it to its chat box
    {
        while (!exit)
        {
            byte[] buffer = new byte[256];
            tcpSocket.Receive(buffer); //Awaits and receives message
            string msg = string.Concat(DateTime.Now.ToString(), " ", ASCIIEncoding.ASCII.GetString(buffer));
            textManager.Say(msg);
            Debug.Log(msg);

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
