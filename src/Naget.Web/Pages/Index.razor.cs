namespace NaGet.Web.Pages;

public class IndexBase : ComponentBase
{
    [Inject]
    private ISearchService SearchService { get; set; }

    public const int ResultsPerPage = 20;

    [BindProperty(Name = "q", SupportsGet = true)]
    public string Query { get; set; } = string.Empty;

    [BindProperty(Name = "p", SupportsGet = true)]
    [Range(1, int.MaxValue)]
    public int PageIndex { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string PackageType { get; set; } = "any";

    [BindProperty(SupportsGet = true)]
    public string Framework { get; set; } = "any";

    [BindProperty(SupportsGet = true)]
    public bool Prerelease { get; set; } = true;

    public IReadOnlyList<SearchResult> Packages { get; private set; } = new List<SearchResult>();

    //public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    public async Task<string> OnGetAsync(CancellationToken cancellationToken)
    {
        // Todo: Validate model state?!
        //if (!ModelState.IsValid)
        //{
        //    return BadRequest();
        //}

        var packageType = PackageType == "any" ? null : PackageType;
        var framework = Framework == "any" ? null : Framework;

        var search = await this.SearchService.Search(
            new SearchRequest
            {
                Skip = (PageIndex - 1) * ResultsPerPage,
                Take = ResultsPerPage,
                IncludePrerelease = Prerelease,
                IncludeSemVer2 = true,
                PackageType = packageType,
                Framework = framework,
                Query = Query,
            },
            cancellationToken);

        Packages = search.Data;

        return string.Empty;

        // Todo: return something?
        //return Page();
    }

    protected IReadOnlyDictionary<string, string> PackageTypes { get; } = new Dictionary<string, string>()
    {
        { "any", "Any" },
        { "dependency", "Dependency" },
        { "dotnettool", ".NET tool" },
        { "dotnettemplate", ".NET template" },
    };

    protected IReadOnlyDictionary<string, string> TargetFrameworks { get; } = new Dictionary<string, string>()
    {
        { "any", "Any" },

        { "divider1", "-" },
        { "header1", ".NET" },

        { "net6.0", ".NET 6.0" },
        { "net5.0", ".NET 5.0" },

        { "divider2", "-" },
        { "header2", ".NET Standard" },

        { "netstandard2.1", ".NET Standard 2.1" },
        { "netstandard2.0", ".NET Standard 2.0" },
        { "netstandard1.6", ".NET Standard 1.6" },
        { "netstandard1.5", ".NET Standard 1.5" },
        { "netstandard1.4", ".NET Standard 1.4" },
        { "netstandard1.3", ".NET Standard 1.3" },
        { "netstandard1.2", ".NET Standard 1.2" },
        { "netstandard1.1", ".NET Standard 1.1" },
        { "netstandard1.0", ".NET Standard 1.0" },

        { "divider3", "-" },
        { "header3", ".NET Core" },

        { "netcoreapp3.1", ".NET Core 3.1" },
        { "netcoreapp3.0", ".NET Core 3.0" },
        { "netcoreapp2.2", ".NET Core 2.2" },
        { "netcoreapp2.1", ".NET Core 2.1" },
        { "netcoreapp1.1", ".NET Core 1.1" },
        { "netcoreapp1.0", ".NET Core 1.0" },

        { "divider4", "-" },
        { "header4", ".NET Framework" },

        { "net48", ".NET Framework 4.8" },
        { "net472", ".NET Framework 4.7.2" },
        { "net471", ".NET Framework 4.7.1" },
        { "net463", ".NET Framework 4.6.3" },
        { "net462", ".NET Framework 4.6.2" },
        { "net461", ".NET Framework 4.6.1" },
        { "net46", ".NET Framework 4.6" },
        { "net452", ".NET Framework 4.5.2" },
        { "net451", ".NET Framework 4.5.1" },
        { "net45", ".NET Framework 4.5" },
        { "net403", ".NET Framework 4.0.3" },
        { "net4", ".NET Framework 4" },
        { "net35", ".NET Framework 3.5" },
        { "net2", ".NET Framework 2" },
        { "net11", ".NET Framework 1.1" },
    };

    protected string SearchLink(
        string? packageType = null,
        string? framework = null,
        bool? prerelease = null,
        int? page = null)
    {
        // Use model's value if the argument was skipped.
        packageType = packageType ?? this.PackageType;
        framework = framework ?? this.Framework;
        prerelease = prerelease ?? this.Prerelease;
        page = page ?? this.PageIndex;

        // Remove query parameters that have default value
        if (packageType == "any") packageType = null;
        if (framework == "any") framework = null;
        if (prerelease == true) prerelease = null;
        if (page == 1) page = null;

        return string.Empty;

        // Todo: Fix this.
        //return Url.Page("Index", new
        //{
        //    q = this.Query,
        //    p = page,
        //    packageType,
        //    framework,
        //    prerelease,
        //});
    }
}
