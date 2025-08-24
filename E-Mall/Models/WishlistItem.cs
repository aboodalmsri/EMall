using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace E_Mall.Models
{
    public class WishlistItem
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string WishlistId { get; set; }

        [Required]
        public string ProductId { get; set; }

        // Navigation properties
        [ForeignKey("WishlistId")]
        public virtual Wishlist Wishlist { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}
