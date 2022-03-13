namespace NaGet.GoogleCloud;

public class GoogleCloudStorageOptions : StorageOptions
{
    [Required]
    public string BucketName { get; set; } = string.Empty;
}
