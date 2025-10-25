using System.ComponentModel.DataAnnotations;

namespace WebATB.Models.Account
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }

        [Display(Name = "Новий пароль")]
        [Required(ErrorMessage = "Пароль є обов'язковим.")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Мінімальна довжина пароля — 6 символів.")]

        public string Password { get; set; }

        [Display(Name = "Підтвердження пароля")]
        [Required(ErrorMessage = "Підтвердження пароля є обов'язковим.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Паролі не співпадають.")]
        public string ConfirmPassword { get; set; }
    }
}