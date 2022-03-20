// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AliyunStorageOptions.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The Azure search indexer class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Azure.Search;

/// <summary>
/// The Azure search indexer class.
/// </summary>
public class AzureSearchIndexer : ISearchIndexer
{
    /// <summary>
    /// The package database.
    /// </summary>
    private readonly IPackageDatabase packages;

    /// <summary>
    /// The index action builder.
    /// </summary>
    private readonly IndexActionBuilder indexAxtionBuilder;

    /// <summary>
    /// The batch indexer.
    /// </summary>
    private readonly AzureSearchBatchIndexer batchIndexer;

    /// <summary>
    /// The logger.
    /// </summary>
    private readonly ILogger<AzureSearchIndexer> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureSearchIndexer"/> class.
    /// </summary>
    /// <param name="packages">The packages.</param>
    /// <param name="indexActionBuilder">The index action builder.</param>
    /// <param name="batchIndexer">The batch indexer.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters is null.</exception>
    public AzureSearchIndexer(
        IPackageDatabase packages,
        IndexActionBuilder indexActionBuilder,
        AzureSearchBatchIndexer batchIndexer,
        ILogger<AzureSearchIndexer> logger)
    {
        this.packages = packages ?? throw new ArgumentNullException(nameof(packages));
        this.indexAxtionBuilder = indexActionBuilder ?? throw new ArgumentNullException(nameof(indexActionBuilder));
        this.batchIndexer = batchIndexer ?? throw new ArgumentNullException(nameof(batchIndexer));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc cref="ISearchIndexer"/>
    public async Task Index(Package package, CancellationToken cancellationToken)
    {
        var packages = await this.packages.Find(package.Id, includeUnlisted: false, cancellationToken);

        var actions = indexAxtionBuilder.UpdatePackage(
            new PackageRegistration(
                package.Id,
                packages));

        await batchIndexer.Index(actions, cancellationToken);
    }
}
