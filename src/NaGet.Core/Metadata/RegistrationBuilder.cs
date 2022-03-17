namespace NaGet.Core;

public class RegistrationBuilder
{
    private readonly IUrlGenerator url;

    public RegistrationBuilder(IUrlGenerator url)
    {
        this.url = url ?? throw new ArgumentNullException(nameof(url));
    }

    public virtual NaGetRegistrationIndexResponse BuildIndex(PackageRegistration registration)
    {
        var sortedPackages = registration.Packages.OrderBy(p => p.Version).ToList();

        // TODO: Paging of registration items.
        // "Un-paged" example: https://api.nuget.org/v3/registration3/newtonsoft.json/index.json
        // Paged example: https://api.nuget.org/v3/registration3/fake/index.json
        return new NaGetRegistrationIndexResponse
        {
            RegistrationIndexUrl = url.GetRegistrationIndexUrl(registration.PackageId),
            Type = RegistrationIndexResponse.DefaultType,
            Count = 1,
            TotalDownloads = registration.Packages.Sum(p => p.Downloads),
            Pages = new[]
            {
                    new NaGetRegistrationIndexPage
                    {
                        RegistrationPageUrl = url.GetRegistrationIndexUrl(registration.PackageId),
                        Count = registration.Packages.Count(),
                        Lower = sortedPackages.First().Version.ToNormalizedString().ToLowerInvariant(),
                        Upper = sortedPackages.Last().Version.ToNormalizedString().ToLowerInvariant(),
                        ItemsOrNull = sortedPackages.Select(ToRegistrationIndexPageItem).ToList(),
                    }
                }
        };
    }

    public virtual RegistrationLeafResponse BuildLeaf(Package package)
    {
        var id = package.Id;
        var version = package.Version;

        return new RegistrationLeafResponse
        {
            Type = RegistrationLeafResponse.DefaultType,
            Listed = package.Listed,
            Published = package.Published,
            RegistrationLeafUrl = url.GetRegistrationLeafUrl(id, version),
            PackageContentUrl = url.GetPackageDownloadUrl(id, version),
            RegistrationIndexUrl = url.GetRegistrationIndexUrl(id)
        };
    }

    private NaGetRegistrationIndexPageItem ToRegistrationIndexPageItem(Package package) =>
        new NaGetRegistrationIndexPageItem
        {
            RegistrationLeafUrl = url.GetRegistrationLeafUrl(package.Id, package.Version),
            PackageContentUrl = url.GetPackageDownloadUrl(package.Id, package.Version),
            PackageMetadata = new NaGetPackageMetadata
            {
                PackageId = package.Id,
                Version = package.Version.ToFullString(),
                Authors = string.Join(", ", package.Authors),
                Description = package.Description,
                Downloads = package.Downloads,
                HasReadme = package.HasReadme,
                IconUrl = package.HasEmbeddedIcon
                    ? url.GetPackageIconDownloadUrl(package.Id, package.Version)
                    : package.IconUrlString,
                Language = package.Language,
                LicenseUrl = package.LicenseUrlString,
                Listed = package.Listed,
                MinClientVersion = package.MinClientVersion,
                ReleaseNotes = package.ReleaseNotes,
                PackageContentUrl = url.GetPackageDownloadUrl(package.Id, package.Version),
                PackageTypes = package.PackageTypes.Select(t => t.Name).ToList(),
                ProjectUrl = package.ProjectUrlString,
                RepositoryUrl = package.RepositoryUrlString,
                RepositoryType = package.RepositoryType,
                Published = package.Published,
                RequireLicenseAcceptance = package.RequireLicenseAcceptance,
                Summary = package.Summary,
                Tags = package.Tags,
                Title = package.Title,
                DependencyGroups = ToDependencyGroups(package)
            },
        };

    private IReadOnlyList<DependencyGroupItem> ToDependencyGroups(Package package)
    {
        return package.Dependencies
            .GroupBy(d => d.TargetFramework)
            .Select(group => new DependencyGroupItem
            {
                TargetFramework = group.Key,

                    // A package that supports a target framework but does not have dependencies while on
                    // that target framework is represented by a fake dependency with a null "Id" and "VersionRange".
                    // This fake dependency should not be included in the output.
                    Dependencies = group
                    .Where(d => d.Id is not null && d.VersionRange is not null)
                    .Select(d => new DependencyItem
                    {
                        Id = d.Id,
                        Range = d.VersionRange
                    })
                    .ToList()
            })
            .ToList();
    }
}
