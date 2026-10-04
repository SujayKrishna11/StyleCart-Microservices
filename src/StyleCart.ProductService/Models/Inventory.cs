namespace StyleCart.ProductService.Models;

public sealed class Inventory
{
    public int Id { get; set; }

    public int ProductVariantId { get; set; }

    public int QuantityInStock { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ProductVariant ProductVariant { get; set; } = null!;
}