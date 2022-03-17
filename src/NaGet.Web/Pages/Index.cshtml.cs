using System.ComponentModel.DataAnnotations;
using NaGet.Core;
using NaGet.Protocol.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NaGet.Web
{
    public class IndexModel : PageModel
    {
        private readonly ISearchService search;

        public IndexModel(ISearchService search)
        {
            this.search = search ?? throw new ArgumentNullException(nameof(search));
        }

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

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest();

            var packageType = PackageType == "any" ? null : PackageType;
            var framework = Framework == "any" ? null : Framework;

            var search = await this.search.SearchAsync(
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

            return Page();
        }
    }
}
