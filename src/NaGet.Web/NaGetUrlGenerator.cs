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

        public string GetServiceIndexUrl()
        {
            if (httpContextAccessor?.HttpContext is null)
            {
                return string.Empty;
            }

            return linkGenerator.GetUriByRouteValues(
                httpContextAccessor.HttpContext,
                Routes.IndexRouteName,
                values: null) ?? string.Empty;
        }

        public string GetPackageContentResourceUrl()
        {
            return AbsoluteUrl("v3/package");
        }

        public string GetPackageMetadataResourceUrl()
        {
            return AbsoluteUrl("v3/registration");
        }

        public string GetPackagePublishResourceUrl()
        {
            if (httpContextAccessor?.HttpContext is null)
            {
                return string.Empty;
            }

            return linkGenerator.GetUriByRouteValues(
                httpContextAccessor.HttpContext,
                Routes.UploadPackageRouteName,
                values: null) ?? string.Empty;
        }

        public string GetSymbolPublishResourceUrl()
        {
            if (httpContextAccessor?.HttpContext is null)
            {
                return string.Empty;
            }

            return linkGenerator.GetUriByRouteValues(
                httpContextAccessor.HttpContext,
                Routes.UploadSymbolRouteName,
                values: null) ?? string.Empty;
        }

        public string GetSearchResourceUrl()
        {
            if (httpContextAccessor?.HttpContext is null)
            {
                return string.Empty;
            }

            return linkGenerator.GetUriByRouteValues(
                httpContextAccessor.HttpContext,
                Routes.SearchRouteName,
                values: null) ?? string.Empty;
        }

        public string GetAutocompleteResourceUrl()
        {
            if (httpContextAccessor?.HttpContext is null)
            {
                return string.Empty;
            }

            return linkGenerator.GetUriByRouteValues(
                httpContextAccessor.HttpContext,
                Routes.AutocompleteRouteName,
                values: null) ?? string.Empty;
        }

        public string GetRegistrationIndexUrl(string id)
        {
            if (httpContextAccessor?.HttpContext is null)
            {
                return string.Empty;
            }

            return linkGenerator.GetUriByRouteValues(
                httpContextAccessor.HttpContext,
                Routes.RegistrationIndexRouteName,
                values: new { Id = id.ToLowerInvariant() }) ?? string.Empty;
        }

        public string GetRegistrationPageUrl(string id, NuGetVersion lower, NuGetVersion upper)
        {
            // NaGet does not support paging the registration resource.
            throw new NotImplementedException();
        }

        public string GetRegistrationLeafUrl(string id, NuGetVersion version)
        {
            if (httpContextAccessor?.HttpContext is null)
            {
                return string.Empty;
            }

            return linkGenerator.GetUriByRouteValues(
                httpContextAccessor.HttpContext,
                Routes.RegistrationLeafRouteName,
                values: new
                {
                    Id = id.ToLowerInvariant(),
                    Version = version.ToNormalizedString().ToLowerInvariant(),
                }) ?? string.Empty;
        }

        public string GetPackageVersionsUrl(string id)
        {
            if (httpContextAccessor?.HttpContext is null)
            {
                return string.Empty;
            }

            return linkGenerator.GetUriByRouteValues(
                httpContextAccessor.HttpContext,
                Routes.PackageVersionsRouteName,
                values: new { Id = id.ToLowerInvariant() }) ?? string.Empty;
        }

        public string GetPackageDownloadUrl(string id, NuGetVersion version)
        {
            if (httpContextAccessor?.HttpContext is null)
            {
                return string.Empty;
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
                }) ?? string.Empty;
        }

        public string GetPackageManifestDownloadUrl(string id, NuGetVersion version)
        {
            if (httpContextAccessor?.HttpContext is null)
            {
                return string.Empty;
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
                }) ?? string.Empty;
        }

        public string GetPackageIconDownloadUrl(string id, NuGetVersion version)
        {
            if (httpContextAccessor?.HttpContext is null)
            {
                return string.Empty;
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
                }) ?? string.Empty;
        }

        private string AbsoluteUrl(string relativePath)
        {
            var request = httpContextAccessor?.HttpContext?.Request;

            if (request is null)
            {
                return string.Empty;
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
