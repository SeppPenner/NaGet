namespace NaGet.Protocol.Catalog;

/// <summary>
/// Processes catalog leafs in chronological order.
/// See: https://docs.microsoft.com/en-us/nuget/api/catalog-resource
/// Based off: https://github.com/NuGet/NuGet.Services.Metadata/blob/3a468fe534a03dcced897eb5992209fdd3c4b6c9/src/NuGet.Protocol.Catalog/CatalogProcessor.cs
/// </summary>
public class CatalogProcessor
{
    private readonly ICatalogLeafProcessor leafProcessor;
    private readonly ICatalogClient client;
    private readonly ICursor cursor;
    private readonly CatalogProcessorOptions options;
    private readonly ILogger<CatalogProcessor> logger;

    /// <summary>
    /// Create a processor to discover and download catalog leafs. Leafs are processed
    /// by the <see cref="ICatalogLeafProcessor"/>.
    /// </summary>
    /// <param name="cursor">Cursor to track succesfully processed leafs. Leafs before the cursor are skipped.</param>
    /// <param name="client">The client to interact with the catalog resource.</param>
    /// <param name="leafProcessor">The leaf processor.</param>
    /// <param name="options">The options to configure catalog processing.</param>
    /// <param name="logger">The logger used for telemetry.</param>
    public CatalogProcessor(
        ICursor cursor,
        ICatalogClient client,
        ICatalogLeafProcessor leafProcessor,
        CatalogProcessorOptions options,
        ILogger<CatalogProcessor> logger)
    {
        this.leafProcessor = leafProcessor ?? throw new ArgumentNullException(nameof(leafProcessor));
        this.client = client ?? throw new ArgumentNullException(nameof(client));
        this.cursor = cursor ?? throw new ArgumentNullException(nameof(cursor));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Discovers and downloads all of the catalog leafs after the current cursor value and before the maximum
    /// commit timestamp found in the settings. Each catalog leaf is passed to the catalog leaf processor in
    /// chronological order. After a commit is completed, its commit timestamp is written to the cursor, i.e. when
    /// transitioning from commit timestamp A to B, A is written to the cursor so that it never is processed again.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the task.</param>
    /// <returns>True if all of the catalog leaves found were processed successfully.</returns>
    public async Task<bool> ProcessAsync(CancellationToken cancellationToken = default)
    {
        var minCommitTimestamp = await GetMinCommitTimestamp(cancellationToken);
        logger.LogInformation(
            "Using time bounds {min:O} (exclusive) to {max:O} (inclusive).",
            minCommitTimestamp,
            options.MaxCommitTimestamp);

        return await ProcessIndexAsync(minCommitTimestamp, cancellationToken);
    }

    private async Task<bool> ProcessIndexAsync(DateTimeOffset minCommitTimestamp, CancellationToken cancellationToken)
    {
        var index = await client.GetIndexAsync(cancellationToken);

        var pageItems = index.GetPagesInBounds(
            minCommitTimestamp,
            options.MaxCommitTimestamp);
        logger.LogInformation(
            "{pages} pages were in the time bounds, out of {totalPages}.",
            pageItems.Count,
            index.Items.Count);

        var success = true;
        for (var i = 0; i < pageItems.Count; i++)
        {
            success = await ProcessPageAsync(minCommitTimestamp, pageItems[i], cancellationToken);
            if (!success)
            {
                logger.LogWarning(
                    "{unprocessedPages} out of {pages} pages were left incomplete due to a processing failure.",
                    pageItems.Count - i,
                    pageItems.Count);
                break;
            }
        }

        return success;
    }

    private async Task<bool> ProcessPageAsync(
        DateTimeOffset minCommitTimestamp,
        CatalogPageItem pageItem,
        CancellationToken cancellationToken)
    {
        var page = await client.GetPageAsync(pageItem.CatalogPageUrl, cancellationToken);

        var leafItems = page.GetLeavesInBounds(
            minCommitTimestamp,
            options.MaxCommitTimestamp,
            options.ExcludeRedundantLeaves);
        logger.LogInformation(
            "On page {page}, {leaves} out of {totalLeaves} were in the time bounds.",
            pageItem.CatalogPageUrl,
            leafItems.Count,
            page.Items.Count);

        DateTimeOffset? newCursor = null;
        var success = true;

        for (var i = 0; i < leafItems.Count; i++)
        {
            var leafItem = leafItems[i];

            if (newCursor.HasValue && newCursor.Value != leafItem.CommitTimestamp)
            {
                await cursor.SetAsync(newCursor.Value, cancellationToken);
            }

            newCursor = leafItem.CommitTimestamp;

            success = await ProcessLeafAsync(leafItem, cancellationToken);
            if (!success)
            {
                logger.LogWarning(
                    "{unprocessedLeaves} out of {leaves} leaves were left incomplete due to a processing failure.",
                    leafItems.Count - i,
                    leafItems.Count);
                break;
            }
        }

        if (newCursor.HasValue && success)
        {
            await cursor.SetAsync(newCursor.Value);
        }

        return success;
    }

    private async Task<bool> ProcessLeafAsync(CatalogLeafItem leafItem, CancellationToken cancellationToken)
    {
        bool success;

        try
        {
            if (leafItem.IsPackageDelete())
            {
                var packageDelete = await client.GetPackageDeleteLeafAsync(leafItem.CatalogLeafUrl);
                success = await leafProcessor.ProcessPackageDeleteAsync(packageDelete, cancellationToken);
            }
            else if (leafItem.IsPackageDetails())
            {
                var packageDetails = await client.GetPackageDetailsLeafAsync(leafItem.CatalogLeafUrl);
                success = await leafProcessor.ProcessPackageDetailsAsync(packageDetails, cancellationToken);
            }
            else
            {
                throw new NotSupportedException($"The catalog leaf type '{leafItem.Type}' is not supported.");
            }
        }
        catch (Exception exception)
        {
            logger.LogError(
                0,
                exception,
                "An exception was thrown while processing leaf {leafUrl}.",
                leafItem.CatalogLeafUrl);
            success = false;
        }

        if (!success)
        {
            logger.LogWarning(
                "Failed to process leaf {leafUrl} ({packageId} {packageVersion}, {leafType}).",
                leafItem.CatalogLeafUrl,
                leafItem.PackageId,
                leafItem.PackageVersion,
                leafItem.Type);
        }

        return success;
    }

    private async Task<DateTimeOffset> GetMinCommitTimestamp(CancellationToken cancellationToken)
    {
        var minCommitTimestamp = await cursor.GetAsync(cancellationToken);

        minCommitTimestamp = minCommitTimestamp
            ?? options.DefaultMinCommitTimestamp
            ?? options.MinCommitTimestamp;

        if (minCommitTimestamp.Value < options.MinCommitTimestamp)
        {
            minCommitTimestamp = options.MinCommitTimestamp;
        }

        return minCommitTimestamp.Value;
    }
}
