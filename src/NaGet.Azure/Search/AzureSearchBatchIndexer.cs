namespace NaGet.Azure;

public class AzureSearchBatchIndexer
{
    /// <summary>
    /// Azure Search accepts batches of up to 1000 documents.
    /// </summary>
    public const int MaxBatchSize = 1000;

    private readonly ISearchIndexClient indexClient;
    private readonly ILogger<AzureSearchBatchIndexer> logger;

    public AzureSearchBatchIndexer(
        SearchServiceClient searchClient,
        ILogger<AzureSearchBatchIndexer> logger)
    {
        if (searchClient is null) throw new ArgumentNullException(nameof(searchClient));

        indexClient = searchClient.Indexes.GetClient(PackageDocument.IndexName);
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task IndexAsync(
        IReadOnlyList<IndexAction<KeyedDocument>> batch,
        CancellationToken cancellationToken)
    {
        if (batch.Count > MaxBatchSize)
        {
            throw new ArgumentException(
                $"Batch cannot have more than {MaxBatchSize} elements",
                nameof(batch));
        }

        IList<IndexingResult>? indexingResults = null;
        Exception? innerException = null;

        try
        {
            await indexClient.Documents.IndexAsync(
                IndexBatch.New(batch),
                cancellationToken: cancellationToken);

            logger.LogInformation("Pushed batch of {DocumentCount} documents", batch.Count);

        }
        catch (IndexBatchException ex)
        {
            logger.LogError(ex, "An exception was thrown when pushing batch of documents");
            indexingResults = ex.IndexingResults;
            innerException = ex;
        }
        catch (CloudException ex) when (ex.Response.StatusCode == HttpStatusCode.RequestEntityTooLarge && batch.Count > 1)
        {
            var halfCount = batch.Count / 2;
            var halfA = batch.Take(halfCount).ToList();
            var halfB = batch.Skip(halfCount).ToList();

            logger.LogWarning(
                0,
                ex,
                "The request body for a batch of {BatchSize} was too large. Splitting into two batches of size " +
                "{HalfA} and {HalfB}.",
                batch.Count,
                halfA.Count,
                halfB.Count);

            await IndexAsync(halfA, cancellationToken);
            await IndexAsync(halfB, cancellationToken);
        }

        if (indexingResults is not null && indexingResults.Any(result => !result.Succeeded))
        {
            throw new InvalidOperationException("Failed to pushed batch of documents documents");
        }
    }
}
