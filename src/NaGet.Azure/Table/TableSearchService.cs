// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TableSearchService.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The Azure table search service class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Azure.Table;

/// <inheritdoc cref="ISearchService"/>
/// <summary>
/// The Azure table search service class.
/// </summary>
public class TableSearchService : ISearchService
{
    /// <summary>
    /// The table name.
    /// </summary>
    private const string TableName = "Packages";

    /// <summary>
    /// The cloud table.
    /// </summary>
    private readonly CloudTable table;

    /// <summary>
    /// The search response builder.
    /// </summary>
    private readonly ISearchResponseBuilder searchResponseBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="TableSearchService"/> class.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="searchResponseBuilder">The search response builder.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters is null.</exception>
    public TableSearchService(CloudTableClient client, ISearchResponseBuilder searchResponseBuilder)
    {
        table = client?.GetTableReference(TableName) ?? throw new ArgumentNullException(nameof(client));
        this.searchResponseBuilder = searchResponseBuilder ?? throw new ArgumentNullException(nameof(searchResponseBuilder));
    }

    /// <inheritdoc cref="ISearchService"/>
    public async Task<SearchResponse> Search(
        SearchRequest request,
        CancellationToken cancellationToken)
    {
        var results = await Search(
            request.Query,
            request.Skip,
            request.Take,
            request.IncludePrerelease,
            request.IncludeSemVer2,
            cancellationToken);

        return searchResponseBuilder.BuildSearch(results);
    }

    /// <inheritdoc cref="ISearchService"/>
    public async Task<AutocompleteResponse> Autocomplete(
        AutocompleteRequest request,
        CancellationToken cancellationToken)
    {
        var results = await Search(
            request.Query,
            request.Skip,
            request.Take,
            request.IncludePrerelease,
            request.IncludeSemVer2,
            cancellationToken);

        var packageIds = results.Select(p => p.PackageId).ToList();

        return searchResponseBuilder.BuildAutocomplete(packageIds);
    }

    /// <inheritdoc cref="ISearchService"/>
    public Task<AutocompleteResponse> ListPackageVersions(
        VersionsRequest request,
        CancellationToken cancellationToken)
    {
        // TODO: Support versions autocomplete.
        // See: https://github.com/loic-sharma/NaGet/issues/291
        var response = searchResponseBuilder.BuildAutocomplete(new List<string>());
        return Task.FromResult(response);
    }

    /// <inheritdoc cref="ISearchService"/>
    public Task<DependentsResponse> FindDependents(string packageId, CancellationToken cancellationToken)
    {
        var response = searchResponseBuilder.BuildDependents(new List<PackageDependent>());
        return Task.FromResult(response);
    }

    /// <summary>
    /// Searches for packages.
    /// </summary>
    /// <param name="searchText">The search text.</param>
    /// <param name="skip">The skip counter.</param>
    /// <param name="take">The take counter.</param>
    /// <param name="includePrerelease">A value indicating whether pre-release packages are found or not.</param>
    /// <param name="includeSemVer2">A value indicating whether SemVer2 packages are found or not.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="List{T}"/> of <see cref="PackageRegistration"/s.></returns>
    private async Task<List<PackageRegistration>> Search(
        string? searchText,
        int skip,
        int take,
        bool includePrerelease,
        bool includeSemVer2,
        CancellationToken cancellationToken)
    {
        var query = new TableQuery<PackageEntity>();
        query = query.Where(GenerateSearchFilter(searchText, includePrerelease, includeSemVer2));
        query.TakeCount = 500;

        var results = await LoadPackages(query, maxPartitions: skip + take, cancellationToken);

        return results
            .GroupBy(p => p.Id, StringComparer.OrdinalIgnoreCase)
            .Select(group => new PackageRegistration(group.Key, group.ToList()))
            .Skip(skip)
            .Take(take)
            .ToList();
    }

    /// <summary>
    /// Loads the packages.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="maxPartitions">The maximum partitions.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="IReadOnlyList{T}"/> of <see cref="Package"/s.></returns>
    private async Task<IReadOnlyList<Package>> LoadPackages(
        TableQuery<PackageEntity> query,
        int maxPartitions,
        CancellationToken cancellationToken)
    {
        var results = new List<Package>();

        var partitions = 0;
        string? lastPartitionKey = null;
        TableContinuationToken? token = null;

        do
        {
            var segment = await table.ExecuteQuerySegmentedAsync(query, token, cancellationToken);

            token = segment.ContinuationToken;

            foreach (var result in segment.Results)
            {
                if (lastPartitionKey != result.PartitionKey)
                {
                    lastPartitionKey = result.PartitionKey;
                    partitions++;

                    if (partitions > maxPartitions)
                    {
                        break;
                    }
                }

                results.Add(result.AsPackage());
            }
        }
        while (token is not null);

        return results;
    }

    /// <summary>
    /// Generates the search filter.
    /// </summary>
    /// <param name="searchText">The search text.</param>
    /// <param name="includePrerelease">A value indicating whether pre-release packages are found or not.</param>
    /// <param name="includeSemVer2">A value indicating whether SemVer2 packages are found or not.</param>
    /// <returns>The search filter string.</returns>
    private static string GenerateSearchFilter(string? searchText, bool includePrerelease, bool includeSemVer2)
    {
        var result = string.Empty;

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            // Filter to rows where the "searchText" prefix matches on the partition key.
            var prefix = searchText.TrimEnd().Split(separator: null).Last();

            var prefixLower = prefix;
            var prefixUpper = prefix + "~";

            var partitionLowerFilter = TableQuery.GenerateFilterCondition(
                "PartitionKey",
                QueryComparisons.GreaterThanOrEqual,
                prefixLower);

            var partitionUpperFilter = TableQuery.GenerateFilterCondition(
                "PartitionKey",
                QueryComparisons.LessThanOrEqual,
                prefixUpper);

            result = GenerateAnd(partitionLowerFilter, partitionUpperFilter);
        }

        // Filter to rows that are listed.
        result = GenerateAnd(
            result,
            GenerateIsTrue(nameof(PackageEntity.Listed)));

        if (!includePrerelease)
        {
            result = GenerateAnd(
                result,
                GenerateIsFalse(nameof(PackageEntity.IsPrerelease)));
        }

        if (!includeSemVer2)
        {
            result = GenerateAnd(
                result,
                TableQuery.GenerateFilterConditionForInt(
                    nameof(PackageEntity.SemVerLevel),
                    QueryComparisons.Equal,
                    0));
        }

        return result;

        string GenerateAnd(string left, string right)
        {
            if (string.IsNullOrWhiteSpace(left))
            {
                return right;
            }

            return TableQuery.CombineFilters(left, TableOperators.And, right);
        }

        string GenerateIsTrue(string propertyName)
        {
            return TableQuery.GenerateFilterConditionForBool(
                propertyName,
                QueryComparisons.Equal,
                givenValue: true);
        }

        string GenerateIsFalse(string propertyName)
        {
            return TableQuery.GenerateFilterConditionForBool(
                propertyName,
                QueryComparisons.Equal,
                givenValue: false);
        }
    }
}
