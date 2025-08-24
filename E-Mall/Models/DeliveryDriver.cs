using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Import this

namespace E_Mall.Models
{
    public class DeliveryDriver
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        [Display(Name = "اسم المستخدم")]
        public string DriverName { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صالح")]
        [Display(Name = "البريد الإلكتروني")]
        public string DriverEmail { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [RegularExpression(@"^(\+970|05)\d{8}$", ErrorMessage = "رقم هاتف غير صالح. يجب أن يبدأ بـ +970 أو 05 ويتكون من 10 أرقام.")]
        [Display(Name = "رقم الهاتف")]
        public string DriverPhone { get; set; }

        [Display(Name = "معلومات السيارة")]
        public string DriverCarInfo { get; set; }

        [Required(ErrorMessage = "منطقة التغطية مطلوبة")]
        [Display(Name = "منطقة التغطية")]
        public string CoverageArea { get; set; }

        [Display(Name = "الحالة")]
        public bool IsActive { get; set; } // True if active, false if inactive

        // Foreign Key to AspNetUsers table
        [Required] // Or remove if you want it optional
        public string UserId { get; set; } // Data type MUST match the type of Id in AspNetUsers (string in this case)

        [ForeignKey("UserId")] // Navigation Property
        public E_Mall.Areas.Identity.Data.E_MallUser User { get; set; }  // Corrected namespace
    }
}