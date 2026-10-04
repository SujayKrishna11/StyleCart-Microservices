using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StyleCart.OrderService.Contracts;
using StyleCart.OrderService.Data;
using StyleCart.OrderService.Models;
using StyleCart.OrderService.Services;
using System.Security.Claims;

namespace StyleCart.OrderService.Controllers;

[Authorize]
[ApiController]
[Route("api/cart")]
public sealed class CartController : ControllerBase
{
    private readonly OrderDbContext _dbContext;
    private readonly ProductServiceClient _productServiceClient;

    public CartController(
        OrderDbContext dbContext,
        ProductServiceClient productServiceClient)
    {
        _dbContext = dbContext;
        _productServiceClient = productServiceClient;
    }

    [HttpGet]
    public async Task<ActionResult<CartResponse>> GetCart()
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var cart = await GetOrCreateCartAsync(userId);

        return Ok(MapCart(cart));
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartResponse>> AddItem(AddCartItemRequest request)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        ProductVariantInfo? variant;

        try
        {
            variant = await _productServiceClient.GetVariantAsync(
                request.ProductVariantId);
        }
        catch (HttpRequestException)
        {
            return StatusCode(503, new
            {
                message = "Product Service is unavailable. Try again later."
            });
        }

        if (variant is null || !variant.IsActive || !variant.IsInStock)
        {
            return BadRequest(new
            {
                message = "This product variant is unavailable."
            });
        }

        var cart = await GetOrCreateCartAsync(userId);

        var existingItem = cart.Items.FirstOrDefault(item =>
            item.ProductVariantId == request.ProductVariantId);

        if (existingItem is not null)
        {
            if (existingItem.Quantity + request.Quantity > 10)
            {
                return BadRequest(new
                {
                    message = "You can add a maximum of 10 units of one item."
                });
            }

            existingItem.Quantity += request.Quantity;
            existingItem.UnitPrice = variant.Price;
            existingItem.ProductName = variant.ProductName;
            existingItem.VariantDescription = $"{variant.Color} · {variant.Size}";
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                ProductVariantId = variant.Id,
                ProductName = variant.ProductName,
                VariantDescription = $"{variant.Color} · {variant.Size}",
                UnitPrice = variant.Price,
                Quantity = request.Quantity
            });
        }

        cart.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(MapCart(cart));
    }

    [HttpPut("items/{cartItemId:int}")]
    public async Task<ActionResult<CartResponse>> UpdateItem(
        int cartItemId,
        UpdateCartItemRequest request)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var cart = await _dbContext.Carts
            .Include(item => item.Items)
            .FirstOrDefaultAsync(item => item.UserId == userId);

        var cartItem = cart?.Items.FirstOrDefault(item => item.Id == cartItemId);

        if (cartItem is null)
        {
            return NotFound();
        }

        cartItem.Quantity = request.Quantity;
        cart!.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(MapCart(cart));
    }

    [HttpDelete("items/{cartItemId:int}")]
    public async Task<IActionResult> RemoveItem(int cartItemId)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var cart = await _dbContext.Carts
            .Include(item => item.Items)
            .FirstOrDefaultAsync(item => item.UserId == userId);

        var cartItem = cart?.Items.FirstOrDefault(item => item.Id == cartItemId);

        if (cartItem is null)
        {
            return NotFound();
        }

        _dbContext.CartItems.Remove(cartItem);
        cart!.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    private async Task<Cart> GetOrCreateCartAsync(string userId)
    {
        var cart = await _dbContext.Carts
            .Include(item => item.Items)
            .FirstOrDefaultAsync(item => item.UserId == userId);

        if (cart is not null)
        {
            return cart;
        }

        cart = new Cart
        {
            UserId = userId
        };

        _dbContext.Carts.Add(cart);
        await _dbContext.SaveChangesAsync();

        return cart;
    }

    private static CartResponse MapCart(Cart cart)
    {
        return new CartResponse
        {
            Id = cart.Id,
            UpdatedAt = cart.UpdatedAt,
            Items = cart.Items
                .OrderBy(item => item.Id)
                .Select(item => new CartItemResponse
                {
                    Id = item.Id,
                    ProductVariantId = item.ProductVariantId,
                    ProductName = item.ProductName,
                    VariantDescription = item.VariantDescription,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    LineTotal = item.UnitPrice * item.Quantity
                })
                .ToList()
        };
    }

    private string? GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}