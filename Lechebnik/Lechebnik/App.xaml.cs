using Lechebnik.Models;
using System.Windows;

namespace Lechebnik
{
    public partial class App : Application
    {
        // Глобальная переменная для хранения текущего пользователя
        public static User CurrentUser { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }
    }
}