namespace ChatApp.Services;
public interface IChatService
{
    Task<string> ProcessChatRequest(ChatRequest request);
    Task<ChatSession> CreateOrUpdateChatSession(ChatRequest request,
                                                string response);
}
public class ChatService : IChatService
{
    private readonly IOpenAIService _openAIService;
    private readonly ICosmosDbService _cosmosDbService;

    public ChatService(IOpenAIService openAIService,
                       ICosmosDbService cosmosDbService)
    {
        _openAIService = openAIService;
        _cosmosDbService = cosmosDbService;
    }

    public async Task<string> ProcessChatRequest(ChatRequest request)
    {
        if (string.IsNullOrEmpty(request.SessionId))
        {
            return  _openAIService.GetChatResponse(request.UserMessage);
        }

        var session = await _cosmosDbService.GetSessionAsync(request.UserId,
                                                             request.SessionId);
                                                             
        return _openAIService.GetChatWithHistoryResponse(
            request.UserMessage, session);
    }

    private Message CreateChatMessage(string text, bool isUserMessage)
    {
        return new Message
        {
            MessageId = Guid.NewGuid().ToString(),
            Text = text,
            IsUserMessage = isUserMessage,
            Timestamp = DateTime.UtcNow.ToString("o")
        };
    }

    public async Task<ChatSession> CreateOrUpdateChatSession(ChatRequest request,
                                                             string response)
    {
        if (!string.IsNullOrEmpty(request.SessionId))
        {
            var session = await _cosmosDbService.GetSessionAsync(request.UserId,
                                                                 request.SessionId);
            if (session == null)
                throw new Exception("Session not found.");

            session.Messages.Add(CreateChatMessage(request.UserMessage, true));
            session.Messages.Add(CreateChatMessage(response, false));
            return session;
        }

        return new ChatSession
        {
            sessionId = Guid.NewGuid().ToString(),
            UserId = request.UserId,
            SessionName = "Default Session",
            Messages = new List<Message>
                {
                    CreateChatMessage(request.UserMessage, true),
                    CreateChatMessage(response, false)
                },
            LastUpdated = DateTime.UtcNow.ToString("o")
        };
    }
}
