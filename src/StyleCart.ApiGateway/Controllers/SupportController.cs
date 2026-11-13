using Microsoft.AspNetCore.Mvc;

namespace StyleCart.ApiGateway.Controllers;

[ApiController]
[Route("api/support")]
public sealed class SupportController : ControllerBase
{
    [HttpPost("chat")]
    public ActionResult<SupportChatResponse> Chat(SupportChatRequest request)
    {
        var message = request.Message?.Trim();

        if (string.IsNullOrWhiteSpace(message))
        {
            return BadRequest(new { message = "Please enter a support question." });
        }

        if (message.Length > 500)
        {
            return BadRequest(new { message = "Support questions must be 500 characters or fewer." });
        }

        var normalizedMessage = message.ToLowerInvariant();
        var reply = normalizedMessage.Contains("ship") || normalizedMessage.Contains("deliver")
            ? "Orders are processed after checkout. You can check your order history for the latest order status."
            : normalizedMessage.Contains("return") || normalizedMessage.Contains("refund")
                ? "For returns or refunds, contact StyleCart support with your order number and the reason for the request."
                : normalizedMessage.Contains("order") || normalizedMessage.Contains("status")
                    ? "Open My Orders to view your placed orders and their current status."
                    : "I can help with shipping, returns, refunds, and order status. Please ask one of those questions.";

        return Ok(new SupportChatResponse
        {
            Reply = reply,
            Mode = "FAQ fallback"
        });
    }
}

public sealed class SupportChatRequest
{
    public string? Message { get; set; }
}

public sealed class SupportChatResponse
{
    public string Reply { get; init; } = string.Empty;

    public string Mode { get; init; } = string.Empty;
}
