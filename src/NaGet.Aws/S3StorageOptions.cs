namespace NaGet.Aws;

public class S3StorageOptions
{
    [RequiredIf(nameof(SecretKey), null, IsInverted = true)]
    public string AccessKey { get; set; } = string.Empty;

    [RequiredIf(nameof(AccessKey), null, IsInverted = true)]
    public string SecretKey { get; set; } = string.Empty;

    [Required]
    public string Region { get; set; } = string.Empty;

    [Required]
    public string Bucket { get; set; } = string.Empty;

    public string Prefix { get; set; } = string.Empty;

    public bool UseInstanceProfile { get; set; }

    public string AssumeRoleArn { get; set; } = string.Empty;
}
