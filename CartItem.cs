namespace ShoeShop.Models;

public class CartItem
{
    public int ShoeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public int Quantity { get; set; } = 1;
    public string? Size { get; set; }

    public decimal Total => Price * Quantity;
}
