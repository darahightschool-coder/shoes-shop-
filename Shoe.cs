using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoeShop.Models;

public class Shoe
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Brand name is required.")]
    [StringLength(50)]
    public string Brand { get; set; } = string.Empty;

    [Required(ErrorMessage = "Shoe name is required.")]
    [StringLength(100)]
    [Display(Name = "Shoe Name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Category { get; set; } = "Lifestyle";

    [StringLength(50)]
    public string? Color { get; set; } = "Black";

    [Required]
    [Range(1.00, 10000.00, ErrorMessage = "Price must be between $1.00 and $10,000.00.")]
    [Column(TypeName = "decimal(18,2)")]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    [Display(Name = "Stock Quantity")]
    [Range(0, 100000, ErrorMessage = "Stock quantity cannot be negative.")]
    public int StockQuantity { get; set; } = 50;

    [NotMapped]
    public int Stock
    {
        get => StockQuantity;
        set => StockQuantity = value;
    }

    [Display(Name = "Image URL")]
    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
