using System.ComponentModel.DataAnnotations;

namespace WebATB.Models.Account
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Це поле обов'язкове!")]
        [Display(Name = "Email")]
        public string UsernameOrEmail { get; set; }

        [Required(ErrorMessage = "Це поле обов'язкове!")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        [MinLength(6, ErrorMessage = "Мінімальна довжина пароля 6 символів")]
        public string Password { get; set; }

        [Display(Name = "Не виходити?")]
        public bool RememberMe { get; set; }
    }
}