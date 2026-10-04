namespace StyleCart.ProductService.Models;

public sealed class ProductVariant
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string Size { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public string Sku { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;

    public Product Product { get; set; } = null!;

    public Inventory? Inventory { get; set; }
}