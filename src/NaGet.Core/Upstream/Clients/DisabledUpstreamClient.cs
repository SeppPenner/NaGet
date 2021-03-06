namespace NaGet.Core;

/// <summary>
/// The client used when there are no upstream package sources.
/// </summary>
public class DisabledUpstreamClient : IUpstreamClient
{
    private readonly IReadOnlyList<NuGetVersion> emptyVersionList = new List<NuGetVersion>();
    private readonly IReadOnlyList<Package> emptyPackageList = new List<Package>();

    public Task<IReadOnlyList<NuGetVersion>> ListPackageVersionsAsync(string id, CancellationToken cancellationToken)
    {
        return Task.FromResult(emptyVersionList);
    }

    public Task<IReadOnlyList<Package>> ListPackagesAsync(string id, CancellationToken cancellationToken)
    {
        return Task.FromResult(emptyPackageList);
    }

    public Task<Stream?> DownloadPackageOrNullAsync(
        string id,
        NuGetVersion version,
        CancellationToken cancellationToken)
    {
        return Task.FromResult<Stream?>(null);
    }
}
