using System.ComponentModel.DataAnnotations;
using NaGet.Core;

namespace NaGet.GoogleCloud
{
    public class GoogleCloudStorageOptions : StorageOptions
    {
        [Required]
        public string BucketName { get; set; }
    }
}
