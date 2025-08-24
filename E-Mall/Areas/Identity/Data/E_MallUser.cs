using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace E_Mall.Areas.Identity.Data;

// Add profile data for application users by adding properties to the E_MallUser class
public class E_MallUser : IdentityUser
{
    [Required(ErrorMessage = "الاسم الأول مطلوب")]
    [Display(Name = "الاسم الأول")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "الاسم الأخير مطلوب")]
    [Display(Name = "الاسم الأخير")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "رقم الهاتف مطلوب")]
    [EmailAddress(ErrorMessage = "رقم هاتف غير صالح")]
    [Display(Name = "رقم الهاتف")]
    public string PhoneNumber { get; set; }

}

