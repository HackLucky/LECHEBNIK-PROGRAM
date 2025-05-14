namespace Lechebnik.Models
{
    public class CartItem
    {
        public int ID { get; set; }
        public int Пользователь_ID { get; set; }
        public int Препарат_ID { get; set; }
        public int Количество { get; set; }
    }
}