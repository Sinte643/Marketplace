using System.ComponentModel.DataAnnotations;

namespace Marketplace.Models
{
    public class Card
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [Display(Name = "Номер карты")]
        [StringLength(16, MinimumLength = 16)]
        public string CardNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Месяц")]
        public int ExpiryMonth { get; set; }

        [Required]
        [Display(Name = "Год")]
        public int ExpiryYear { get; set; }

        [Required]
        [Display(Name = "CVV")]
        [StringLength(3, MinimumLength = 3)]
        public string CVV { get; set; } = string.Empty;

        [Display(Name = "Тип карты")]
        public string CardType { get; set; } = string.Empty; // Visa, MasterCard, МИР
    }
}