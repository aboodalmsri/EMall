using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace E_Mall.Models
{
    public class AddCategoryModel
    {
        public string Name { get; set; }

        [Display(Name = "صورة الصنف")]
        public IFormFile? Image { get; set; }
    }
}
