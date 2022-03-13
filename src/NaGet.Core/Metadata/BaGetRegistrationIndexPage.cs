using System.Collections.Generic;
using System.Text.Json.Serialization;
using NaGet.Protocol.Models;

namespace NaGet.Core
{
    /// <summary>
    /// NaGet's extensions to a registration index page.
    /// Extends <see cref="RegistrationIndexPage"/>.
    /// </summary>
    /// <remarks>
    /// TODO: After this project is updated to .NET 5, make <see cref="NaGetRegistrationIndexPage"/>
    /// extend <see cref="RegistrationIndexPage"/> and remove identical properties.
    /// Properties that are modified should be marked with the "new" modified.
    /// See: https://github.com/dotnet/runtime/pull/32107
    /// </remarks>
    public class NaGetRegistrationIndexPage
    {
#region Original properties from RegistrationIndexPage.
        [JsonPropertyName("@id")]
        public string RegistrationPageUrl { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("lower")]
        public string Lower { get; set; }

        [JsonPropertyName("upper")]
        public string Upper { get; set; }
#endregion

        /// <summary>
        /// This was modified to use NaGet's extended registration index page item model.
        /// </summary>
        [JsonPropertyName("items")]
        public IReadOnlyList<NaGetRegistrationIndexPageItem> ItemsOrNull { get; set; }
    }
}
