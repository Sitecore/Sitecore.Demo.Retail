using System.ComponentModel.DataAnnotations;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Models.InputModels
{
    public class DeleteAddressInputModelItem
    {
        [Required]
        public string ExternalId { get; set; }
    }
}