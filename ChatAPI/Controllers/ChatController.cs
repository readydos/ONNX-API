using Microsoft.AspNetCore.Authorization;
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

    private readonly SemaphoreSlim _semaphore = new(1, 1);

    [HttpPost]
    [ApiKey]
    [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChatResponse>> Chat(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken) {
        await _semaphore.WaitAsync();

        try {
            var reply = await _chatService.ChatAsync(request.Message, cancellationToken);
        } finally {
            _semaphore.Release();
        }

        return Ok(new ChatResponse {
            Reply = reply
        });
    }
}