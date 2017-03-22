using System.ComponentModel.DataAnnotations;

namespace Sitecore.Foundation.Commerce.Models.InputModels
{
    public class DeleteAddressInputModelItem
    {
        [Required]
        public string ExternalId { get; set; }
    }
}