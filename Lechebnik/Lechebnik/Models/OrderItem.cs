namespace Lechebnik.Models
{
    public class OrderItem
    {
        public int ID { get; set; }
        public int Препарат_ID { get; set; }
        public int Количество { get; set; }
        public decimal Цена { get; set; }
        public string Название { get; set; }
        public decimal TotalPrice => Количество * Цена;
    }
}