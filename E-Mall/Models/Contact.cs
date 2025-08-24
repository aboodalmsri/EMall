using System.ComponentModel.DataAnnotations;

namespace E_Mall.Models
{
    public class Contact
    {
        [Key]
        public int id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; }

        [StringLength(200, ErrorMessage = "Subject cannot exceed 200 characters")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Message is required")]
        [StringLength(5000, ErrorMessage = "Message cannot exceed 5000 characters")]
        public string Message { get; set; }
        public string? ReplyMessage { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsReplied { get; set; } = false;
        public DateTime? RepliedAt { get; set; }

    }
}
