﻿/////
/// Author:
/////

using System;
using System.Diagnostics;
using System.Text.Json;
using System.Net.Sockets;
using System.Collections.Generic;
using Networking.Queues;
using System.Reflection;
using System.Net;
using Networking.Utils;

namespace Networking.Communicator
{
    public class Server : ICommunicator
    {
        private bool _stopThread=false;
        private Sender _sender;
        private Thread _listenThread;
        private Receiver _receiver;
        private TcpListener _serverListener;
        Dictionary<string, NetworkStream> _clientIDToStream = new();
        private Dictionary<string, IEventHandler> _moduleEventMap = new();

        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public void Send(string serializedObj, string eventType, string destID)
        {
            Console.WriteLine("[Server] Send" + serializedObj + " " + eventType + " " + destID);
            _sender.Send(serializedObj, eventType, destID,"server");
        }   

        public string Start(string? destIP, int? destPort,string senderID)
        {
            Console.WriteLine("[Server] Start" + destIP + " " + destPort);
            _sender = new(_clientIDToStream,false);
            _receiver = new(_clientIDToStream, _moduleEventMap);

            int port = 12345;
            while (true)
            {
                try
                {
                    _serverListener = new TcpListener(IPAddress.Any, port);
                    _serverListener.Start();
                    break;
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
                    {
                        Random random = new();
                        port = random.Next();
                    }
                    else
                    {
                        Console.WriteLine("Socket error: " + ex.SocketErrorCode);
                        throw ex;
                    }
                }
            }
            IPEndPoint localEndPoint = (IPEndPoint)_serverListener.LocalEndpoint;
            Console.WriteLine("[Server] Server is listening on:");
            Console.WriteLine("[Server] IP Address: " + GetLocalIPAddress());
            Console.WriteLine("[Server] Port: " + localEndPoint.Port);

            _listenThread = new Thread(AcceptConnection);
            _listenThread.Start();
            Subscribe(new NetworkingEventHandler(), "networking");
            return localEndPoint.Address + ":" + localEndPoint.Port;
        }

        public void Stop()
        {
            Console.WriteLine("[Server] Stop");
            _stopThread = true;
            _sender.Stop();
            _receiver.Stop();
            foreach (var stream in _clientIDToStream.Values)
            {
                stream.Close(); // Close the network stream
            }

            Console.WriteLine("[Server] Stopped _sender and _receiver");
            _listenThread.Interrupt();
            _serverListener.Stop();
            //_listenThread.Join();
            Console.WriteLine("[Server] Stopped");
        }

        public void Subscribe(IEventHandler eventHandler, string moduleName)
        {
            Console.WriteLine("[Server] Subscribe");
            _moduleEventMap.Add(moduleName, eventHandler);
        }

        void AcceptConnection()
        {
            string clientID = "A";

            while (!_stopThread)
            {
                Console.WriteLine("waiting for connection");
                TcpClient client=new();
                try
                {
                    client = _serverListener.AcceptTcpClient();
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode == SocketError.Interrupted)
                    {
                        Console.WriteLine("[Server] Listener stopped");
                        break;
                    }
                }
                NetworkStream stream = client.GetStream();
                _clientIDToStream.Add(clientID, stream);
                clientID += 'A';
                Console.WriteLine("client connected");
            }
        }
    }
}
