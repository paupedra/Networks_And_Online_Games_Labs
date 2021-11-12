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

    public int index;
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

    public TextManager textManager;

    public InputField inputField;

    public int maxUsers = 10;

    ServerUser[] users;
    Socket[] sockets;

    ArrayList receiveList = new ArrayList();

    // Start is called before the first frame update
    void Start()
    {
        sockets = new Socket[maxUsers];
        users = new ServerUser[maxUsers];

        for(int i =0;i<maxUsers;i++) //This feels dumb
        {
            users[i] = new ServerUser();
            users[i].index = i;
        }

        tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"),6969);

        tcpSocket.Bind(ipep);
        tcpSocket.Listen(maxUsers); //Accept 10 connections at the same time

        Thread accept = new Thread(Accept);
        accept.Start();

        // start communication thread
        receiveThread = new Thread(Receive);
        receiveThread.Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Accept()
    {
        while(!exit)
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
        int size = sockets[_user.index].Receive(buffer);

        string bufferText = ASCIIEncoding.ASCII.GetString(buffer);

        User jsonUser = JsonUtility.FromJson<User>(bufferText);

        _user.username = jsonUser.username;
        _user.color = jsonUser.color;
        _user.active = true; //Set user to active to start receiving messages

        Debug.Log(string.Concat("New user connected: ", _user.username));

        string a = string.Concat(_user.username, " just joined the chat!");

        Message serverMessage = new Message();
        serverMessage.server = true;
        serverMessage.message = a;
        SendMessageAllClients(serverMessage); //Notify all users of new connected user
    }

    void Receive() //Constant communication thread with User
    {

        while(!exit)
        {
            int i;
            for (i =0;i<sockets.Length;i++)
            {
                if(users[i].active)
                {
                    receiveList.Add(sockets[i]);
                }
                
            }
            
            if(receiveList.Count>0)
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
                        HandleCommand(msg.message, msg.username);
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

    void HandleCommand(string _message,string username)
    {
        switch(_message)
        {
            case "/disconnect":

                //SendMessageAllClients(string.Concat("User: ", user.username, " Disconnected"));
                //user.active = false;
                break;
        }
    }

    public void SubmitMessage() //Called then text box submits
    {
        if(inputField.text.Length > 0)
        {
            Message serverMessage = new Message();
            serverMessage.color = Color.white;
            serverMessage.message = inputField.text;
            SendMessageAllClients(serverMessage);
            inputField.text = "";
        }
        
    }

    void SendMessageAllClients(Message _message) //Send message to all connected clients
    {
        textManager.Say(_message);

        Debug.Log(string.Concat("Sending: ", _message, " to all clients"));

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
                    Debug.Log(string.Concat("Could not send message to active user ",users[i].username));
                }
               
            }
        }
    }

    void OnDestroy()
    {
        exit = true;
        receiveThread.Abort();
    }
}
