using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models
{
    //this dto model will be associated with the reset password form/page/cshtml/razor view
    public class PasswordResetDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";
        [Required, MaxLength(100)]
        public string Password { get; set; }//new password of user
        [Required(ErrorMessage = "The Confirm Password field is required")]
        [Compare("Password", ErrorMessage = "Confirm Password and Password do not match")]
        public string ConfirmPassword { get; set; } = "";
    }
}
