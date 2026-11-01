using System.ComponentModel.DataAnnotations;

namespace Marketplace.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "ID заказа")]
        public string OrderNumber { get; set; } = string.Empty;
       
        [Display(Name = "Статус")]
        public string Status { get; set; } = "В обработке";
        
        [Required]
        [Display(Name = "Дата получения")]
        [DataType(DataType.Date)]
        public DateTime DeliveryDate { get; set; }

        [Required]
        [Display(Name = "Время получения")]
        public string DeliveryTime { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Способ оплаты")]
        public string PaymentMethod { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Общая сумма")]
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }

        [Required]
        [Display(Name = "Количество товаров")]
        public int ItemCount { get; set; }
    }
}