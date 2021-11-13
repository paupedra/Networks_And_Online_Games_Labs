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
    public int uid;
}

public class TCPClient : MonoBehaviour //TCP client for exercice 2
{

    Socket tcpSocket;

    EndPoint serverIp;

    bool exit = false;

    Thread receiveThread;
    Thread notifyConnection;
    Thread disconnectThread;

    public TextManager textManager;

    public InputField inputField;
    public InputField usernameInput;

    public Button exitButton;

    public Text clientText;

    bool logged = false;

    User user = new User();

    Message message;

    // Start is called before the first frame update
    void Start()
    {
        tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        serverIp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4201);

        exitButton.onClick.AddListener(OnExit);

        user.username = "DefaultName";
        user.color.r = UnityEngine.Random.Range(0f, 1f);
        user.color.g = UnityEngine.Random.Range(0f, 1f);
        user.color.b = UnityEngine.Random.Range(0f, 1f);
        user.color.a = 1;

        user.uid = UnityEngine.Random.Range(0, 999999);

        clientText.color = user.color;

        receiveThread = new Thread(ReceiveMessagesThread);
        notifyConnection = new Thread(NotifyConnection);
        disconnectThread = new Thread(NotifyDisconnection);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SubmitConnect() //When user submits its name connect to server
    {
        user.username = usernameInput.text;
        logged = true;

        Thread connect = new Thread(Connect);
        connect.Start();
    }

    void Connect()
    {
        tcpSocket.Connect(serverIp);

        notifyConnection.Start();

        receiveThread.Start();
    }

    void NotifyConnection() //Sends user name to server
    {
        string serializedUser = JsonUtility.ToJson(user);
        tcpSocket.Send(Encoding.ASCII.GetBytes(serializedUser));
    }

    void NotifyDisconnection()
    {
        try
        {
            message = new Message();
            message.message = "/disconnect";
            message.color = user.color;
            message.username = user.username;
            message.uid = user.uid;

            string jsonMessage = JsonUtility.ToJson(message);

            tcpSocket.Send(ASCIIEncoding.ASCII.GetBytes(jsonMessage));
            Debug.Log(string.Concat("Client ", user.username, " sent: ", message));

            logged = false;
        }
        catch
        {

        }
    }

    public void SubmitText() //When input box is triggered send message to server
    {
        if (logged)
        {
            if (inputField.text.Length > 0)
            {
                if(inputField.text.StartsWith("/"))
                {
                    switch (inputField.text)
                    {
                        case "/disconnect":
                            NotifyDisconnection();
                            return;
                    }

                    textManager.Say("This command could not be recognised");

                }

                message = new Message();
                message.message = inputField.text;
                message.color = user.color;
                message.username = user.username;
                message.uid = user.uid;

                string jsonMessage = JsonUtility.ToJson(message);

                tcpSocket.Send(ASCIIEncoding.ASCII.GetBytes(jsonMessage));
                Debug.Log(string.Concat("Client ", user.username, " sent: ", message));

                inputField.text = "";
            }
        }
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
        if(logged)
        {
            //disconnectThread = new Thread(() => SendThread("/disconnect"));
            //disconnectThread.Start();
            exit = true;
        }
    }

    void OnDestroy()
    {
        
        NotifyDisconnection();

        exit = true;
        notifyConnection.Abort();
        receiveThread.Abort();
    }
}
