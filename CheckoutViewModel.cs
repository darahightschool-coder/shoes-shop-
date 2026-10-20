using System.ComponentModel.DataAnnotations;
using ShoeShop.Models;

namespace ShoeShop.ViewModels;

public enum PaymentMethodOption
{
    [Display(Name = "Cash on Delivery (COD)")]
    COD,

    [Display(Name = "PayPal")]
    PayPal,

    [Display(Name = "QR Code / Mobile Banking")]
    QRCode
}

public class CheckoutViewModel
{
    // ==========================================
    // 1. Customer Information
    // ==========================================
    [Required(ErrorMessage = "Full Name is required.")]
    [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters.")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    [StringLength(25, MinimumLength = 7, ErrorMessage = "Phone number must be between 7 and 25 characters.")]
    [Display(Name = "Phone Number")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Shipping address is required.")]
    [StringLength(250, ErrorMessage = "Shipping address cannot exceed 250 characters.")]
    [Display(Name = "Shipping Address")]
    public string ShippingAddress { get; set; } = string.Empty;

    // ==========================================
    // 2. Payment Method Selection ("COD", "PayPal", "QRCode")
    // ==========================================
    [Required(ErrorMessage = "Please select a payment method.")]
    [Display(Name = "Payment Method")]
    public string PaymentMethod { get; set; } = "COD";

    // ==========================================
    // 3. Order Summary Data (retrieved from Session)
    // ==========================================
    public List<CartItem> CartItems { get; set; } = new();

    [Display(Name = "Total Amount")]
    [DataType(DataType.Currency)]
    public decimal TotalAmount => CartItems.Sum(item => item.Total);
}
