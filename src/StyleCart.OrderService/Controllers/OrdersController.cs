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
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly OrderDbContext _dbContext;
    private readonly ProductServiceClient _productServiceClient;

    public OrdersController(
        OrderDbContext dbContext,
        ProductServiceClient productServiceClient)
    {
        _dbContext = dbContext;
        _productServiceClient = productServiceClient;
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<OrderResponse>> Checkout(CheckoutRequest request)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var cart = await _dbContext.Carts
            .Include(item => item.Items)
            .FirstOrDefaultAsync(item => item.UserId == userId);

        if (cart is null || cart.Items.Count == 0)
        {
            return BadRequest(new { message = "Your cart is empty." });
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            return Unauthorized();
        }

        foreach (var cartItem in cart.Items)
        {
            var reserved = await _productServiceClient.ReserveStockAsync(
                cartItem.ProductVariantId,
                cartItem.Quantity,
                authorizationHeader);

            if (!reserved)
            {
                return BadRequest(new
                {
                    message = $"Stock is no longer available for {cartItem.ProductName}."
                });
            }
        }

        var order = new Order
        {
            UserId = userId,
            RecipientName = request.RecipientName.Trim(),
            ShippingAddress = request.ShippingAddress.Trim(),
            TotalAmount = cart.Items.Sum(item => item.UnitPrice * item.Quantity),
            Items = cart.Items.Select(item => new OrderItem
            {
                ProductVariantId = item.ProductVariantId,
                ProductName = item.ProductName,
                VariantDescription = item.VariantDescription,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                LineTotal = item.UnitPrice * item.Quantity
            }).ToList()
        };

        _dbContext.Orders.Add(order);
        _dbContext.CartItems.RemoveRange(cart.Items);

        cart.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(MapOrder(order));
    }

    [HttpGet("my-orders")]
    public async Task<ActionResult<IReadOnlyList<OrderResponse>>> GetMyOrders()
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var orders = await _dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .Where(order => order.UserId == userId)
            .OrderByDescending(order => order.OrderDate)
            .ToListAsync();

        return Ok(orders.Select(MapOrder).ToList());
    }

    private static OrderResponse MapOrder(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            OrderStatus = order.OrderStatus,
            RecipientName = order.RecipientName,
            ShippingAddress = order.ShippingAddress,
            Items = order.Items.Select(item => new OrderItemResponse
            {
                ProductVariantId = item.ProductVariantId,
                ProductName = item.ProductName,
                VariantDescription = item.VariantDescription,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                LineTotal = item.LineTotal
            }).ToList()
        };
    }

    private string? GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}