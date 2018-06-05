using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace ServerElementsGame
{
    public class Server
    {
        public TcpListener tcpListener;
        private List<Client> clients;
        private GameProcess gameProcess;

        public List<Client> Clients { get => clients; set => clients = value; }
        public GameProcess Game { get => gameProcess; }

        protected internal void StartListen()
        {
            Clients = new List<Client>();
            gameProcess = new GameProcess(this);
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 8888);
                tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    Client client = new Client(tcpClient, this);

                    if (clients.Count < 2)
                    {
                        Clients.Add(client);
                        Thread clientThread = new Thread(new ThreadStart(client.StartListening));
                        clientThread.Start();
                    }
                    else
                    {
                        byte[] data = Encoding.Unicode.GetBytes("max_gamers" + "\n");
                        tcpClient.GetStream().Write(data, 0, data.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }

        internal void outputClients()
        {
           Console.WriteLine("Клиенты на сервере:");
           foreach (Client c in Clients)
            {
                if (c != null)
                    Console.WriteLine(c.EndPoint + " " + c.Name);
            }
        }

        internal bool checkClientsReady()
        {
            foreach (Client c in Clients)
            {
                if (!c.Ready)
                {
                    Console.WriteLine(c.Name + " Не готов");
                    return false;
                }
            }

            return true;
        }

        internal bool checkClientsStarted()
        {
            foreach (Client c in Clients)
            {
                if (!c.Started)
                {
                    Console.WriteLine(c.Name + " Не начал игру" );
                    return false;
                }
            }

            return true;
        }

        internal bool checkClients()
        {
            try
            {
                return clients[0] != null && clients[1] != null;
            }
            catch
            {
                return false;
            }
        }

        internal void Restart()
        {
            Clients.Clear();
        }

        protected internal void BroadcastMessageByEndPoint(string message, string endPoint)
        {
            byte[] data = Encoding.Unicode.GetBytes(message + "\n");
            foreach (Client client in Clients)
            {
                if (client != null && client.EndPoint == endPoint)
                    client.Stream.Write(data, 0, data.Length);
            }
            Thread.Sleep(100);
        }

        internal Client getOpponent(Client client)
        {
            if (Clients[0] == client)
                return Clients[1];
            else
                return Clients[0];
        }

        internal void BroadcastMessage(string message)
        {
            byte[] data = Encoding.Unicode.GetBytes(message + "\n");
            for (int i = 0; i < Clients.Count; i++)
            {
                if (Clients[i] != null)
                    Clients[i].Stream.Write(data, 0, data.Length);
            }
            Thread.Sleep(100);
        }

        protected internal void Disconnect()
        {
            tcpListener.Stop();

            foreach (Client client in Clients)
            {
                if (client != null)
                    client.Close();
            }
            Environment.Exit(0);
        }
    }
}