// Models/Review.cs
using E_Mall.Areas.Identity.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Mall.Models
{
    public class Review
    {
        [Key]
        public string Id { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(500)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign keys - Changed StoreId to string to match Store.Id
        [Required]
        public string StoreId { get; set; }
        public string CustomerId { get; set; }

        // Navigation properties
        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }

        [ForeignKey("CustomerId")]
        public virtual E_MallUser Customer { get; set; }

    }
}