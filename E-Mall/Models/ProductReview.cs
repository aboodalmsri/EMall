using E_Mall.Areas.Identity.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Mall.Models
{
    public class ProductReview
    {
        [Key]
        public string Id { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(500)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public string ProductId { get; set; }
        public string CustomerId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [ForeignKey("CustomerId")]
        public virtual E_MallUser Customer { get; set; }
    }

}
