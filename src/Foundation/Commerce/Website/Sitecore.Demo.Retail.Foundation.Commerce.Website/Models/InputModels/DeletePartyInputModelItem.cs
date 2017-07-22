using System.ComponentModel.DataAnnotations;

namespace Foundation.Commerce.Website.Models.InputModels
{
    public class DeleteAddressInputModelItem
    {
        [Required]
        public string ExternalId { get; set; }
    }
}