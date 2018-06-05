using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string choosed = "";
        private bool running;
        private bool ready = false;

        public MainWindow()
        {
            InitializeComponent();
            ReadyButton.IsEnabled = false;
            Connector.getInstance().setMainView(this);
            StatusLabel.Content = "Начните игру";
        }


        private void Image_MouseUp_Fire(object sender, MouseButtonEventArgs e)
        {
            if (!running)
                return;
            Connector.getInstance().SendMessage("fire");
            choosed = "fire";
            myChoose.Source = getImageView(Constants.gameCommands.IndexOf(choosed) + 1);
        }

        private void Image_MouseUp_Earth(object sender, MouseButtonEventArgs e)
        {
            if (!running)
                return;
            Connector.getInstance().SendMessage("earth");
            choosed = "earth";
            myChoose.Source = getImageView(Constants.gameCommands.IndexOf(choosed) + 1);
        }

        private void Image_MouseUp_Iron(object sender, MouseButtonEventArgs e)
        {
            if (!running)
                return;
            Connector.getInstance().SendMessage("iron");
            choosed = "iron";
            myChoose.Source = getImageView(Constants.gameCommands.IndexOf(choosed) + 1);
        }

        private void Image_MouseUp_Water(object sender, MouseButtonEventArgs e)
        {
            if (!running)
                return;
            Connector.getInstance().SendMessage("water");
            choosed = "water";
            myChoose.Source = getImageView(Constants.gameCommands.IndexOf(choosed) + 1);
        }

        private void Image_MouseUp_Tree(object sender, MouseButtonEventArgs e)
        {
            if (!running)
                return;
            Connector.getInstance().SendMessage("tree");
            choosed = "tree";
            myChoose.Source = getImageView(Constants.gameCommands.IndexOf(choosed) + 1);
        }

        void Window_Closing(object sender, CancelEventArgs e)
        {
            Connector.getInstance().DisconnectNoReboot();
        }

        public void showMessage(string message)
        {
            if (message == "opponent_disconnected\n")
            {
                ScoreLabel.Content = "Счёт 0 : 0";
                MessageBox.Show("Ваш оппонент отключился от сервера. Вы можете подождать нового.");
            }

            if (message == "game_started\n") 
                startGameProcess();

            if (message.StartsWith("results:"))
            {
                running = false;
                StatusLabel.Content = "Подтвердите готовность";
                string[] results = message.Split(' ');

                string res = results[1];

                if (res == "win" || res == "lose")
                {
                    winLooseLabel.Content = res == "win" ? "Вы победили!" : "Вы проиграли!";
                    winLooseLabel.Visibility = Visibility.Visible;
                    int myRes = Int32.Parse(results[3]);
                    int myScore = Int32.Parse(results[5]);
                    int enemyRes = Int32.Parse(results[7]);
                    int enemyScore = Int32.Parse(results[9]);
                    TextBlock tb = new TextBlock();
                    tb.TextWrapping = TextWrapping.Wrap;
                    if (myRes == 6 || enemyRes == 6)
                        tb.Text = Constants.descrs[100];
                    else
                        tb.Text = Constants.descrs[(myRes-1) * (myRes-1) + (enemyRes-1) * (enemyRes-1)];
                    descriptionLabel.Content = tb;
                    showResults(myRes, enemyRes, myScore, enemyScore);
                }
               else if (res == "draw")
                {
                    winLooseLabel.Content = "Ничья";
                    winLooseLabel.Visibility = Visibility.Visible;
                    int myRes, enemyRes;
                    myRes = enemyRes =  Int32.Parse(results[3]);
                    int myScore = Int32.Parse(results[6]);
                    int enemyScore = Int32.Parse(results[8]);
                    
                    TextBlock tb = new TextBlock();
                    tb.TextWrapping = TextWrapping.Wrap;
                    tb.Text = Constants.descrs[0];
                    descriptionLabel.Content = tb;

                    showResults(myRes, enemyRes, myScore, enemyScore);
                }
            }
            Console.WriteLine(message);
        }


        private void showResults(int myRes, int enemyRes, int myScore, int enemyScore)
        {
            vsImage.Visibility = Visibility.Visible;
            ScoreLabel.Content = "Счёт " + myScore + " : " + enemyScore;
            myChoose.Source = getImageView(myRes);

            enemyChoose.Source = getImageView(enemyRes);
                
            
        }

        private BitmapImage getImageView(int num)
        {
            return new BitmapImage(new Uri(@".\res\element" + num + ".png", UriKind.Relative));
        }


        private void startGameProcess()
        {
            error = false;
            enemyChoose.Source = getImageView(6);
            myChoose.Source = getImageView(6);
            running = true;
            initTimer();
            StartTimer(DateTime.Now);
        }


        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (ready)
                return;
            ReadyButton.IsEnabled = true;
            StatusLabel.Content = "Подтвердите готовность";
            Connector.getInstance().SendMessage("start");
        }

        private void ReadyButton_Click(object sender, RoutedEventArgs e)
        {
            ready = true;
            StatusLabel.Content = "Ожидаем соперника";
            Connector.getInstance().SendMessage("ready");
        }















        private DateTime _startCountdown; // время запуска таймера
        private TimeSpan _startTimeSpan = TimeSpan.FromSeconds(10); // начальное время до окончания таймера
        private TimeSpan _timeToEnd; // время до окончания таймера. Меняется когда таймер запущен
        private TimeSpan _interval = TimeSpan.FromMilliseconds(1); // интервал таймера
        private DateTime _pauseTime;


        private DispatcherTimer _timer;
        private bool error;

        private void initTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = _interval;
            _timer.Tick += delegate
            {
                var now = DateTime.Now;
                var elapsed = now.Subtract(_startCountdown);
                TimeToEnd = _startTimeSpan.Subtract(elapsed);
            };
            StopTimer();
        }


        public TimeSpan TimeToEnd
        {
            get
            {
                return _timeToEnd;
            }

            set
            {
                _timeToEnd = value;
                if (value.TotalMilliseconds <= 0)
                {
                    StopTimer();
                    if (error)
                        return;

                    if (choosed == "")
                    {
                        Connector.getInstance().SendMessage("nothing");
                        StatusLabel.Content = "Вы не успели";
                    }

                    Connector.getInstance().SendMessage("ended_round");
                    choosed = "";
                    return;
                }
                else
                {
                    StatusLabel.Content = (int)(value.TotalMilliseconds / 1000);
                }
                OnPropertyChanged("StringCountdown");
            }
        }

        public string StringCountdown
        {
            get
            {
                var frmt = TimeToEnd.Minutes < 1 ? "ss\\.ff" : "mm\\:ss";
                return _timeToEnd.ToString(frmt);
            }
        }

        public bool TimerIsEnabled
        {
            get { return _timer.IsEnabled; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        private void StopTimer()
        {
            if (TimerIsEnabled)
                _timer.Stop();
            TimeToEnd = _startTimeSpan;

            Console.WriteLine("Timer stopped");
        }

        private void StartTimer(DateTime sDate)
        {
            _startCountdown = sDate;
            _timer.Start();
        }
    }

}
