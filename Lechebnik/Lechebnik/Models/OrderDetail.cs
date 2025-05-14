namespace Lechebnik.Models
{
    public class OrderDetail
    {
        public int ID { get; set; }
        public int Заказ_ID { get; set; }
        public int Препарат_ID { get; set; }
        public int Количество { get; set; }
        public decimal Цена { get; set; }
    }
}