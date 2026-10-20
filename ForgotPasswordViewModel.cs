using System.ComponentModel.DataAnnotations;

namespace ShoeShop.ViewModels.Account;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [Display(Name = "Account Email")]
    public string Email { get; set; } = string.Empty;
}
