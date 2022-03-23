namespace NaGet.Web;

/// <summary>
/// The Package Metadata resource, used to fetch packages' information.
/// See: https://docs.microsoft.com/en-us/nuget/api/registration-base-url-resource
/// </summary>
public class PackageMetadataController : Controller
{
    private readonly IPackageMetadataService packageMetadataService;

    public PackageMetadataController(IPackageMetadataService packageMetadataService)
    {
        this.packageMetadataService = packageMetadataService ?? throw new ArgumentNullException(nameof(packageMetadataService));
    }

    // GET v3/registration/{id}.json
    [HttpGet]
    public async Task<ActionResult<NaGetRegistrationIndexResponse>> RegistrationIndexAsync(string id, CancellationToken cancellationToken)
    {
        var index = await packageMetadataService.GetRegistrationIndexOrNullAsync(id, cancellationToken);

        if (index is null)
        {
            return NotFound();
        }

        return index;
    }

    // GET v3/registration/{id}/{version}.json
    [HttpGet]
    public async Task<ActionResult<RegistrationLeafResponse>> RegistrationLeafAsync(string id, string version, CancellationToken cancellationToken)
    {
        if (!NuGetVersion.TryParse(version, out var nugetVersion))
        {
            return NotFound();
        }

        var leaf = await packageMetadataService.GetRegistrationLeafOrNullAsync(id, nugetVersion, cancellationToken);

        if (leaf is null)
        {
            return NotFound();
        }

        return leaf;
    }
}
