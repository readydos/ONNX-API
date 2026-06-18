using Microsoft.AspNetCore.Mvc;
using OnnxChatApi.Models;
using OnnxChatApi.Services;

namespace OnnxChatApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ChatController : ControllerBase {
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService) {
        _chatService = chatService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChatResponse>> Chat(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken) {
        var reply = await _chatService.ChatAsync(request.Message, cancellationToken);

        return Ok(new ChatResponse {
            Reply = reply
        });
    }
}