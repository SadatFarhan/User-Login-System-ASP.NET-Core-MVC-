using System.ComponentModel.DataAnnotations;

namespace FirstApp.Models
{
    public class Login
    {
        [Required]
        [Display(Name = "Name or Email")]
        public string? NameOrEmail { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}
