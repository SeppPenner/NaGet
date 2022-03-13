namespace NaGet.Core;

public class DatabaseOptions
{
    public string Type { get; set; } = string.Empty;

    [Required]
    public string ConnectionString { get; set; } = string.Empty;
}
