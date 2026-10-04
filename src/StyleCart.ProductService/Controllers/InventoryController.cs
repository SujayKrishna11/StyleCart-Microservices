using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StyleCart.ProductService.Contracts;
using StyleCart.ProductService.Data;

namespace StyleCart.ProductService.Controllers;

[ApiController]
[Route("api/inventory")]
public sealed class InventoryController : ControllerBase
{
    private readonly ProductDbContext _dbContext;

    public InventoryController(ProductDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("{variantId:int}")]
    public async Task<ActionResult<InventoryResponse>> GetByVariantId(int variantId)
    {
        var inventory = await _dbContext.Inventories
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.ProductVariantId == variantId);

        return inventory is null ? NotFound() : Ok(MapInventory(inventory));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{variantId:int}")]
    public async Task<ActionResult<InventoryResponse>> Update(
        int variantId,
        UpdateInventoryRequest request)
    {
        var inventory = await _dbContext.Inventories
            .FirstOrDefaultAsync(item => item.ProductVariantId == variantId);

        if (inventory is null)
        {
            return NotFound();
        }

        inventory.QuantityInStock = request.QuantityInStock;
        inventory.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(MapInventory(inventory));
    }

    [Authorize]
    [HttpPost("{variantId:int}/reserve")]
    public async Task<ActionResult<InventoryResponse>> Reserve(
        int variantId,
        ReserveInventoryRequest request)
    {
        var inventory = await _dbContext.Inventories
            .Include(item => item.ProductVariant)
            .FirstOrDefaultAsync(item => item.ProductVariantId == variantId);

        if (inventory is null || !inventory.ProductVariant.IsActive)
        {
            return NotFound(new { message = "Product variant was not found." });
        }

        if (inventory.QuantityInStock < request.Quantity)
        {
            return BadRequest(new { message = "Insufficient stock." });
        }

        inventory.QuantityInStock -= request.Quantity;
        inventory.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(MapInventory(inventory));
    }

    private static InventoryResponse MapInventory(Models.Inventory inventory)
    {
        return new InventoryResponse
        {
            ProductVariantId = inventory.ProductVariantId,
            QuantityInStock = inventory.QuantityInStock,
            UpdatedAt = inventory.UpdatedAt
        };
    }
}