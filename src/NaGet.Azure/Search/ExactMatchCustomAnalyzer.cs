namespace NaGet.Azure;

/// <summary>
/// A custom analyzer for case insensitive exact match.
/// </summary>
public static class ExactMatchCustomAnalyzer
{
    public const string Name = "naget-exact-match-analyzer";

    public static CustomAnalyzer Instance = new(
        Name,
        TokenizerName.Keyword,
        new List<TokenFilterName>
        {
                TokenFilterName.Lowercase
        });
}
