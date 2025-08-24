using E_Mall.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Mall.Models
{
    public class Order
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();  //Consider setting default ID.  This ensures one will always exist

        [Required]
        public string UserId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;  //Consider setting a default date

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be greater than 0")]
        public decimal TotalAmount { get; set; }

        [Required]
        public string Status { get; set; } = "Pending"; // Consider an enum

        [ForeignKey("UserId")] // <-- ربط بالمفتاح الأجنبي
        public virtual E_MallUser User { get; set; } // <-- الخاصية الجديدة

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
