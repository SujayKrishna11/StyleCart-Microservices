namespace StyleCart.OrderService.Models;

public sealed class Cart
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}