using System.ComponentModel.DataAnnotations;

namespace ShoeShop.ViewModels.Account;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "The password must be at least {2} characters long.")]
    [DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$",
        ErrorMessage = "Password must contain at least 1 uppercase letter, 1 lowercase letter, 1 digit, and 1 special character.")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "Shipping Address")]
    [MaxLength(250)]
    public string? Address { get; set; }

    [Display(Name = "City")]
    [MaxLength(100)]
    public string? City { get; set; }

    [Display(Name = "Postal Code")]
    [MaxLength(20)]
    [DataType(DataType.PostalCode)]
    public string? PostalCode { get; set; }
}
