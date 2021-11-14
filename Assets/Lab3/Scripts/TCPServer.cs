using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using UnityEngine.UI;

public class ServerUser
{
    public ServerUser()
    {
        active = false;
        username = "NoName";
    }

    public int uid = 0;
    public int socketIndex;
    public string username = "NoName";
    public Color color;
    public bool active = false;
}

public class TCPServer : MonoBehaviour //TCP server for exercice 2
{
    Socket tcpSocket;

    IPEndPoint ipep;

    bool exit = false;

    Thread receiveThread;
    Thread acceptThread;

    public TextManager textManager;

    public InputField inputField;

    public int maxUsers = 10;

    ServerUser[] users;
    Socket[] sockets;

    ArrayList receiveList = new ArrayList();

    public Text userList;

    // Start is called before the first frame update
    void Start()
    {
        sockets = new Socket[maxUsers];
        users = new ServerUser[maxUsers];

        for (int i = 0; i < maxUsers; i++) //This feels dumb
        {
            users[i] = new ServerUser();
            users[i].socketIndex = i;
        }

        tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4201);

        tcpSocket.Bind(ipep);
        tcpSocket.Listen(maxUsers); //Accept 10 connections at the same time

        acceptThread = new Thread(Accept);
        acceptThread.Start();

        // start communication thread
        receiveThread = new Thread(Receive);
        receiveThread.Start();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateUserList();
    }

    void Accept()
    {
        while (!exit)
        {
            Socket client = tcpSocket.Accept(); //Block until it finds a sending socket
            Debug.Log("Accepted TCP connection");
            try
            {

                //Generate new user (Gather username when received)
                int userIndex = 0;
                for (; userIndex < users.Length; userIndex++)
                {

                    if (!users[userIndex].active)
                    {
                        sockets[userIndex] = client;


                        break;
                    }
                }

                HandleNewUser(ref users[userIndex]);


            }
            catch
            {
                Debug.Log("Could not connect with new client");
            }


        }
    }

    void HandleNewUser(ref ServerUser _user)
    {
        //gather name
        byte[] buffer = new byte[256];
        int size = sockets[_user.socketIndex].Receive(buffer);

        string bufferText = ASCIIEncoding.ASCII.GetString(buffer);

        User jsonUser = JsonUtility.FromJson<User>(bufferText);

        _user.username = jsonUser.username;
        _user.color = jsonUser.color;
        _user.active = true; //Set user to active to start receiving messages
        _user.uid = jsonUser.uid;

        Debug.Log(string.Concat("New user connected: ", _user.username));

        string a = string.Concat(_user.username, " just joined the chat!");

        Message serverMessage = new Message();
        serverMessage.server = true;
        serverMessage.message = a;
        SendMessageAllClients(serverMessage); //Notify all users of new connected user
    }

    void Receive() //Constant communication thread with User
    {

        while (!exit)
        {
            int i;
            for (i = 0; i < sockets.Length; i++)
            {
                if (users[i].active)
                {
                    receiveList.Add(sockets[i]);
                }

            }

            if (receiveList.Count > 0)
            {
                Socket.Select(receiveList, null, null, 1000);

                for (i = 0; i < receiveList.Count; i++)
                {
                    byte[] buffer = new byte[256];
                    ((Socket)receiveList[i]).Receive(buffer);

                    string bufferText = ASCIIEncoding.ASCII.GetString(buffer);
                    Message msg = JsonUtility.FromJson<Message>(bufferText);

                    if (msg.message.StartsWith("/")) //Determine if message is /command or regular message
                    {
                        HandleCommand(msg);
                    }
                    else
                    {
                        Debug.Log(string.Concat("Server receive: ", msg));

                        SendMessageAllClients(msg);
                        Debug.Log(string.Concat("Finished sending message: ", msg, " to all users"));
                    }
                }
            }

            receiveList.Clear();
        }

        Debug.Log("Shutting down server");
    }

    void HandleCommand(Message _message)
    {
        switch (_message.message)
        {
            case "/disconnect":
                Debug.Log(string.Concat("User: ", _message.username, " wants to disconnect"));
                //search for disconnected user
                for (int i = 0; i < maxUsers; i++)
                {
                    if (users[i].uid == _message.uid)
                    {
                        users[i].active = false;

                        Message disconnectMessage = new Message();
                        disconnectMessage.message = "/disconnect";
                        disconnectMessage.server = true;

                        string str = JsonUtility.ToJson(disconnectMessage);

                        sockets[users[i].socketIndex].Send(Encoding.ASCII.GetBytes(str)); //Send disconnection message to user

                        

                        break;
                    }
                }

                Message msg = new Message();
                msg.message = string.Concat("User ", _message.username, " has left the chat!");
                msg.server = true;

                

                //notify users of disconnection

                SendMessageAllClients(msg);

                break;
        }
    }

    public void SubmitMessage() //Called then text box submits
    {

        if (inputField.text.Length > 0)
        {
            Message serverMessage = new Message();
            serverMessage.color = Color.black;
            serverMessage.message = inputField.text;
            SendMessageAllClients(serverMessage);
            inputField.text = "";
        }

    }

    void SendMessageAllClients(Message _message) //Send message to all connected clients
    {
        textManager.Say(_message);

        Debug.Log(string.Concat("Sending: ", _message.message, " to all clients"));

        string jsonMessage = JsonUtility.ToJson(_message);

        for (int i = 0; i < users.Length; i++)
        {
            if (users[i].active)
            {
                try
                {
                    sockets[i].Send(ASCIIEncoding.ASCII.GetBytes(jsonMessage));
                }
                catch
                {
                    Debug.Log(string.Concat("Could not send message to active user ", users[i].username));
                    textManager.Say(string.Concat("Could not send message to active user ", users[i].username));
                }

            }
        }
    }

    void UpdateUserList()
    {
        userList.text = "";
        foreach (var user in users)
        {
            if (user.active)
            {
                userList.text += user.username + "\n";
            }
        }
    }
    void OnDestroy()
    {
        exit = true;
        tcpSocket.Close();
        receiveThread.Abort();
        acceptThread.Abort();
    }
}
