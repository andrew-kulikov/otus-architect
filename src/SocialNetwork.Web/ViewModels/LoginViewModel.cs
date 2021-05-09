using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Web.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username cannot be empty")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password cannot be empty")]
        public string Password { get; set; }
    }
}