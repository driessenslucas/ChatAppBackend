namespace ChatApp.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly ICosmosDbService _cosmosDbService;
    private readonly IKeyVaultService _keyVaultService;

    public ChatController(IChatService chatService,
                          ICosmosDbService cosmosDbService,
                          IKeyVaultService keyVaultService)

    {
        _chatService = chatService;
        _cosmosDbService = cosmosDbService;
        _keyVaultService = keyVaultService;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User not authenticated.");

        request.UserId = userId;

        try
        {
            var response = await _chatService.ProcessChatRequest(request);
            var chatSession =
                await _chatService.CreateOrUpdateChatSession(request, response);
            await _cosmosDbService.SaveSessionAsync(chatSession);

            return Ok(new { data = new { response, chatSession.sessionId } });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in Chat endpoint: {ex.Message}");
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetChatHistory()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User not authenticated.");

        try
        {
            var sessions = await _cosmosDbService.GetSessionsForUserAsync(userId);
            return Ok(new { data = sessions });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error retrieving chat history: {ex.Message}");
            return StatusCode(500,
                              "An error occurred while retrieving chat history.");
        }
    }

    [HttpGet("sessions/{sessionId}")]
    public async Task<IActionResult> GetSession(string sessionId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(sessionId))
            return BadRequest("User ID and Session ID are required.");

        try
        {
            var session = await _cosmosDbService.GetSessionAsync(userId, sessionId);
            if (session == null)
                return NotFound("Session not found.");
            return Ok(new { data = session });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error retrieving session: {ex.Message}");
            return StatusCode(500, "An error occurred while retrieving the session.");
        }
    }

    [HttpGet("userProfile")]
    public IActionResult GetUserProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User not authenticated.");

        return Ok(new { data = new { userId } });
    }
}
