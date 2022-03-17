namespace NaGet.Azure;

using StorageException = Microsoft.Azure.Cosmos.Table.StorageException;

/// <summary>
/// Stores the metadata of packages using Azure Table Storage.
/// </summary>
public class TablePackageDatabase : IPackageDatabase
{
    private const string TableName = "Packages";
    private const int MaxPreconditionFailures = 5;

    private readonly TableOperationBuilder operationBuilder;
    private readonly CloudTable table;
    private readonly ILogger<TablePackageDatabase> logger;

    public TablePackageDatabase(
        TableOperationBuilder operationBuilder,
        CloudTableClient client,
        ILogger<TablePackageDatabase> logger)
    {
        this.operationBuilder = operationBuilder ?? throw new ArgumentNullException(nameof(operationBuilder));
        table = client?.GetTableReference(TableName) ?? throw new ArgumentNullException(nameof(client));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PackageAddResult> AddAsync(Package package, CancellationToken cancellationToken)
    {
        try
        {
            var operation = operationBuilder.AddPackage(package);

            await table.ExecuteAsync(operation, cancellationToken);
        }
        catch (Microsoft.Azure.Cosmos.Table.StorageException e) when (e.IsAlreadyExistsException())
        {
            return PackageAddResult.PackageAlreadyExists;
        }

        return PackageAddResult.Success;
    }

    public async Task AddDownloadAsync(
        string id,
        NuGetVersion version,
        CancellationToken cancellationToken)
    {
        var attempt = 0;

        while (true)
        {
            try
            {
                var operation = TableOperation.Retrieve<PackageDownloadsEntity>(
                    id.ToLowerInvariant(),
                    version.ToNormalizedString().ToLowerInvariant());

                var result = await table.ExecuteAsync(operation, cancellationToken);
                var entity = result.Result as PackageDownloadsEntity;

                if (entity is null)
                {
                    return;
                }

                entity.Downloads += 1;

                await table.ExecuteAsync(TableOperation.Merge(entity), cancellationToken);
                return;
            }
            catch (StorageException e)
                when (attempt < MaxPreconditionFailures && e.IsPreconditionFailedException())
            {
                attempt++;
                logger.LogWarning(
                    e,
                    $"Retrying due to precondition failure, attempt {{Attempt}} of {MaxPreconditionFailures}..",
                    attempt);
            }
        }
    }

    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken)
    {
        var filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, id.ToLowerInvariant());
        var query = new TableQuery<PackageEntity>()
            .Select(MinimalColumnSet)
            .Where(filter)
            .Take(1);

        var result = await table.ExecuteQuerySegmentedAsync(query, token: null, cancellationToken);

        return result.Results.Any();
    }

    public async Task<bool> ExistsAsync(
        string id,
        NuGetVersion version,
        CancellationToken cancellationToken)
    {
        var operation = TableOperation.Retrieve<PackageEntity>(
            id.ToLowerInvariant(),
            version.ToNormalizedString().ToLowerInvariant(),
            MinimalColumnSet);

        var execution = await table.ExecuteAsync(operation, cancellationToken);

        return execution.Result is PackageEntity;
    }

    public async Task<IReadOnlyList<Package>> FindAsync(string id, bool includeUnlisted, CancellationToken cancellationToken)
    {
        var filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, id.ToLowerInvariant());
        if (!includeUnlisted)
        {
            filter = TableQuery.CombineFilters(
                filter,
                TableOperators.And,
                TableQuery.GenerateFilterConditionForBool(nameof(PackageEntity.Listed), QueryComparisons.Equal, true));
        }

        var query = new TableQuery<PackageEntity>().Where(filter);
        var results = new List<Package>();

        // Request 500 results at a time from the server.
        TableContinuationToken token = null;
        query.TakeCount = 500;

        do
        {
            var segment = await table.ExecuteQuerySegmentedAsync(query, token, cancellationToken);

            token = segment.ContinuationToken;

            // Write out the properties for each entity returned.
            results.AddRange(segment.Results.Select(r => r.AsPackage()));
        }
        while (token != null);

        return results.OrderBy(p => p.Version).ToList();
    }

    public async Task<Package> FindOrNullAsync(
        string id,
        NuGetVersion version,
        bool includeUnlisted,
        CancellationToken cancellationToken)
    {
        var operation = TableOperation.Retrieve<PackageEntity>(
            id.ToLowerInvariant(),
            version.ToNormalizedString().ToLowerInvariant());

        var result = await table.ExecuteAsync(operation, cancellationToken);
        var entity = result.Result as PackageEntity;

        if (entity is null)
        {
            return null;
        }

        // Filter out the package if it's unlisted.
        if (!includeUnlisted && !entity.Listed)
        {
            return null;
        }

        return entity.AsPackage();
    }

    public async Task<bool> HardDeletePackageAsync(string id, NuGetVersion version, CancellationToken cancellationToken)
    {
        return await TryUpdatePackageAsync(
            operationBuilder.HardDeletePackage(id, version),
            cancellationToken);
    }

    public async Task<bool> RelistPackageAsync(string id, NuGetVersion version, CancellationToken cancellationToken)
    {
        return await TryUpdatePackageAsync(
            operationBuilder.RelistPackage(id, version),
            cancellationToken);
    }

    public async Task<bool> UnlistPackageAsync(string id, NuGetVersion version, CancellationToken cancellationToken)
    {
        return await TryUpdatePackageAsync(
            operationBuilder.UnlistPackage(id, version),
            cancellationToken);
    }

    private List<string> MinimalColumnSet => new List<string> { "PartitionKey" };

    private async Task<bool> TryUpdatePackageAsync(TableOperation operation, CancellationToken cancellationToken)
    {
        try
        {
            await table.ExecuteAsync(operation, cancellationToken);
        }
        catch (StorageException e) when (e.IsNotFoundException())
        {
            return false;
        }

        return true;
    }
}
