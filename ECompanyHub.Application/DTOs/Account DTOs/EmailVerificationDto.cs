using System.ComponentModel.DataAnnotations;

namespace ECompanyHub.Application.DTOs.Account_DTOs
{
    public class EmailVerificationDto
    {
        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }
    }
}
