using System.ComponentModel.DataAnnotations;

namespace NaGet.Azure
{
    public class AzureTableOptions
    {
        [Required]
        public string ConnectionString { get; set; }
    }
}
