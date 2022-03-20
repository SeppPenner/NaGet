// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AliyunStorageOptions.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The Azure table package database class.
//     Stores the metadata of packages using Azure Table Storage.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Azure.Table;

using StorageException = Microsoft.Azure.Cosmos.Table.StorageException;

/// <inheritdoc cref="IPackageDatabase"/>
/// <summary>
/// The Azure table package database class.
/// Stores the metadata of packages using Azure Table Storage.
/// </summary>
public class TablePackageDatabase : IPackageDatabase
{
    /// <summary>
    /// The table name.
    /// </summary>
    private const string TableName = "Packages";

    /// <summary>
    /// The maximum pre-condition failures.
    /// </summary>
    private const int MaxPreconditionFailures = 5;

    /// <summary>
    /// The operation builder.
    /// </summary>
    private readonly TableOperationBuilder operationBuilder;

    /// <summary>
    /// The cloud table.
    /// </summary>
    private readonly CloudTable table;

    /// <summary>
    /// The logger.
    /// </summary>
    private readonly ILogger<TablePackageDatabase> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TablePackageDatabase"/> class.
    /// </summary>
    /// <param name="operationBuilder">The operation builder.</param>
    /// <param name="client">The client.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters is null.</exception>
    public TablePackageDatabase(
        TableOperationBuilder operationBuilder,
        CloudTableClient client,
        ILogger<TablePackageDatabase> logger)
    {
        this.operationBuilder = operationBuilder ?? throw new ArgumentNullException(nameof(operationBuilder));
        table = client?.GetTableReference(TableName) ?? throw new ArgumentNullException(nameof(client));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc cref="IPackageDatabase"/>
    public async Task<PackageAddResult> Add(Package package, CancellationToken cancellationToken)
    {
        try
        {
            var operation = operationBuilder.AddPackage(package);
            await table.ExecuteAsync(operation, cancellationToken);
        }
        catch (StorageException e) when (e.IsAlreadyExistsException())
        {
            return PackageAddResult.PackageAlreadyExists;
        }

        return PackageAddResult.Success;
    }

    /// <inheritdoc cref="IPackageDatabase"/>
    public async Task AddDownload(
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

                if (result.Result is not PackageDownloadsEntity entity)
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

    /// <inheritdoc cref="IPackageDatabase"/>
    public async Task<bool> Exists(string id, CancellationToken cancellationToken)
    {
        var filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, id.ToLowerInvariant());
        var query = new TableQuery<PackageEntity>()
            .Select(MinimalColumnSet)
            .Where(filter)
            .Take(1);

        var result = await table.ExecuteQuerySegmentedAsync(query, token: null, cancellationToken);

        return result.Results.Any();
    }

    /// <inheritdoc cref="IPackageDatabase"/>
    public async Task<bool> Exists(
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

    /// <inheritdoc cref="IPackageDatabase"/>
    public async Task<IReadOnlyList<Package>> Find(string id, bool includeUnlisted, CancellationToken cancellationToken)
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
        TableContinuationToken? token = null;
        query.TakeCount = 500;

        do
        {
            var segment = await table.ExecuteQuerySegmentedAsync(query, token, cancellationToken);

            token = segment.ContinuationToken;

            // Write out the properties for each entity returned.
            results.AddRange(segment.Results.Select(r => r.AsPackage()));
        }
        while (token is not null);

        return results.OrderBy(p => p.Version).ToList();
    }

    /// <inheritdoc cref="IPackageDatabase"/>
    public async Task<Package?> FindOrNull(
        string id,
        NuGetVersion version,
        bool includeUnlisted,
        CancellationToken cancellationToken)
    {
        var operation = TableOperation.Retrieve<PackageEntity>(
            id.ToLowerInvariant(),
            version.ToNormalizedString().ToLowerInvariant());

        var result = await table.ExecuteAsync(operation, cancellationToken);

        if (result.Result is not PackageEntity entity)
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

    /// <inheritdoc cref="IPackageDatabase"/>
    public async Task<bool> HardDeletePackage(string id, NuGetVersion version, CancellationToken cancellationToken)
    {
        return await TryUpdatePackage(
            operationBuilder.HardDeletePackage(id, version),
            cancellationToken);
    }

    /// <inheritdoc cref="IPackageDatabase"/>
    public async Task<bool> RelistPackage(string id, NuGetVersion version, CancellationToken cancellationToken)
    {
        return await TryUpdatePackage(
            operationBuilder.RelistPackage(id, version),
            cancellationToken);
    }

    /// <inheritdoc cref="IPackageDatabase"/>
    public async Task<bool> UnlistPackage(string id, NuGetVersion version, CancellationToken cancellationToken)
    {
        return await TryUpdatePackage(
            operationBuilder.UnlistPackage(id, version),
            cancellationToken);
    }

    /// <summary>
    /// The minimal column set.
    /// </summary>
    private static List<string> MinimalColumnSet => new() { "PartitionKey" };

    /// <summary>
    /// Tries to update the package.
    /// </summary>
    /// <param name="operation">The table operation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A value indiacting whether the package update was successful or not.</returns>
    private async Task<bool> TryUpdatePackage(TableOperation operation, CancellationToken cancellationToken)
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
