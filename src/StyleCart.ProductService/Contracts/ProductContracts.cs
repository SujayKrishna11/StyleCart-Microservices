using System.ComponentModel.DataAnnotations;

namespace StyleCart.ProductService.Contracts;

public sealed class CreateProductRequest
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? Brand { get; set; }

    [Range(0.01, 999999)]
    public decimal BasePrice { get; set; }
}

public sealed class CreateProductVariantRequest
{
    [Required]
    [StringLength(50)]
    public string Size { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Color { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Sku { get; set; } = string.Empty;

    [Range(0.01, 999999)]
    public decimal Price { get; set; }

    [Range(0, 999999)]
    public int QuantityInStock { get; set; }
}

public sealed class UpdateInventoryRequest
{
    [Range(0, 999999)]
    public int QuantityInStock { get; set; }
}

public sealed class ReserveInventoryRequest
{
    [Range(1, 100)]
    public int Quantity { get; set; }
}

public sealed class ProductResponse
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public string? Brand { get; init; }

    public decimal BasePrice { get; init; }

    public bool IsActive { get; init; }

    public bool IsInStock { get; init; }
}

public sealed class ProductVariantResponse
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

public sealed class InventoryResponse
{
    public int ProductVariantId { get; init; }

    public int QuantityInStock { get; init; }

    public DateTime UpdatedAt { get; init; }
}