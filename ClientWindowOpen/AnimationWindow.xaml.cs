using System;
using System.ComponentModel;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace ClientWindowOpen
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class AnimationWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private NotifyIcon _notifyIcon = new NotifyIcon()
        {
            Icon = new Icon("Icons/favicon.ico"),
            Visible = false
        };

        // Глобальная перменная для хранения состояния.
        private TemperatureState _tempState = TemperatureState.Normal;
        public TemperatureState TempState
        {
            get => _tempState;
            set
            {
                if (_tempState != value)
                {
                    _tempState = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _globalTemp = 5;
        public int GlobalTemp
        {
            get => _globalTemp;
            set
            {
                if (_globalTemp != value)
                {
                    _globalTemp = value;
                    OnPropertyChanged();
                }
            }
        }


        DoubleAnimation anim;
        int left;
        int top;
        DependencyProperty prop;
        int end;


        public AnimationWindow()
        {
            InitializeComponent();

            TrayPos tpos = new TrayPos();
            tpos.getXY((int)this.Width, (int)this.Height, out top, out left, out prop, out end);
            this.Top = top;
            this.Left = left;
            anim = new DoubleAnimation(end, TimeSpan.FromSeconds(1));

            DataContext = this;


            if (FunctionConnect())
            {
                MessageTextBox.Content = "Соединение установлено";
            }
            else
            {
                MessageTextBox.Content = "Ошибка соединения. Переподключитесь!";
            }

            _notifyIcon.Click += delegate (object sender, EventArgs args)
            {
                _notifyIcon.Visible = false;

                Show();
                WindowState = WindowState.Normal;
            };
        }

        private void Cold_Button_Click(object sender, RoutedEventArgs e)
        {
            TempState = TemperatureState.Cold;
        }
        private void Normal_Button_Click(object sender, RoutedEventArgs e)
        {
            TempState = TemperatureState.Normal;
        }

        async void GetMessageFromServer(object sender)
        {
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += new EventHandler(TimerTick);
            timer.Interval = new TimeSpan(0, 0, 10);
            timer.Start();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            // Буфер для входящих данных
            byte[] bytes = new byte[1024];

            IPHostEntry ipHost = Dns.GetHostEntry("10.0.10.254");//
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 11000);

            Socket senders = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Соединяем сокет с удаленной точкой
            senders.Connect(ipEndPoint);
            byte[] msg = Encoding.UTF8.GetBytes(Convert.ToString(Convert.ToInt32(TempState) * 5));
            _ = senders.Send(msg);

            int bytesRec = senders.Receive(bytes);
            senders.Disconnect(true);
            Console.WriteLine("\nОтвет от сервера: {0}\n\n", Encoding.UTF8.GetString(bytes, 0, bytesRec));
            if (MessageTextBox != null)
            {
                MessageTextBox.Content = Encoding.UTF8.GetString(bytes, 0, bytesRec).Substring(2);
                var a = Encoding.UTF8.GetString(bytes, 0, 1);
                ScrollValue = Convert.ToInt32(a);
            }


            if (Encoding.UTF8.GetString(bytes, 3, 8) == "Идет" || Encoding.UTF8.GetString(bytes, 2, 8) == "Идет")
            {
                //Show();
                Activate();
                WindowState = WindowState.Normal;
            }

            if (MessageTextBox != null)
                MessageTextBox.Content = Encoding.UTF8.GetString(bytes, 0, bytesRec);

        }

        private int _scrollValue;
        public int ScrollValue
        {
            get { return _scrollValue; }
            set
            {
                if (_scrollValue != value)
                {
                    _scrollValue = value;
                    OnPropertyChanged("ScrollValue");
                }
            }
        }

        public bool FunctionConnect()
        {
            try
            {
                IPHostEntry ipHost = Dns.GetHostEntry("10.0.10.254");//
                IPAddress ipAddr = ipHost.AddressList[0];
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 11000);

                Socket sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Соединяем сокет с удаленной точкой
                sender.Connect(ipEndPoint);

                sender.Disconnect(true);

                GetMessageFromServer(sender);
                Console.WriteLine("Сокет соединяется с {0} ", sender.RemoteEndPoint.ToString());
                return true;
            }
            catch
            {
                return false;
            }
        }

        void SendMessageFromSocket(string mes)
        {
            // Буфер для входящих данных
            byte[] bytes = new byte[1024];

            // Соединяемся с удаленным устройством

            // Устанавливаем удаленную точку для сокета
            IPHostEntry ipHost = Dns.GetHostEntry("localhost");//
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 11000);

            Socket sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                // Соединяем сокет с удаленной точкой
                sender.Connect(ipEndPoint);

                string message = mes;

                Console.WriteLine("Сокет соединяется с {0} ", sender.RemoteEndPoint.ToString());
                byte[] msg = Encoding.UTF8.GetBytes(message);

                // Отправляем данные через сокет
                int bytesSent = sender.Send(msg);

                // Получаем ответ от сервера
                int bytesRec = sender.Receive(bytes);



                Console.WriteLine("\nОтвет от сервера: {0}\n\n", Encoding.UTF8.GetString(bytes, 0, bytesRec));

                var a = Encoding.UTF8.GetString(bytes, 0, 4);
                if (Encoding.UTF8.GetString(bytes, 3, 4) == "Идет" || Encoding.UTF8.GetString(bytes, 2, 4) == "Идет")
                    this.Show();

                if (MessageTextBox != null)
                    MessageTextBox.Content = Encoding.UTF8.GetString(bytes, 0, bytesRec);



                // Используем рекурсию для неоднократного вызова SendMessageFromSocket()
                if (message.IndexOf("<TheEnd>") == -1)
                    //  SendMessageFromSocket(port, mes);

                    // Освобождаем сокет
                    sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
            catch
            {
                Console.WriteLine("Соединение не установлено");
            }

        }

        private void Hot_Button_Click(object sender, RoutedEventArgs e)
        {
            TempState = TemperatureState.Hot;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                _notifyIcon.Visible = true;
                Hide();
            }

            base.OnStateChanged(e);
        }

        // Оповещение об изменении свойства.
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var clock = anim.CreateClock();
            ApplyAnimationClock(prop, clock);
        }
    }
}
