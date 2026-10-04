using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace StyleCart.OrderService.Services;

public sealed class ProductServiceClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ProductServiceClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ProductVariantInfo?> GetVariantAsync(int variantId)
    {
        var client = _httpClientFactory.CreateClient("ProductService");

        return await client.GetFromJsonAsync<ProductVariantInfo>(
            $"/api/products/variants/{variantId}");
    }

    public async Task<bool> ReserveStockAsync(
        int variantId,
        int quantity,
        string authorizationHeader)
    {
        var client = _httpClientFactory.CreateClient("ProductService");

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/inventory/{variantId}/reserve");

        request.Content = JsonContent.Create(new { quantity });

        request.Headers.Authorization =
            AuthenticationHeaderValue.Parse(authorizationHeader);

        using var response = await client.SendAsync(request);

        return response.IsSuccessStatusCode;
    }
}

public sealed class ProductVariantInfo
{
    public int Id { get; init; }

    public int ProductId { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public string Size { get; init; } = string.Empty;

    public string Color { get; init; } = string.Empty;

    public string Sku { get; init; } = string.Empty;

    public decimal Price { get; init; }

    public bool IsActive { get; init; }

    public int QuantityInStock { get; init; }

    public bool IsInStock { get; init; }
}