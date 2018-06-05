using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для Start.xaml
    /// </summary>
    public partial class Start : Window
    {
        public Start()
        {
            Thread.Sleep(1000);
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Connector.getInstance().StartView = this;
            string name = nameTextBox.Text != "" ? nameTextBox.Text : "";
            string ip = ipTextBox.Text != "" ? ipTextBox.Text : "";

            if (name == "" || ip == "")
                return;

            Connector.getInstance().Connect(ip, name, this);
        }

        public void connectedAnswer(string answer)
        {
            if (answer == "connected\n")
            {
                MainWindow main = new MainWindow();
                main.Show();
                Hide();
            }
            else if (answer == "not_connected\n")
            {
                MessageBox.Show("Подключение не удалось. Попробуйте еще раз");
            }
            else if (answer == "max_gamers\n")
            {
                MessageBox.Show("На сервере уже максимальное количество игроков, попробуйте позже");
            }
        }


        void Window_Closing(object sender, CancelEventArgs e)
        {
            Connector.getInstance().DisconnectNoReboot();
        }

    }
}
