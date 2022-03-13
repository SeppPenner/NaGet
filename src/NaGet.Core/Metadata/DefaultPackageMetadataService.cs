namespace NaGet.Core;

/// <inheritdoc />
public class DefaultPackageMetadataService : IPackageMetadataService
{
    private readonly IPackageService packages;
    private readonly RegistrationBuilder builder;

    public DefaultPackageMetadataService(
        IPackageService packages,
        RegistrationBuilder builder)
    {
        this.packages = packages ?? throw new ArgumentNullException(nameof(packages));
        this.builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    public async Task<NaGetRegistrationIndexResponse> GetRegistrationIndexOrNullAsync(
        string packageId,
        CancellationToken cancellationToken = default)
    {
        var packages = await this.packages.FindPackagesAsync(packageId, cancellationToken);

        if (!packages.Any())
        {
            return null;
        }

        return builder.BuildIndex(
            new PackageRegistration(
                packageId,
                packages));
    }

    public async Task<RegistrationLeafResponse> GetRegistrationLeafOrNullAsync(
        string id,
        NuGetVersion version,
        CancellationToken cancellationToken = default)
    {
        var package = await packages.FindPackageOrNullAsync(id, version, cancellationToken);
        if (package == null)
        {
            return null;
        }

        return builder.BuildLeaf(package);
    }
}
