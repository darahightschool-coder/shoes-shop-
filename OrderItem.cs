using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoeShop.Models;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int ShoeId { get; set; }
    public Shoe? Shoe { get; set; }

    [Required]
    [StringLength(100)]
    public string ShoeName { get; set; } = string.Empty;

    [StringLength(50)]
    public string? ShoeBrand { get; set; }

    [StringLength(20)]
    public string Size { get; set; } = "Standard";

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; } = 1;

    public string? ImageUrl { get; set; }

    [NotMapped]
    public decimal Total => UnitPrice * Quantity;
}
