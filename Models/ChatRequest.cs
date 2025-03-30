namespace ChatApp.Models;

public class ChatRequest
{
    public string? SessionId { get; set; } // Optional session ID
    public string? UserId { get; set; }
    public required string UserMessage { get; set; }
    public bool IncludeHistory { get; set; } =
        false; // Flag to include chat history
}
