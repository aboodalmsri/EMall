using System.ComponentModel.DataAnnotations;

namespace E_Mall.Models
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "الاسم الأول مطلوب")]
        [Display(Name = "الاسم الأول")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "الاسم الأخير مطلوب")]
        [Display(Name = "الاسم الأخير")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [RegularExpression(@"^(\+970|05)\d{8}$", ErrorMessage = "رقم هاتف غير صالح. يجب أن يبدأ بـ +970 أو 05 ويتكون من 10 أرقام.")]
        [Display(Name = "رقم الهاتف")]
        public string PhoneNumber { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } // Optional: Include if you allow Email updates
    }
}
