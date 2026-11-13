namespace StyleCart.OrderService.Contracts;

public sealed class ProductRecommendationResponse
{
    public int ProductVariantId { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public string VariantDescription { get; init; } = string.Empty;

    public decimal UnitPrice { get; init; }

    public int PurchaseCount { get; init; }
}
