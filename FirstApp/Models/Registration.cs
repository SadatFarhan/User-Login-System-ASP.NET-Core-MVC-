using System.ComponentModel.DataAnnotations;

namespace FirstApp.Models
{
    public class Registration
    {



        [Key]
        public int Id { get; set; }
        [Required]
        [DataType(DataType.Text)]
        public string? Name { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
        [Required]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }
    }
}
