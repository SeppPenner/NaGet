namespace NaGet.Azure;

public class AzureSearchIndexer : ISearchIndexer
{
    private readonly IPackageDatabase packages;
    private readonly IndexActionBuilder actionBuilder;
    private readonly AzureSearchBatchIndexer batchIndexer;
    private readonly ILogger<AzureSearchIndexer> logger;

    public AzureSearchIndexer(
        IPackageDatabase packages,
        IndexActionBuilder actionBuilder,
        AzureSearchBatchIndexer batchIndexer,
        ILogger<AzureSearchIndexer> logger)
    {
        this.packages = packages ?? throw new ArgumentNullException(nameof(packages));
        this.actionBuilder = actionBuilder ?? throw new ArgumentNullException(nameof(actionBuilder));
        this.batchIndexer = batchIndexer ?? throw new ArgumentNullException(nameof(batchIndexer));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task IndexAsync(Package package, CancellationToken cancellationToken)
    {
        var packages = await this.packages.FindAsync(package.Id, includeUnlisted: false, cancellationToken);

        var actions = actionBuilder.UpdatePackage(
            new PackageRegistration(
                package.Id,
                packages));

        await batchIndexer.IndexAsync(actions, cancellationToken);
    }
}
