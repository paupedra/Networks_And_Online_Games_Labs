using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

public class TCPServer_2 : MonoBehaviour //TCP server for exercice 3
{
    Socket tcpSocket;

    IPEndPoint ipep;

    Socket client;

    bool exit = false;

    Thread accept;

    public TextManager textManager;

    // Start is called before the first frame update
    void Start()
    {

        tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6969);

        accept = new Thread(Accept);
        accept.Start();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void Accept()
    {
        tcpSocket.Bind(ipep);

        tcpSocket.Listen(1);

        while (!exit)
        {
            client = tcpSocket.Accept(); //Block until it finds a sending socket

            textManager.Say("Server connected with Client");

            bool done = false; //Done with the current lient

            while (!done)
            {
                byte[] buffer = new byte[256];

                int size = client.Receive(buffer); //Receive Ping

                string msg = ASCIIEncoding.ASCII.GetString(buffer);
                textManager.Say(string.Concat(DateTime.Now.ToString(), " Server received: ", msg));
                Debug.Log(string.Concat("Server Received: ", msg));

                Thread.Sleep(1000);

                if (msg.Contains("Ping")) //Client wants pong
                {
                    client.Send(ASCIIEncoding.ASCII.GetBytes("Pong")); //Send Pong
                }
                else if (msg.Contains("Exit")) //Client wants to exit
                {
                    done = true;
                    Debug.Log("Client Exited!");
                    textManager.Say("Server disconnected with Client");
                }
                else
                {
                    Debug.Log("Client message unknown!");
                    client.Send(ASCIIEncoding.ASCII.GetBytes("Unknown")); //Send Pong
                }

            }
        }

    }

    void OnDestroy()
    {
        exit = true;
        accept.Abort();
    }
}

