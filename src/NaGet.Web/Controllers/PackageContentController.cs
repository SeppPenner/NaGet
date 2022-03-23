namespace NaGet.Web;

/// <inheritdoc cref="Controller"/>
/// <summary>
/// A controller to upload files for the image recognition app.
/// </summary>
//[Route("api/PackageContent")]
//[ApiController]
//[OpenApiTag("Image recognition", Description = "Image recognition management.")]

/// <summary>
/// The Package Content resource, used to download content from packages.
/// See: https://docs.microsoft.com/en-us/nuget/api/package-base-address-resource
/// </summary>
public class PackageContentController : Controller
{
    /// <summary>
    /// The package content service.
    /// </summary>
    private readonly IPackageContentService packageContentService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PackageContentController"/> class.
    /// </summary>
    /// <param name="packageContentService">The package content service.</param>
    /// <exception cref="ArgumentNullException">Thrown if the package content service is null.</exception>
    public PackageContentController(IPackageContentService packageContentService)
    {
        this.packageContentService = packageContentService ?? throw new ArgumentNullException(nameof(packageContentService));
    }

    /// <summary>
    ///     Uploads the file used for the text recognition from the image.
    /// </summary>
    /// <param name="files">The file used for the text recognition from the image.</param>
    /// <returns>
    ///     A <see cref="List{T}"/> of <see cref="string"/>s.
    /// </returns>
    /// <remarks>
    ///     Uploads the file used for the text recognition from the image.
    /// </remarks>
    /// <response code="200">Text found successfully.</response>
    /// <response code="400">Bad request.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="500">Internal server error.</response>
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    [Authorize]
    [HttpPost("uploadfile")]
    public async Task<ActionResult<PackageVersionsResponse>> GetPackageVersionsAsync(string id, CancellationToken cancellationToken)
    {
        var versions = await packageContentService.GetPackageVersionsOrNullAsync(id, cancellationToken);

        if (versions is null)
        {
            return NotFound();
        }

        return versions;
    }

    public async Task<IActionResult> DownloadPackageAsync(string id, string version, CancellationToken cancellationToken)
    {
        if (!NuGetVersion.TryParse(version, out var nugetVersion))
        {
            return NotFound();
        }

        var packageStream = await packageContentService.GetPackageContentStreamOrNullAsync(id, nugetVersion, cancellationToken);

        if (packageStream is null)
        {
            return NotFound();
        }

        return File(packageStream, "application/octet-stream");
    }

    public async Task<IActionResult> DownloadNuspecAsync(string id, string version, CancellationToken cancellationToken)
    {
        if (!NuGetVersion.TryParse(version, out var nugetVersion))
        {
            return NotFound();
        }

        var nuspecStream = await packageContentService.GetPackageManifestStreamOrNullAsync(id, nugetVersion, cancellationToken);

        if (nuspecStream is null)
        {
            return NotFound();
        }

        return File(nuspecStream, "text/xml");
    }

    public async Task<IActionResult> DownloadReadmeAsync(string id, string version, CancellationToken cancellationToken)
    {
        if (!NuGetVersion.TryParse(version, out var nugetVersion))
        {
            return NotFound();
        }

        var readmeStream = await packageContentService.GetPackageReadmeStreamOrNullAsync(id, nugetVersion, cancellationToken);

        if (readmeStream is null)
        {
            return NotFound();
        }

        return File(readmeStream, "text/markdown");
    }

    public async Task<IActionResult> DownloadIconAsync(string id, string version, CancellationToken cancellationToken)
    {
        if (!NuGetVersion.TryParse(version, out var nugetVersion))
        {
            return NotFound();
        }

        var iconStream = await packageContentService.GetPackageIconStreamOrNullAsync(id, nugetVersion, cancellationToken);

        if (iconStream is null)
        {
            return NotFound();
        }

        return File(iconStream, "image/xyz");
    }
}
