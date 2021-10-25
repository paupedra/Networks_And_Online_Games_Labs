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
    public Socket socket;
    public string username = "Null";
    public bool active = false;
}

public class TCPServer : MonoBehaviour //TCP server for exercice 2
{
    Socket tcpSocket;

    IPEndPoint ipep;

    //Socket client;
  
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

            try
            {
                //Generate new user (Gather username when received)
                int userIndex;
                for (userIndex = 0; userIndex < users.Length; userIndex++)
                {
                    if (!users[userIndex].active)
                    {
                        users[userIndex].active = true;
                        users[userIndex].socket = client;
                        break;
                    }
                }

                //gather name
                byte[] buffer = new byte[128];
                users[userIndex].socket.Receive(buffer);
                users[userIndex].username = ASCIIEncoding.ASCII.GetString(buffer);

                Debug.Log(string.Concat("New user connected: ", users[userIndex].username));

                // start receive thread
                receiveThread = new Thread(() => Receive(users[userIndex]));
                receiveThread.Start();
            }
            catch
            {
                Debug.Log("Could not connect with new client");
            }
            

        }

    }

    void ConnectNewUser()
    {

    }

    void Receive(User user)
    {

        while(!exit)
        {
            byte[] buffer = new byte[256];
            user.socket.Receive(buffer);

            string msg = (ASCIIEncoding.ASCII.GetString(buffer));
            Debug.Log(string.Concat("Server receive: ", msg));
            SendMessageAllClients(msg);

        }

        Debug.Log("Shutting down server");
    }

    public void SubmitMessage()
    {
        if(inputField.text.Length>0)
        {
            SendMessageAllClients(inputField.text);
            inputField.text = "";
        }
        
    }

    void SendMessageAllClients(string message)
    {
        textManager.Say(message);
        
        for (int i = 0; i < users.Length; i++)
        {
            if (users[i].active)
            {
                try
                {
                    users[i].socket.Send(ASCIIEncoding.ASCII.GetBytes(message));
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
