using System.ComponentModel.DataAnnotations;

namespace E_Mall.Models
{
    public class EditStoreModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "اسم المتجر مطلوب")]
        [Display(Name = "اسم المتجر")]
        public string Name { get; set; }

        [Display(Name = "شعار المتجر")]
        public IFormFile? Logo { get; set; }

        [Display(Name = "صورة الغلاف")]
        public IFormFile? CoverImage { get; set; }

        [Display(Name = "الوصف")]
        public string Description { get; set; }


        [Display(Name = "الموقع")]
        public string Location { get; set; }
        
        [Required(ErrorMessage = "الفئة مطلوبة")]
        [Display(Name = "الفئة")]
        public string CategoryId { get; set; }

    }
}
