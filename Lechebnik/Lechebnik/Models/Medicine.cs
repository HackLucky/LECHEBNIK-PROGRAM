namespace Lechebnik.Models
{
    public class Medicine
    {
        public int ID { get; set; }
        public string Название { get; set; }
        public bool Требование_рецепта { get; set; } // true - требуется рецепт
        public int Количество_на_складе { get; set; }
        public decimal Цена { get; set; } // В рублях
        public string Применение_при_симптомах { get; set; }
        public string Способ_применения { get; set; }
        public string Агрегатное_состояние { get; set; }
        public string Тип_препарата { get; set; }
        public string Поставщик { get; set; }
        public string Страна_изготовления { get; set; }
    }
}