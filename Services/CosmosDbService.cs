namespace ChatApp.Services;

public interface ICosmosDbService
{
    Task SaveSessionAsync(ChatSession session);
    Task<ChatSession> GetSessionAsync(string userId, string sessionId);
    Task<List<ChatSession>> GetSessionsForUserAsync(string userId);
    Task DeleteSessionAsync(string userId, string sessionId);
}

public class CosmosDbService : ICosmosDbService
{
    private readonly Container _container;

    public CosmosDbService(CosmosClient cosmosClient, IConfiguration configuration)
    {
        var databaseName = configuration["CosmosDb:DatabaseName"] ??
                           throw new ArgumentNullException("DatabaseName is not configured.");
        var containerName = configuration["CosmosDb:ContainerName"] ??
                            throw new ArgumentNullException("ContainerName is not configured.");

        var databaseResponse = cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName).Result;
        var database = databaseResponse.Database;

        var containerProperties = new ContainerProperties(containerName, partitionKeyPath: "/userId");
        var containerResponse = database.CreateContainerIfNotExistsAsync(containerProperties).Result;
        _container = containerResponse.Container;
    }
    // private readonly Container _container;
    // private readonly string _connectionString;
    // private readonly string _databaseName;
    // private readonly string _containerName;

    // private readonly IKeyVaultService _keyVaultService;

    // public CosmosDbService(IKeyVaultService keyVaultService, IConfiguration configuration)
    // {
    //     _keyVaultService = keyVaultService;
    //     _connectionString = _keyVaultService.GetSecretAsync(
    //         configuration["Azure:CosmosDbConnectionString"] ??
    //         throw new ArgumentNullException("CosmosDbConnectionString is not configured.")).Result;

    //     var cosmosClient = new CosmosClient(_connectionString);

    //     _databaseName =
    //         configuration["CosmosDb:DatabaseName"] ??
    //         throw new ArgumentNullException("DatabaseName is not configured.");
    //     _containerName =
    //         configuration["CosmosDb:ContainerName"] ??
    //         throw new ArgumentNullException("ContainerName is not configured.");

    //     try
    //     {
    //         var databaseResponse =
    //             cosmosClient.CreateDatabaseIfNotExistsAsync(_databaseName).Result;
    //         var database = databaseResponse.Database;

    //         var containerProperties = new ContainerProperties(
    //             _containerName, partitionKeyPath: "/userId");
    //         var containerResponse =
    //             database.CreateContainerIfNotExistsAsync(containerProperties).Result;
    //         _container = containerResponse.Container;
    //     }
    //     catch (CosmosException ex)
    //     {
    //         throw new CosmosException(
    //             $"CosmosException during database or container creation: {ex.Message}",
    //             ex.StatusCode,
    //             ex.SubStatusCode,
    //             ex.ActivityId,
    //             ex.RequestCharge);
    //     }
    //     catch (Exception ex)
    //     {
    //         throw new Exception(
    //             $"Exception during database or container creation: {ex.Message}", ex);
    //     }
    // }

    public async Task SaveSessionAsync(ChatSession session)
    {
        try
        {

            if (string.IsNullOrEmpty(session.sessionId))
            {
                throw new ArgumentException("sessionId cannot be null or empty");
            }
            if (string.IsNullOrEmpty(session.UserId))
            {
                throw new ArgumentException("UserId cannot be null or empty");
            }

            if (session.Messages != null && session.Messages.Count > 0)
            {
                var firstUserMessage =
                    session.Messages.FirstOrDefault(m => m.IsUserMessage)?.Text;
                if (!string.IsNullOrEmpty(firstUserMessage))
                {
                    var words = firstUserMessage.Split(
                        ' ', StringSplitOptions.RemoveEmptyEntries);
                    session.SessionName =
                        string.Join(" ", words.Take(2)) + (words.Length > 2 ? "..." : "");
                }
            }

            session.LastUpdated = DateTime.UtcNow.ToString("o");

            var response = await _container.UpsertItemAsync(
                item: session, partitionKey: new PartitionKey(session.UserId));
        }
        catch (CosmosException ex)
        {
            throw new CosmosException(
                $"Cosmos DB Error saving session: {ex.Message}",
                ex.StatusCode,
                ex.SubStatusCode,
                ex.ActivityId,
                ex.RequestCharge);
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"Error saving session: {ex.Message}", ex);
        }
    }

    public async Task<ChatSession> GetSessionAsync(string userId,
                                                   string sessionId)
    {
        try
        {
            var response = await _container.ReadItemAsync<ChatSession>(
                id: sessionId, partitionKey: new PartitionKey(userId));
            return response.Resource;
        }
        catch (CosmosException ex)
            when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"Error retrieving session. UserId: {userId}, SessionId: {sessionId}, Error: {ex.Message}", ex);
        }
    }

    public async Task<List<ChatSession>> GetSessionsForUserAsync(string userId)
    {
        try
        {
            var query =
                new QueryDefinition("SELECT * FROM c WHERE c.userId = @userId")
                    .WithParameter("@userId", userId);

            var resultSet = _container.GetItemQueryIterator<ChatSession>(query);
            var sessions = new List<ChatSession>();

            while (resultSet.HasMoreResults)
            {
                var response = await resultSet.ReadNextAsync();
                sessions.AddRange(response);
            }
            return sessions;
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"Error retrieving sessions for user {userId}: {ex.Message}", ex);
        }
    }

    public async Task DeleteSessionAsync(string userId, string sessionId)
    {
        try
        {
            var response = await _container.DeleteItemAsync<ChatSession>(
                sessionId, new PartitionKey(userId));
        }
        catch (CosmosException ex)
            when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new InvalidOperationException("Session not found.");
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"Error deleting session: {ex.Message}", ex);
        }
    }
}
