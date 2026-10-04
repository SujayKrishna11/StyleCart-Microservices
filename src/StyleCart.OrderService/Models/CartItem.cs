namespace StyleCart.OrderService.Models;

public sealed class CartItem
{
    public int Id { get; set; }

    public int CartId { get; set; }

    public int ProductVariantId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string VariantDescription { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public Cart Cart { get; set; } = null!;
}