using System.ComponentModel.DataAnnotations;

namespace StyleCart.OrderService.Contracts;

public sealed class AddCartItemRequest
{
    [Range(1, int.MaxValue)]
    public int ProductVariantId { get; set; }

    [Range(1, 10)]
    public int Quantity { get; set; }
}

public sealed class UpdateCartItemRequest
{
    [Range(1, 10)]
    public int Quantity { get; set; }
}

public sealed class CheckoutRequest
{
    [Required]
    [StringLength(150)]
    public string RecipientName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string ShippingAddress { get; set; } = string.Empty;
}

public sealed class CartItemResponse
{
    public int Id { get; init; }

    public int ProductVariantId { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public string VariantDescription { get; init; } = string.Empty;

    public decimal UnitPrice { get; init; }

    public int Quantity { get; init; }

    public decimal LineTotal { get; init; }
}

public sealed class CartResponse
{
    public int Id { get; init; }

    public DateTime UpdatedAt { get; init; }

    public IReadOnlyList<CartItemResponse> Items { get; init; } = [];
}

public sealed class OrderItemResponse
{
    public int ProductVariantId { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public string VariantDescription { get; init; } = string.Empty;

    public decimal UnitPrice { get; init; }

    public int Quantity { get; init; }

    public decimal LineTotal { get; init; }
}

public sealed class OrderResponse
{
    public int Id { get; init; }

    public DateTime OrderDate { get; init; }

    public decimal TotalAmount { get; init; }

    public string OrderStatus { get; init; } = string.Empty;

    public string RecipientName { get; init; } = string.Empty;

    public string ShippingAddress { get; init; } = string.Empty;

    public IReadOnlyList<OrderItemResponse> Items { get; init; } = [];
}