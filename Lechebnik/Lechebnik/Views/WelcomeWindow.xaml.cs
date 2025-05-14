using System;
using System.Windows;
using System.Windows.Threading;

namespace Lechebnik.Views
{
    public partial class WelcomeWindow : Window
    {
        public WelcomeWindow()
        {
            InitializeComponent();
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                LoginWindow loginWindow = new LoginWindow();
                Application.Current.MainWindow = loginWindow;
                Close();
                loginWindow.Show();
            };
            timer.Start();
        }
    }
}