using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using UnityEngine.UI;

class User
{
    public User()
    {
        active = false;
        username = "NoName";
    }
    public Socket socket;
    public string username = "NoName";
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

    User[] users;

    // Start is called before the first frame update
    void Start()
    {
        users = new User[maxUsers];

        for(int i =0;i<maxUsers;i++) //This feels dumb
        {
            users[i] = new User();
        }

        tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"),6969);

        tcpSocket.Bind(ipep);
        tcpSocket.Listen(maxUsers); //Accept 10 connections at the same time

        Thread accept = new Thread(Accept);
        accept.Start();
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
                        users[userIndex].socket = client;
                        users[userIndex].active = true;

                        break;
                    }
                }

                //gather name
                byte[] buffer = new byte[128];
                int size = users[userIndex].socket.Receive(buffer);
                
                string bufferText = ASCIIEncoding.ASCII.GetString(buffer);

                users[userIndex].username = bufferText.Substring(0,size);

                Debug.Log(string.Concat("New user connected: ", users[userIndex].username));

                string a = string.Concat(users[userIndex].username, " just joined the chat!");
                Debug.Log(a);
                SendMessageAllClients(a); //Notify all users of new connected user

                // start communication thread
                receiveThread = new Thread(() => Receive(ref users[userIndex]));
                receiveThread.Start();
            }
            catch
            {
                Debug.Log("Could not connect with new client");
            }
            

        }
    }

    void Receive(ref User user) //Constant communication thread with User
    {

        while(!exit && user.active)
        {
            byte[] buffer = new byte[256];
            int size = user.socket.Receive(buffer);

            string bufferText = ASCIIEncoding.ASCII.GetString(buffer);
            string msg = bufferText.Substring(0, size);

            if (msg.StartsWith("/")) //Determine if message is /command or regular message
            {
                HandleCommand(msg,ref user);
            }
            else
            {
                Debug.Log(string.Concat("Server receive: ", msg));
                SendMessageAllClients(msg);
            }


        }

        Debug.Log("Shutting down server");
    }

    void HandleCommand(string _message,ref User user)
    {
        switch(_message)
        {
            case "/disconnect":

                SendMessageAllClients(string.Concat("User: ", user.username, " Disconnected"));
                user.active = false;
                break;
        }
    }

    public void SubmitMessage() //Called then text box submits
    {
        if(inputField.text.Length > 0)
        {
            SendMessageAllClients(inputField.text);
            inputField.text = "";
        }
        
    }

    void SendMessageAllClients(string _message) //Send message to all connected clients
    {
        textManager.Say(_message);

        Debug.Log(string.Concat("Sending: ", _message, " to all clients"));
        
        for (int i = 0; i < users.Length; i++)
        {
            if (users[i].active)
            {
                try
                {
                    users[i].socket.Send(ASCIIEncoding.ASCII.GetBytes(_message));
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
