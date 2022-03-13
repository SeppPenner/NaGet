namespace NaGet.Protocol.Models;

public class SearchContext
{
    public static SearchContext Default(string registrationBaseUrl)
    {
        return new SearchContext
        {
            Vocab = "http://schema.nuget.org/schema#",
            Base = registrationBaseUrl
        };
    }

    [JsonPropertyName("@vocab")]
    public string Vocab { get; set; } = string.Empty;

    [JsonPropertyName("@base")]
    public string Base { get; set; } = string.Empty;
}
