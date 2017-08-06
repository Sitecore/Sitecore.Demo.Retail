using System.ComponentModel.DataAnnotations;

namespace Sitecore.Foundation.Commerce.Website.Models.InputModels
{
    public class DeleteAddressInputModelItem
    {
        [Required]
        public string ExternalId { get; set; }
    }
}