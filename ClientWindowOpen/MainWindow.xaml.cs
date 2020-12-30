using System.Windows;

namespace ClientWindowOpen
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        AnimationWindow notify;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            notify = new AnimationWindow();

            notify.Show();

            this.Close();

        }

        private void Cold_Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
