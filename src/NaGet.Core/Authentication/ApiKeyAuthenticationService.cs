namespace NaGet.Core;

public class ApiKeyAuthenticationService : IAuthenticationService
{
    private readonly string apiKey;

    public ApiKeyAuthenticationService(IOptionsSnapshot<NaGetOptions> options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        apiKey = string.IsNullOrWhiteSpace(options.Value.ApiKey) ? string.Empty : options.Value.ApiKey;
    }

    public Task<bool> AuthenticateAsync(string apiKey, CancellationToken cancellationToken)
        => Task.FromResult(Authenticate(apiKey));

    private bool Authenticate(string apiKey)
    {
        // No authentication is necessary if there is no required API key.
        if (this.apiKey is null)
        {
            return true;
        }

        return this.apiKey == apiKey;
    }
}
