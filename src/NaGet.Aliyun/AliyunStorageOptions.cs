namespace NaGet.Aliyun;

public class AliyunStorageOptions
{
    [Required]
    public string AccessKey { get; set; } = string.Empty;

    [Required]
    public string AccessKeySecret { get; set; } = string.Empty;

    [Required]
    public string Endpoint { get; set; } = string.Empty;

    [Required]
    public string Bucket { get; set; } = string.Empty;

    public string Prefix { get; set; } = string.Empty;
}
