namespace Sitecore.Foundation.Commerce.Website.Models
{
    public interface IParty
    {
        string Name { get; set; }
        bool IsPrimary { get; set; }
        string ExternalId { get; set; }
        string Address1 { get; set; }
        string Address2 { get; set; }
        string City { get; set; }
        string Region { get; set; }
        string ZipPostalCode { get; set; }
        string Country { get; set; }
        string PartyId { get; set; }
    }
}