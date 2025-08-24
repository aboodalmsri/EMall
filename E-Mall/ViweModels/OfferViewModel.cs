using System.ComponentModel.DataAnnotations;

namespace E_Mall.Models
{
    public class OfferViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "الرجاء إدخال اسم العرض")]
        [Display(Name = "اسم العرض")]
        public string Name { get; set; }

        [Required(ErrorMessage = "الرجاء اختيار نوع العرض")]
        [Display(Name = "نوع العرض")]
        public string Type { get; set; }

        [Required(ErrorMessage = "الرجاء إدخال قيمة العرض")]
        [Display(Name = "قيمة العرض")]
        public decimal Value { get; set; }

        [Required(ErrorMessage = "الرجاء تحديد تاريخ بداية العرض")]
        [Display(Name = "تاريخ البداية")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "الرجاء تحديد تاريخ انتهاء العرض")]
        [Display(Name = "تاريخ الانتهاء")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
    }
}
