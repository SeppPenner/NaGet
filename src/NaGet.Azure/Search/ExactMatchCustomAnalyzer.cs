// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExactMatchCustomAnalyzer.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    A custom analyzer class for case insensitive exact matches.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Azure.Search;

/// <summary>
/// A custom analyzer class for case insensitive exact matches.
/// </summary>
public static class ExactMatchCustomAnalyzer
{
    /// <summary>
    /// The name.
    /// </summary>
    public const string Name = "naget-exact-match-analyzer";

    /// <summary>
    /// The instance.
    /// </summary>
    public static CustomAnalyzer Instance = new(
        Name,
        TokenizerName.Keyword,
        new List<TokenFilterName>
        {
            TokenFilterName.Lowercase
        });
}
