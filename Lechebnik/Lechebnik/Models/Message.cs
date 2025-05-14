using System;

namespace Lechebnik.Models
{
    public class Message
    {
        public int ID { get; set; }
        public int Отправитель_ID { get; set; }
        public int Получатель_ID { get; set; }
        public string Сообщение { get; set; }
        public DateTime Дата_отправки { get; set; }
    }
}