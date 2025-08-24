using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Mall.Models
{
    public class Product
    {
        public string Id { get; set; }
        [Required(ErrorMessage = "اسم المنتج مطلوب")]
        [Display(Name = "اسم المنتج")]
        public string Name { get; set; }

        [Display(Name = "وصف المنتج")]
        public string Description { get; set; }

        [Required(ErrorMessage = "السعر مطلوب")]
        [Display(Name = "السعر")]
        [Range(0.01, double.MaxValue, ErrorMessage = "السعر يجب أن يكون أكبر من 0")]
        public decimal Price { get; set; }

        [Display(Name = "صورة المنتج")]
        public string Image { get; set; } 

        [Display(Name = "الفئة")]
        public string Category { get; set; }

        [Display(Name = "التقييم")]
        public double Rating { get; set; }

        [Display(Name = "مميز")]
        public bool Featured { get; set; }

        [Display(Name = "يوجد خصم")]
        public bool HaveDiscount { get; set; } = false;

        [Display(Name = "الخصم")]
        public int? Discount { get; set; }

        [Display(Name = "السعر بعد الخصم")]
        public decimal? DiscountedPrice { get; set; }

        [Display(Name = "جديد")]
        public bool IsNew { get; set; }

        [Display(Name = "الكمية في المخزون")]
        public int QuantityInStock { get; set; }

        // Navigation property
        [Display(Name = "المتجر")]
        public string StoreId { get; set; }
        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
    }
}