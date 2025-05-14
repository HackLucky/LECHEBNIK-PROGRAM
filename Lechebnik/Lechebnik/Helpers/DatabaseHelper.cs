using System.Data.SqlClient;
using System.Configuration;

namespace Lechebnik.Helpers
{
    public static class DatabaseHelper
    {
        // Строка подключения берется из App.config
        private static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["LechebnikConnection"].ConnectionString;

        // Метод для получения подключения к базе данных
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}