using Microsoft.AspNetCore.Mvc;
using StyleCart.ApiGateway.Controllers;

namespace StyleCart.ApiGateway.Tests;

public sealed class SupportControllerTests
{
    private readonly SupportController _controller = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Chat_WithBlankMessage_ReturnsBadRequest(string? message)
    {
        var result = _controller.Chat(new SupportChatRequest { Message = message });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Theory]
    [InlineData("When will my order ship?", "Orders are processed")]
    [InlineData("How do I return this item?", "returns or refunds")]
    [InlineData("What is my order status?", "My Orders")]
    public void Chat_WithSupportedQuestion_ReturnsRelevantFaqReply(
        string message,
        string expectedText)
    {
        var result = _controller.Chat(new SupportChatRequest { Message = message });

        var response = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<SupportChatResponse>(response.Value);

        Assert.Contains(expectedText, payload.Reply);
        Assert.Equal("FAQ fallback", payload.Mode);
    }

    [Fact]
    public void Chat_WithUnknownQuestion_ReturnsSafeFallback()
    {
        var result = _controller.Chat(new SupportChatRequest
        {
            Message = "Tell me about your loyalty programme"
        });

        var response = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<SupportChatResponse>(response.Value);

        Assert.Contains("shipping, returns, refunds, and order status", payload.Reply);
    }
}
