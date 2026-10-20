using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoeShop.Models;

public class Order
{
    public int Id { get; set; }

    [Required]
    [StringLength(30)]
    public string OrderNumber { get; set; } = string.Empty;

    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    [Required]
    [StringLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string CustomerPhone { get; set; } = string.Empty;

    [Required]
    [StringLength(300)]
    public string ShippingAddress { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string PaymentMethod { get; set; } = "COD";

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Pending - Cash on Delivery";

    [StringLength(250)]
    [Display(Name = "Cancellation Reason")]
    public string? CancellationReason { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<OrderItem> OrderItems { get; set; } = new();
}
