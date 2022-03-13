namespace NaGet.Azure;

public class AzureTableOptions
{
    [Required]
    public string ConnectionString { get; set; } = string.Empty;
}
