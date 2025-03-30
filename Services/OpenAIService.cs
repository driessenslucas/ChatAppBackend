namespace ChatApp.Services;

public interface IOpenAIService
{
    string GetChatResponse(string userMessage);

    string GetChatWithHistoryResponse(string userMessage,
                                                 ChatSession session);
}

public class OpenAIService : IOpenAIService
{
    private readonly string _endpoint;
    private readonly string _apiKey;
    private readonly IKeyVaultService _keyVaultService;

    public OpenAIService(IKeyVaultService keyVaultService, IConfiguration configuration)
    {
      
        _keyVaultService = keyVaultService;
        _endpoint = configuration["Azure:OpenAIEndpoint"] ?? throw new ArgumentNullException(nameof(configuration));
        _apiKey = Task.Run(async () => await _keyVaultService.GetSecretAsync(configuration["Azure:OpenAIKeySecretName"] ?? throw new ArgumentNullException(nameof(configuration)))).Result;
    }


    public string GetChatResponse(string userMessage)
    {
        try
        {
            AzureOpenAIClient client =
                new(new Uri(_endpoint), new AzureKeyCredential(_apiKey));
            var chatClient = client.GetChatClient("gpt-35-turbo");
            var completionUpdates = chatClient.CompleteChatStreaming(
                new SystemChatMessage("You are a helpful assistant."),
                new UserChatMessage(userMessage));

            string chatMessageContent = "";

            foreach (var update in completionUpdates)
            {
                foreach (var contentPart in update.ContentUpdate)
                {
                    chatMessageContent += contentPart.Text;
                }
            }

            return chatMessageContent;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get chat response: {ex.Message}");
        }
    }

    public string GetChatWithHistoryResponse(string userMessage, ChatSession session)
    {
        try
        {
            AzureOpenAIClient client = new(new Uri(_endpoint), new AzureKeyCredential(_apiKey));
            var chatClient = client.GetChatClient("gpt-35-turbo");

            var chatHistory = new List<OpenAIChatMessage>();

            if (!session.Messages.Any())
            {
                chatHistory.Add(new SystemChatMessage("You are a helpful assistant."));
            }

            chatHistory.AddRange(
                session.Messages.Select(m => m.IsUserMessage
                    ? (OpenAIChatMessage)new UserChatMessage(m.Text)
                    : new AssistantChatMessage(m.Text))
            );

            chatHistory.Add(new UserChatMessage(userMessage));

            var completionUpdates = chatClient.CompleteChatStreaming(chatHistory);

            StringBuilder chatMessageContent = new();

            foreach (var update in completionUpdates)
            {
                foreach (var contentPart in update.ContentUpdate)
                {
                    chatMessageContent.Append(contentPart.Text);
                }
            }

            return chatMessageContent.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get chat response: {ex.Message}");

        }
    }
}
