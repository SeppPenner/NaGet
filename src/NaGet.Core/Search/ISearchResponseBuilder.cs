using System.Collections.Generic;
using NaGet.Protocol.Models;

namespace NaGet.Core
{
    public interface ISearchResponseBuilder
    {
        SearchResponse BuildSearch(IReadOnlyList<PackageRegistration> results);
        AutocompleteResponse BuildAutocomplete(IReadOnlyList<string> data);
        DependentsResponse BuildDependents(IReadOnlyList<PackageDependent> results);
    }
}
