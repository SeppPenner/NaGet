// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IndexActionBuilder.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The Azure index action builder class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Azure.Search;

/// <summary>
/// The Azure index action builder class.
/// </summary>
public class IndexActionBuilder
{
    /// <summary>
    /// Adds a package.
    /// </summary>
    /// <param name="registration">The package registration.</param>
    /// <returns>The found documents.</returns>
    public virtual IReadOnlyList<IndexAction<KeyedDocument>> AddPackage(PackageRegistration registration)
    {
        return AddOrUpdatePackage(registration, isUpdate: false);
    }

    /// <summary>
    /// Updates a package.
    /// </summary>
    /// <param name="registration">The package registration.</param>
    /// <returns>The found documents.</returns>
    public virtual IReadOnlyList<IndexAction<KeyedDocument>> UpdatePackage(PackageRegistration registration)
    {
        return AddOrUpdatePackage(registration, isUpdate: true);
    }

    /// <summary>
    /// Adds or updates a package.
    /// </summary>
    /// <param name="registration">The package registration.</param>
    /// <param name="isUpdate">A value indicating whether the package is updated or not.</param>
    /// <returns>The found documents.</returns>
    private static IReadOnlyList<IndexAction<KeyedDocument>> AddOrUpdatePackage(PackageRegistration registration, bool isUpdate)
    {
        var encodedId = EncodePackageId(registration.PackageId.ToLowerInvariant());
        var result = new List<IndexAction<KeyedDocument>>();

        for (var i = 0; i < 4; i++)
        {
            var includePrerelease = (i & 1) != 0;
            var includeSemVer2 = (i & 2) != 0;
            var searchFilters = (SearchFilters)i;

            var documentKey = $"{encodedId}-{searchFilters}";
            var filtered = registration.Packages.Where(p => p.Listed);

            if (!includePrerelease)
            {
                filtered = filtered.Where(p => !p.IsPrerelease);
            }

            if (!includeSemVer2)
            {
                filtered = filtered.Where(p => p.SemVerLevel != SemVerLevel.SemVer2);
            }

            var versions = filtered.OrderBy(p => p.Version).ToList();

            if (versions.Count == 0)
            {
                if (isUpdate)
                {
                    var action = IndexAction.Delete(
                        new KeyedDocument
                        {
                            Key = documentKey
                        });

                    result.Add(action);
                }

                continue;
            }

            var latest = versions.Last();
            var dependencies = latest
                .Dependencies
                .Select(d => d.Id?.ToLowerInvariant())
                .Where(d => d is not null)
                .Distinct()
                .ToArray() ?? Array.Empty<string>();

            var document = new PackageDocument
            {
                Key = $"{encodedId}-{searchFilters}",
                Id = latest.Id,
                Version = latest.Version.ToFullString(),
                Description = latest.Description,
                Authors = latest.Authors,
                HasEmbeddedIcon = latest.HasEmbeddedIcon,
                IconUrl = latest.IconUrlString,
                LicenseUrl = latest.LicenseUrlString,
                ProjectUrl = latest.ProjectUrlString,
                Published = latest.Published,
                Summary = latest.Summary,
                Tags = latest.Tags,
                Title = latest.Title,
                TotalDownloads = versions.Sum(p => p.Downloads)
            };
            document.DownloadsMagnitude = document.TotalDownloads.ToString().Length;
            document.Versions = versions.Select(p => p.Version.ToFullString()).ToArray();
            document.VersionDownloads = versions.Select(p => p.Downloads.ToString()).ToArray();
            document.Dependencies = dependencies!;
            document.PackageTypes = latest.PackageTypes.Select(t => t.Name).ToArray();
            document.Frameworks = latest.TargetFrameworks.Select(f => f.Moniker.ToLowerInvariant()).ToArray();
            document.SearchFilters = searchFilters.ToString();

            result.Add(
                isUpdate
                    ? IndexAction.MergeOrUpload<KeyedDocument>(document)
                    : IndexAction.Upload<KeyedDocument>(document));
        }

        return result;
    }

    /// <summary>
    /// Encodes the package identifier.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The encoded string.</returns>
    private static string EncodePackageId(string key)
    {
        // Keys can only contain letters, digits, underscore(_), dash(-), or equal sign(=).
        // TODO: Align with NuGet.org's algorithm.
        var bytes = Encoding.UTF8.GetBytes(key);
        var base64 = Convert.ToBase64String(bytes);
        return base64.Replace('+', '-').Replace('/', '_');
    }
}
