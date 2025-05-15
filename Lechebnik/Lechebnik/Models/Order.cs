using System;
using System.Collections.Generic;

namespace Lechebnik.Models
{
    public class Order
    {
        public int ID { get; set; }
        public int Пользователь_ID { get; set; }
        public DateTime Дата_заказа { get; set; }
        public string Статус { get; set; }
        public DateTime? Дата_доставки { get; set; }
        public decimal Сумма { get; set; }
        public int Пункт_выдачи_ID { get; set; }
        public string Способ_оплаты { get; set; }
        public List<OrderItem> Items { get; set; }
    }
}