using NaGet.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NuGet.Versioning;

namespace NaGet.Web
{
    // TODO: This should validate the "Host" header against known valid values
    public class NaGetUrlGenerator : IUrlGenerator
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly LinkGenerator linkGenerator;

        public NaGetUrlGenerator(IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator)
        {
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this.linkGenerator = linkGenerator ?? throw new ArgumentNullException(nameof(linkGenerator));
        }

        public string? GetServiceIndexUrl()
        {
            if (httpContextAccessor?.HttpContext is null)
            {
                return null;
            }

            return linkGenerator.GetUriByRouteValues(
                httpContextAccessor.HttpContext,
                Routes.IndexRouteName,
                values: null);
        }

        public string? GetPackageContentResourceUrl()
        {
            return AbsoluteUrl("v3/package");
        }

        public string? GetPackageMetadataResourceUrl()
        {
            return AbsoluteUrl("v3/registration");
        }

        public string? GetPackagePublishResourceUrl()
        {
            if (httpContextAccessor?.HttpContext is null)
            {
                return null;
            }

            return linkGenerator.GetUriByRouteValues(
                httpContextAccessor.HttpContext,
                Routes.UploadPackageRouteName,
                values: null);
        }

        public string? GetSymbolPublishResourceUrl()
        {
            if (httpContextAccessor?.HttpContext is null)
            {
                return null;
            }

            return linkGenerator.GetUriByRouteValues(
                httpContextAccessor.HttpContext,
                Routes.UploadSymbolRouteName,
                values: null);
        }

        public string? GetSearchResourceUrl()
        {
            if (httpContextAccessor?.HttpContext is null)
            {
                return null;
            }

            return linkGenerator.GetUriByRouteValues(
                httpContextAccessor.HttpContext,
                Routes.SearchRouteName,
                values: null);
        }

        public string? GetAutocompleteResourceUrl()
        {
            if (httpContextAccessor?.HttpContext is null)
            {
                return null;
            }

            return linkGenerator.GetUriByRouteValues(
                httpContextAccessor.HttpContext,
                Routes.AutocompleteRouteName,
                values: null);
        }

        public string? GetRegistrationIndexUrl(string id)
        {
            if (httpContextAccessor?.HttpContext is null)
            {
                return null;
            }

            return linkGenerator.GetUriByRouteValues(
                httpContextAccessor.HttpContext,
                Routes.RegistrationIndexRouteName,
                values: new { Id = id.ToLowerInvariant() });
        }

        public string GetRegistrationPageUrl(string id, NuGetVersion lower, NuGetVersion upper)
        {
            // NaGet does not support paging the registration resource.
            throw new NotImplementedException();
        }

        public string? GetRegistrationLeafUrl(string id, NuGetVersion version)
        {
            if (httpContextAccessor?.HttpContext is null)
            {
                return null;
            }

            return linkGenerator.GetUriByRouteValues(
                httpContextAccessor.HttpContext,
                Routes.RegistrationLeafRouteName,
                values: new
                {
                    Id = id.ToLowerInvariant(),
                    Version = version.ToNormalizedString().ToLowerInvariant(),
                });
        }

        public string? GetPackageVersionsUrl(string id)
        {
            if (httpContextAccessor?.HttpContext is null)
            {
                return null;
            }

            return linkGenerator.GetUriByRouteValues(
                httpContextAccessor.HttpContext,
                Routes.PackageVersionsRouteName,
                values: new { Id = id.ToLowerInvariant() });
        }

        public string? GetPackageDownloadUrl(string id, NuGetVersion version)
        {
            if (httpContextAccessor?.HttpContext is null)
            {
                return null;
            }

            id = id.ToLowerInvariant();
            var versionString = version.ToNormalizedString().ToLowerInvariant();

            return linkGenerator.GetUriByRouteValues(
                httpContextAccessor.HttpContext,
                Routes.PackageDownloadRouteName,
                values: new
                {
                    Id = id,
                    Version = versionString,
                    IdVersion = $"{id}.{versionString}"
                });
        }

        public string? GetPackageManifestDownloadUrl(string id, NuGetVersion version)
        {
            if (httpContextAccessor?.HttpContext is null)
            {
                return null;
            }

            id = id.ToLowerInvariant();
            var versionString = version.ToNormalizedString().ToLowerInvariant();

            return linkGenerator.GetUriByRouteValues(
                httpContextAccessor.HttpContext,
                Routes.PackageDownloadRouteName,
                values: new
                {
                    Id = id,
                    Version = versionString,
                    Id2 = id,
                });
        }

        public string? GetPackageIconDownloadUrl(string id, NuGetVersion version)
        {
            if (httpContextAccessor?.HttpContext is null)
            {
                return null;
            }

            id = id.ToLowerInvariant();
            var versionString = version.ToNormalizedString().ToLowerInvariant();

            return linkGenerator.GetUriByRouteValues(
                httpContextAccessor.HttpContext,
                Routes.PackageDownloadIconRouteName,
                values: new
                {
                    Id = id,
                    Version = versionString
                });
        }

        private string? AbsoluteUrl(string relativePath)
        {
            var request = httpContextAccessor?.HttpContext?.Request;

            if (request is null)
            {
                return null;
            }

            return string.Concat(
                request.Scheme,
                "://",
                request.Host.ToUriComponent(),
                request.PathBase.ToUriComponent(),
                "/",
                relativePath);
        }
    }
}
