using E_Mall.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Mall.Models
{
    public class Store
    {
        [Key]
        public string Id { get; set; } // تأكد من أن هذا هو المفتاح الأساسي

        [Required(ErrorMessage = "اسم المتجر مطلوب")]
        [Display(Name = "اسم المتجر")]
        [MaxLength(255)] // إضافة حد أقصى للطول
        public string Name { get; set; }

        [Display(Name = "شعار المتجر")]
        public string Logo { get; set; }

        [Display(Name = "صورة الغلاف")]
        public string CoverImage { get; set; }

        [Display(Name = "الوصف")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Display(Name = "مميز")]
        public bool Featured { get; set; } = false;

        [Display(Name = "التقييم")]
        [Range(0, 5, ErrorMessage = "التقييم يجب أن يكون بين 0 و 5")]
        public double Rating { get; set; } = 0;

        [Display(Name = "الموقع")]
        [MaxLength(255)] // إضافة حد أقصى للطول
        public string Location { get; set; }

        [Display(Name = "عدد المنتجات")]
        public int ProductCount { get; set; } = 0;

        [Display(Name = "عدد المنتجات الحالي")]
        public int CurrentProductCount { get; set; } = 0;

        [Required(ErrorMessage = "التاجر مطلوب")]
        [Display(Name = "التاجر")]
        public int MerchantId { get; set; }

        [ForeignKey("MerchantId")]
        public virtual Merchant Merchant { get; set; }

        [Required(ErrorMessage = "الفئة مطلوبة")]
        [Display(Name = "الفئة")]
        public string CategoryId { get; set; }

        [ForeignKey("CategoryId")]

        public virtual Category Category { get; set; }

        [Display(Name = "نشط")]
        public bool IsActive { get; set; } = true;

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}