using System.ComponentModel.DataAnnotations;

namespace WebATB.Models.Account
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email є обов'язковим.")]
        [EmailAddress(ErrorMessage = "Некоректний email.")]
        public string Email { get; set; }
    }
}