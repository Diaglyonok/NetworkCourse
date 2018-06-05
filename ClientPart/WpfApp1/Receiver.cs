using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    public class Receiver
    {
        TcpClient client;
        NetworkStream stream;

        public Receiver(TcpClient client, NetworkStream stream)
        {
            this.client = client;
            this.stream = stream;
        }

        public class WorkerResultEventArgs : EventArgs
        {
            private string _result;
            public string Result
            {
                get
                {
                    return _result;
                }
            }

            public WorkerResultEventArgs(string result)
            {
                _result = result;
            }
        }

        public delegate void WorkerResultEventHandler(object sender, WorkerResultEventArgs e);
        
        public event WorkerResultEventHandler Finished;

        public void ReceiveMessage()
        {

            while (true)
            {
                string message = "";
                try
                {
                    byte[] data = new byte[64];
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    message = builder.ToString();

                    WorkerResultEventArgs e = new WorkerResultEventArgs(message);

                    OnFinished(e);

                }
                catch (Exception e)
                {
                    Connector.getInstance().DisconnectReboot();
                }
            }
        }


        protected void OnFinished(WorkerResultEventArgs e)
        {

            if (Finished == null)
                return;
            try
            {
                Application.Current.Dispatcher.Invoke(Finished, new object[] { this, e });
            }
            catch
            {
                Console.WriteLine("onFinished error");
            }
            
        }
    }
}

