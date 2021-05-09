using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Web.ViewModels
{
    public class RegistrationViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        [Required(ErrorMessage = "ConfirmPassword is required")]
        [Compare(nameof(Password), ErrorMessage = "Passwords are not equal")]
        public string ConfirmPassword { get; set; }
    }
}