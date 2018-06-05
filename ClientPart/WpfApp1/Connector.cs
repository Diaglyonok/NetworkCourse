using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    class Connector
    {
        private static int port = 8888;
        Start startView;
        Window view;
        TcpClient client;
        NetworkStream stream;


        private static Connector instance;

        public Window View { get => view; }
        public TcpClient Client { get => client; set => client = value; }
        public Start StartView { get => startView; set => startView = value; }

        private Connector() { }

        public static Connector getInstance()
        {
            if (instance == null)
                instance = new Connector();
            return instance;
        }

        internal void setMainView(MainWindow mainWindow)
        {
            view = mainWindow;
        }

        public void Connect(string host, string username, Start view)
        {
            StartView = (Start)this.view;
            this.view = view;

            try
            {
                Client = new TcpClient();

                Client.Connect(host, port);
                stream = Client.GetStream();

                string message = username;
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);

                Receiver r = new Receiver(Client, stream);
                r.Finished += new Receiver.WorkerResultEventHandler(handleMessage);

                Thread receiveThread = new Thread(new ThreadStart(r.ReceiveMessage));
                receiveThread.Start();

            }
            catch (Exception ex)
            {
                view.connectedAnswer("not_connected\n");
                Console.WriteLine("Not Connected");
            }
        }

        public void DisconnectReboot()
        {
            if (Client == null || stream == null)
                return;
            
            Client.Close();
            stream.Close();

            MessageBox.Show("Произошло экстренное отключение сервера. Вы будете отключены.");
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Environment.Exit(0);
        }

        public void DisconnectNoReboot()
        {
            if (Client == null || stream == null)
                return;

            SendMessage("leave");
            Client.Close();
            stream.Close();

            Environment.Exit(0);
        }

        private void handleMessage(object sender, Receiver.WorkerResultEventArgs e)
        {
            if (View is Start)
            {
                ((Start)View).connectedAnswer(e.Result);
            }
            else if (View is MainWindow)
            {
                ((MainWindow)View).showMessage(e.Result);
            }
        }


        public void SendMessage(string message)
        {
            if (stream.CanWrite)
            {
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);
                Thread.Sleep(10);
            }
        }

    }

    

}
