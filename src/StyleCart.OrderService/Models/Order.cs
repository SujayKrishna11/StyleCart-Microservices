namespace StyleCart.OrderService.Models;

public sealed class Order
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public decimal TotalAmount { get; set; }

    public string OrderStatus { get; set; } = "Placed";

    public string RecipientName { get; set; } = string.Empty;

    public string ShippingAddress { get; set; } = string.Empty;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}