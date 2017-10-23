namespace Sitecore.Foundation.Commerce.Website.Models
{
    public class PartyEntity : IParty
    {
        public string Name { get; set; }
        public bool IsPrimary { get; set; }
        public string ExternalId { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string ZipPostalCode { get; set; }
        public string Country { get; set; }
        public string PartyId { get; set; }
    }
}