using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Mall.Models
{
    public class Merchant
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم التاجر مطلوب")]
        [Display(Name = "اسم التاجر")]
        public string MerchantName { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صالح")]
        [Display(Name = "البريد الإلكتروني")]
        public string MerchantEmail { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Display(Name = "رقم الهاتف")]
        public string MerchantPhone { get; set; }

        [Display(Name = "اسم المتجر")]
        public string StoreName { get; set; }

        [Display(Name = "رابط المتجر")]
        public string StoreLink { get; set; }

        [Display(Name = "نشط")]
        public bool IsActive { get; set; }

        // Foreign Key to AspNetUsers
        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public E_Mall.Areas.Identity.Data.E_MallUser User { get; set; }  // Corrected namespace
        public virtual ICollection<Store> Stores { get; set; } = new List<Store>();
    }
}