using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

public class TCPClient_2 : MonoBehaviour //TCP client for exercice 3
{

    Socket tcpSocket1;
    Socket tcpSocket2;
    Socket tcpSocket3;

    EndPoint serverIp;

    bool exit = false;

    Thread connect;
    Thread connect2;
    Thread connect3;

    public TextManager textManager;

    // Start is called before the first frame update
    void Start()
    {
        tcpSocket1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        tcpSocket2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        tcpSocket3 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        serverIp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6969);

        connect = new Thread(() => Connect(tcpSocket1,1));
        connect.Start();

        connect2 = new Thread(() => Connect(tcpSocket2,2));
        connect2.Start();

        connect3 = new Thread(() => Connect(tcpSocket3,3));
        connect3.Start();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void Connect(Socket socket,int client)
    {
        socket.Connect(serverIp); //Block until it connects

        byte[] data = new byte[256];
        int i = 0;
        bool received;

        while (!exit)
        {

            while (i < 5) // Send Ping 5 times
            {
                i++;
                received = false;

                while (!received) //Data recieved must be Pong, if not, keep on sending Ping
                {
                    data = ASCIIEncoding.ASCII.GetBytes("Ping");
                    socket.Send(data, data.Length, SocketFlags.None);

                    byte[] buffer = new byte[256];
                    socket.Receive(buffer); //Awaits and receives message
                    textManager.Say(string.Concat(DateTime.Now.ToString(), " Client ", client.ToString() ," received: ", ASCIIEncoding.ASCII.GetString(buffer)));
                    Debug.Log(string.Concat("Client received: ", ASCIIEncoding.ASCII.GetString(buffer)));

                    if (ASCIIEncoding.ASCII.GetString(buffer).Contains("Pong")) //finish resending if Server returned Pong
                    {
                        received = true;
                        Thread.Sleep(1000);
                    }

                }

            }

            data = ASCIIEncoding.ASCII.GetBytes("Exit");
            socket.Send(data, data.Length, SocketFlags.None);

            exit = true;
        }

        
    }

    void OnDestroy()
    {
        exit = true;
        connect.Abort();
        connect2.Abort();
        connect3.Abort();
    }
}

