using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StyleCart.OrderService.Contracts;
using StyleCart.OrderService.Data;
using System.Security.Claims;

namespace StyleCart.OrderService.Controllers;

[ApiController]
[Route("api/recommendations")]
public sealed class RecommendationsController : ControllerBase
{
    private const int RecommendationLimit = 4;
    private readonly OrderDbContext _dbContext;

    public RecommendationsController(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [Authorize]
    [HttpGet("for-you")]
    public async Task<ActionResult<IReadOnlyList<ProductRecommendationResponse>>> GetForYou()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var purchasedVariantIds = await _dbContext.OrderItems
            .AsNoTracking()
            .Where(item => item.Order.UserId == userId)
            .Select(item => item.ProductVariantId)
            .Distinct()
            .ToListAsync();

        var recommendations = await _dbContext.OrderItems
            .AsNoTracking()
            .Where(item => !purchasedVariantIds.Contains(item.ProductVariantId))
            .GroupBy(item => new
            {
                item.ProductVariantId,
                item.ProductName,
                item.VariantDescription,
                item.UnitPrice
            })
            .OrderByDescending(group => group.Sum(item => item.Quantity))
            .ThenBy(group => group.Key.ProductName)
            .Take(RecommendationLimit)
            .Select(group => new ProductRecommendationResponse
            {
                ProductVariantId = group.Key.ProductVariantId,
                ProductName = group.Key.ProductName,
                VariantDescription = group.Key.VariantDescription,
                UnitPrice = group.Key.UnitPrice,
                PurchaseCount = group.Sum(item => item.Quantity)
            })
            .ToListAsync();

        return Ok(recommendations);
    }

    [HttpGet("variants/{variantId:int}/also-bought")]
    public async Task<ActionResult<IReadOnlyList<ProductRecommendationResponse>>> GetAlsoBought(
        int variantId)
    {
        var matchingOrderIds = _dbContext.OrderItems
            .Where(item => item.ProductVariantId == variantId)
            .Select(item => item.OrderId);

        var recommendations = await _dbContext.OrderItems
            .AsNoTracking()
            .Where(item => matchingOrderIds.Contains(item.OrderId) &&
                           item.ProductVariantId != variantId)
            .GroupBy(item => new
            {
                item.ProductVariantId,
                item.ProductName,
                item.VariantDescription,
                item.UnitPrice
            })
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key.ProductName)
            .Take(RecommendationLimit)
            .Select(group => new ProductRecommendationResponse
            {
                ProductVariantId = group.Key.ProductVariantId,
                ProductName = group.Key.ProductName,
                VariantDescription = group.Key.VariantDescription,
                UnitPrice = group.Key.UnitPrice,
                PurchaseCount = group.Sum(item => item.Quantity)
            })
            .ToListAsync();

        return Ok(recommendations);
    }
}
