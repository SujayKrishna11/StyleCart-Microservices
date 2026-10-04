using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StyleCart.ProductService.Contracts;
using StyleCart.ProductService.Data;
using StyleCart.ProductService.Models;

namespace StyleCart.ProductService.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly ProductDbContext _dbContext;

    public ProductsController(ProductDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetAll()
    {
        var products = await _dbContext.Products
            .AsNoTracking()
            .Include(product => product.Variants)
            .ThenInclude(variant => variant.Inventory)
            .Where(product => product.IsActive)
            .OrderBy(product => product.Name)
            .ToListAsync();

        return Ok(products.Select(MapProduct).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponse>> GetById(int id)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .Include(product => product.Variants)
            .ThenInclude(variant => variant.Inventory)
            .FirstOrDefaultAsync(product => product.Id == id && product.IsActive);

        return product is null ? NotFound() : Ok(MapProduct(product));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request)
    {
        var product = new Product
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Brand = request.Brand?.Trim(),
            BasePrice = request.BasePrice
        };

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            MapProduct(product));
    }

    [HttpGet("{id:int}/variants")]
    public async Task<ActionResult<IReadOnlyList<ProductVariantResponse>>> GetVariants(int id)
    {
        var productExists = await _dbContext.Products
            .AnyAsync(product => product.Id == id && product.IsActive);

        if (!productExists)
        {
            return NotFound();
        }

        var variants = await _dbContext.ProductVariants
            .AsNoTracking()
            .Include(variant => variant.Inventory)
            .Include(variant => variant.Product)
            .Where(variant => variant.ProductId == id && variant.IsActive)
            .OrderBy(variant => variant.Color)
            .ThenBy(variant => variant.Size)
            .ToListAsync();

        return Ok(variants.Select(MapVariant).ToList());
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/variants")]
    public async Task<ActionResult<ProductVariantResponse>> CreateVariant(
        int id,
        CreateProductVariantRequest request)
    {
        var productExists = await _dbContext.Products
            .AnyAsync(product => product.Id == id && product.IsActive);

        if (!productExists)
        {
            return NotFound(new { message = "Product was not found." });
        }

        var sku = request.Sku.Trim().ToUpperInvariant();

        var skuExists = await _dbContext.ProductVariants
            .AnyAsync(variant => variant.Sku == sku);

        if (skuExists)
        {
            return Conflict(new { message = "A variant already uses this SKU." });
        }

        var variant = new ProductVariant
        {
            ProductId = id,
            Size = request.Size.Trim(),
            Color = request.Color.Trim(),
            Sku = sku,
            Price = request.Price,
            Inventory = new Inventory
            {
                QuantityInStock = request.QuantityInStock
            }
        };

        _dbContext.ProductVariants.Add(variant);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetVariants),
            new { id },
            MapVariant(variant));
    }


    [HttpGet("variants/{variantId:int}")]
    public async Task<ActionResult<ProductVariantResponse>> GetVariantById(int variantId)
    {
        var variant = await _dbContext.ProductVariants
            .AsNoTracking()
            .Include(item => item.Product)
            .Include(item => item.Inventory)
            .FirstOrDefaultAsync(item => item.Id == variantId && item.IsActive);

        return variant is null ? NotFound() : Ok(MapVariant(variant));
    }


    private static ProductResponse MapProduct(Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Brand = product.Brand,
            BasePrice = product.BasePrice,
            IsActive = product.IsActive,
            IsInStock = product.Variants.Any(variant =>
                variant.IsActive &&
                variant.Inventory is not null &&
                variant.Inventory.QuantityInStock > 0)
        };
    }

    private static ProductVariantResponse MapVariant(ProductVariant variant)
    {
        var quantityInStock = variant.Inventory?.QuantityInStock ?? 0;

        return new ProductVariantResponse
        {
            Id = variant.Id,
            ProductId = variant.ProductId,
            ProductName = variant.Product?.Name ?? string.Empty,
            Size = variant.Size,
            Color = variant.Color,
            Sku = variant.Sku,
            Price = variant.Price,
            IsActive = variant.IsActive,
            QuantityInStock = quantityInStock,
            IsInStock = quantityInStock > 0
        };
    }
}