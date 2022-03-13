namespace NaGet.Core;

public class SearchResponseBuilder : ISearchResponseBuilder
{
    private readonly IUrlGenerator url;

    public SearchResponseBuilder(IUrlGenerator url)
    {
        this.url = url ?? throw new ArgumentNullException(nameof(url));
    }

    public SearchResponse BuildSearch(IReadOnlyList<PackageRegistration> packageRegistrations)
    {
        var result = new List<SearchResult>();

        foreach (var packageRegistration in packageRegistrations)
        {
            var versions = packageRegistration.Packages.OrderByDescending(p => p.Version).ToList();
            var latest = versions.First();
            var iconUrl = latest.HasEmbeddedIcon
                ? url.GetPackageIconDownloadUrl(latest.Id, latest.Version)
                : latest.IconUrlString;

            result.Add(new SearchResult
            {
                PackageId = latest.Id,
                Version = latest.Version.ToFullString(),
                Description = latest.Description,
                Authors = latest.Authors,
                IconUrl = iconUrl,
                LicenseUrl = latest.LicenseUrlString,
                ProjectUrl = latest.ProjectUrlString,
                RegistrationIndexUrl = url.GetRegistrationIndexUrl(latest.Id),
                Summary = latest.Summary,
                Tags = latest.Tags,
                Title = latest.Title,
                TotalDownloads = versions.Sum(p => p.Downloads),
                Versions = versions
                    .Select(p => new SearchResultVersion
                    {
                        RegistrationLeafUrl = url.GetRegistrationLeafUrl(p.Id, p.Version),
                        Version = p.Version.ToFullString(),
                        Downloads = p.Downloads,
                    })
                    .ToList(),
            });
        }

        return new SearchResponse
        {
            TotalHits = result.Count,
            Data = result,
            Context = SearchContext.Default(url.GetPackageMetadataResourceUrl()),
        };
    }

    public AutocompleteResponse BuildAutocomplete(IReadOnlyList<string> data)
    {
        return new AutocompleteResponse
        {
            TotalHits = data.Count,
            Data = data,
            Context = AutocompleteContext.Default
        };
    }

    public DependentsResponse BuildDependents(IReadOnlyList<PackageDependent> packages)
    {
        return new DependentsResponse
        {
            TotalHits = packages.Count,
            Data = packages,
        };
    }
}
