using System.ComponentModel.DataAnnotations;

namespace E_Mall.Models
{
    public class AddProductsModel
    {
        [Required(ErrorMessage = "اسم المنتج مطلوب")]
        [Display(Name = "اسم المنتج")]
        public string Name { get; set; }

        [Display(Name = "وصف المنتج")]
        public string Description { get; set; }

        [Required(ErrorMessage = "السعر مطلوب")]
        [Display(Name = "السعر")]
        [Range(0.01, double.MaxValue, ErrorMessage = "السعر يجب أن يكون أكبر من 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "الكمية في المخزون مطلوبة")]
        [Display(Name = "الكمية في المخزون")]
        [Range(1, int.MaxValue, ErrorMessage = "الكمية في المخزون يجب أن تكون أكبر من 0")]
        public int QuantityInStock { get; set; }

        [Display(Name = "صورة المنتج")]
        public IFormFile? ImageFile { get; set; } 

        [Display(Name = "الفئة")]
        public string Category { get; set; }

        public bool HaveDiscount { get; set; } = false;

        [Display(Name = "الخصم (%)")]
        [Range(0, 100, ErrorMessage = "الخصم يجب أن يكون بين 0 و 100")]
        public int? Discount { get; set; }

        [Display(Name = "السعر بعد الخصم")]
        public decimal? DiscountedPrice { get; set; }

        [Display(Name = "التقييم")]
        public double Rating { get; set; }

        [Display(Name = "مميز")]
        public bool Featured { get; set; }

        [Display(Name = "جديد")]
        public bool IsNew { get; set; }

    }
}
