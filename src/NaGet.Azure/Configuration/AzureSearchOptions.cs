using System.ComponentModel.DataAnnotations;

namespace NaGet.Azure
{
    public class AzureSearchOptions
    {
        [Required]
        public string AccountName { get; set; }

        [Required]
        public string ApiKey { get; set; }
    }
}
