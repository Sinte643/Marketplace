using System.ComponentModel.DataAnnotations;

namespace Marketplace.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Название товара обязательно")]
        [StringLength(100)]
        [Display(Name = "Название")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Категория")]
        public string Category { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Подкатегория")]
        public string? SubCategory { get; set; }

        [Required]
        [Range(0, 1000000, ErrorMessage = "Цена не может быть отрицательной")]
        [Display(Name = "Цена")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Display(Name = "Изображение")]
        public string? ImageUrl { get; set; }

        [StringLength(1000)]
        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Display(Name = "Состав")]
        public string? Composition { get; set; }

        [Display(Name = "Инструкция по уходу")]
        public string? CareInstructions { get; set; }

        [Display(Name = "Доступные размеры")]
        public string? AvailableSizes { get; set; } // Будем хранить как строку через запятую: "S,M,L,XL"
        [Display(Name = "Количество на складе")]
        [Range(0, 10000)]
        public int Stock { get; set; } = 0;
    }
}