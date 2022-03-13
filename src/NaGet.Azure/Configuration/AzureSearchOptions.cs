namespace NaGet.Azure;

public class AzureSearchOptions
{
    [Required]
    public string AccountName { get; set; } = string.Empty;

    [Required]
    public string ApiKey { get; set; } = string.Empty;
}
