using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace E_Mall.Models
{
    public class AddStoreModel
    {

        [Required(ErrorMessage = "اسم المتجر مطلوب")]
        [Display(Name = "اسم المتجر")]
        public string Name { get; set; }

        [Display(Name = "شعار المتجر")]
        public IFormFile? Logo { get; set; }

        [Display(Name = "صورة الغلاف")]
        public IFormFile? CoverImage { get; set; }

        [Display(Name = "الوصف")]
        public string Description { get; set; }

        [Display(Name = "مميز")]
        public bool Featured { get; set; } = false;

        [Display(Name = "الموقع")]
        public string Location { get; set; }

        [Display(Name = "عدد المنتجات")]
        public int ProductCount { get; set; } = 0;

        [Required(ErrorMessage = "التاجر مطلوب")]
        [Display(Name = "التاجر")]
        public int MerchantId { get; set; }

        [Required(ErrorMessage = "الفئة مطلوبة")]
        [Display(Name = "الفئة")]
        public string CategoryId { get; set; }

        [Display(Name = "نشط")]
        public bool IsActive { get; set; } = false;
    }
}