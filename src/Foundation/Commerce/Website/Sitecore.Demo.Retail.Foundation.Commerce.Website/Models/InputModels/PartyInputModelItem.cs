using System.ComponentModel.DataAnnotations;

namespace Sitecore.Demo.Retail.Foundation.Commerce.Website.Models.InputModels
{
    public class PartyInputModelItem : IParty
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Address1 { get; set; }

        public string Address2 { get; set; }

        [Required]
        public string Country { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Region { get; set; }

        public string State { get; set; }

        [Required]
        public string ZipPostalCode { get; set; }

        public string ExternalId { get; set; }

        public string PartyId { get; set; }

        public bool IsPrimary { get; set; }
    }
}