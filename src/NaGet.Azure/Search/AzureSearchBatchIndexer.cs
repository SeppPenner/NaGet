// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AzureSearchBatchIndexer.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The Azure search batch indexer class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Azure.Search;

/// <summary>
/// The Azure search batch indexer class.
/// </summary>
public class AzureSearchBatchIndexer
{
    /// <summary>
    /// The maximum batch size. Azure search accepts batches of up to 1000 documents.
    /// </summary>
    public const int MaxBatchSize = 1000;

    /// <summary>
    /// The index client.
    /// </summary>
    private readonly ISearchIndexClient indexClient;

    /// <summary>
    /// The logger.
    /// </summary>
    private readonly ILogger<AzureSearchBatchIndexer> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureSearchBatchIndexer"/>.
    /// </summary>
    /// <param name="searchClient">The search client.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown if the search client or logger is null.</exception>
    public AzureSearchBatchIndexer(
        SearchServiceClient searchClient,
        ILogger<AzureSearchBatchIndexer> logger)
    {
        if (searchClient is null)
        {
            throw new ArgumentNullException(nameof(searchClient));
        }

        indexClient = searchClient.Indexes.GetClient(PackageDocument.IndexName);
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Searches Azure for matching documents.
    /// </summary>
    /// <param name="batch">The batch of documents.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Thrown if the batches are too big.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the indexing didn't work.</exception>
    public async Task Index(
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

            await Index(halfA, cancellationToken);
            await Index(halfB, cancellationToken);
        }

        if (indexingResults is not null && indexingResults.Any(result => !result.Succeeded))
        {
            throw new InvalidOperationException("Failed to pushed batch of documents documents");
        }
    }
}
