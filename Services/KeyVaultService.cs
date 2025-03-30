namespace ChatApp.Services;

public interface IKeyVaultService
{
    Task<string> GetSecretAsync(string secretName);
}

public class KeyVaultService : IKeyVaultService
{
    private readonly SecretClient _secretClient;

    public KeyVaultService(string keyVaultUri)
    {
        var credential = new AzureCliCredential();
        _secretClient = new SecretClient(new Uri(keyVaultUri), credential);
    }

    public async Task<string> GetSecretAsync(string secretName)
    {
        try
        {
            var secret = await _secretClient.GetSecretAsync(secretName).ConfigureAwait(false);

            if (string.IsNullOrEmpty(secret.Value?.Value))
                throw new InvalidOperationException($"Secret '{secretName}' not found or is empty in KeyVault.");

            return secret.Value.Value;
        }
        catch (RequestFailedException ex)
        {
            Console.WriteLine($"Azure Key Vault request failed: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error retrieving secret: {ex.Message}");
            throw;
        }
    }
}

