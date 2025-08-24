using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace E_Mall.Models
{
    public class CartItem
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string CartId { get; set; }

        [Required]
        public string ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون على الأقل 1")] // Localized validation
        [Display(Name = "الكمية")]
        public int Quantity { get; set; }

        // Navigation properties
        [ForeignKey("CartId")]
        public virtual Cart Cart { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}
