using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;

public class User
{
    public User()
    {
        username = "NoName";
    }

    public string username = "NoName";
    public Color color;
}

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

    public Text clientText;

    User user = new User();

    // Start is called before the first frame update
    void Start()
    {
        tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        serverIp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6969);

        exitButton.onClick.AddListener(OnExit);

        user.username = "DefaultName";
        user.color.r = UnityEngine.Random.Range(0f, 1f);
        user.color.g = UnityEngine.Random.Range(0f, 1f);
        user.color.b = UnityEngine.Random.Range(0f, 1f);
        user.color.a = 1;

        clientText.color = user.color;
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
        string serializedUser = JsonUtility.ToJson(user);
        tcpSocket.Send(Encoding.ASCII.GetBytes(serializedUser));
    }

    public void SubmitText() //When input box is triggered send message to server
    {
        if (inputField.text.Length > 0)
        {
            Debug.Log(inputField.text);

            Message tmp = new Message();
            tmp.message = inputField.text;
            tmp.color = user.color;
            tmp.username = user.username;

            sendThread = new Thread(() => SendThread(tmp));
            sendThread.Start();

            inputField.text = "";
        }
    }

    void SendThread(Message _message) //Send current message to server
    {

        string jsonMessage = JsonUtility.ToJson(_message);

        tcpSocket.Send(ASCIIEncoding.ASCII.GetBytes(jsonMessage));
        Debug.Log(string.Concat("Client ", user.username, " sent: ", _message));
    }

    void ReceiveMessagesThread() //Constant thread tyhat receives all messages froms erver and adds it to its chat box
    {
        while (!exit)
        {
            byte[] buffer = new byte[256];
            tcpSocket.Receive(buffer); //Awaits and receives message

            //deserialize from json
            Message msg = JsonUtility.FromJson<Message>(Encoding.ASCII.GetString(buffer));

            textManager.Say(msg);
        }
    }

    public void OnExit() //Notify Server of client's disconnection
    {
        //disconnectThread = new Thread(() => SendThread("/disconnect"));
        //disconnectThread.Start();
        exit = true;
    }

    void OnDestroy()
    {
        exit = true;
        sendThread.Abort();
        receiveThread.Abort();
        
    }
}
