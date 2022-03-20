namespace NaGet.Core;

public interface IAuthenticationService
{
    Task<bool> Authenticate(string apiKey, CancellationToken cancellationToken);
}
