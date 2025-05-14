namespace Lechebnik.Models
{
    public class User
    {
        public int ID { get; set; }
        public string Логин { get; set; }
        public string Пароль { get; set; } // Хешированный пароль
        public string Фамилия { get; set; }
        public string Имя { get; set; }
        public string Отчество { get; set; }
        public string Телефон { get; set; }
        public string Почта { get; set; }
        public int Роль { get; set; } // 1 - Пользователь, 2 - Администратор
        public int Статус { get; set; } // 1 - Активен, 2 - Заблокирован
    }
}