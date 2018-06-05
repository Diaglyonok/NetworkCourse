using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace ServerElementsGame
{
    public class Client
    {
        private Server myServer;
        private NetworkStream stream;
        private TcpClient tcpClient;
        private string endPoint;
        private string name;


        public TcpClient TcpClient { get => tcpClient; }
        public NetworkStream Stream { get => stream; }
        public Server MyServer { get => myServer; }
        public string EndPoint { get => endPoint; }
        public int Score { get => score; set => score = value; }
        public string Choosed { get => chosed; set => chosed = value; }
        public bool Started { get => started; set => started = value; }
        public bool Ready { get => ready; set => ready = value; }
        public string Name { get => name; set => name = value; }
        public bool Ended { get => ended; set => ended = value; }

        private int score;
        private bool started;
        private bool ready;
        private string chosed;
        private bool ended;

        public Client(TcpClient tcpClient, Server server)
        {
            this.tcpClient = tcpClient;
            this.myServer = server;
            this.endPoint = tcpClient.Client.RemoteEndPoint.ToString();
        }

        public void StartListening()
        {
            try
            {
                stream = TcpClient.GetStream();
                MessageWorker messageWorker = new MessageWorker(this);
                string connectedMessage = GetMessage();
                name = connectedMessage;

                MyServer.BroadcastMessageByEndPoint("connected", EndPoint);
                Console.WriteLine(connectedMessage + " подключился из " + TcpClient.Client.RemoteEndPoint);

                while (true)
                {
                    try
                    {
                        string message = GetMessage();
                        if (message == "" || message == "leave")
                            throw new Exception();

                        messageWorker.receiveMessage(message);

                        Console.WriteLine("mesage from " + EndPoint + ": " + message);
                    }
                    catch (Exception e)
                    {
                        myServer.Clients.Remove(this);
                        myServer.Game.Running = false;
                        if (myServer.Clients.Count == 1 && myServer.Clients[0] != null) { myServer.BroadcastMessageByEndPoint("opponent_disconnected", myServer.Clients[0].EndPoint); }

                        Console.WriteLine("Игрок " + Name + " ip = " + endPoint + " отключился ");
                        //Close();
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                //myServer.Clients.Clear();
                Close();
            }
        }

        public void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (tcpClient != null)
                tcpClient.Client.Close();
        }


        private string GetMessage()
        {
            byte[] data = new byte[64];
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            return builder.ToString();
        }
    }
}