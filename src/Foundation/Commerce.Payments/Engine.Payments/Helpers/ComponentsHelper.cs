using global::Braintree;
using Sitecore.Commerce.Core;

namespace Sitecore.Foundation.Commerce.Engine.Plugin.Payments.Helpers
{
    public class ComponentsHelper
    {
        internal static protected AddressRequest TranslatePartyToAddressRequest(Party party, CommercePipelineExecutionContext context)
        {            
            var addressRequest = new AddressRequest();
            addressRequest.CountryCodeAlpha2 = party.CountryCode;
            addressRequest.CountryName = party.Country;
            addressRequest.FirstName = party.FirstName;
            addressRequest.LastName = party.LastName;
            addressRequest.PostalCode = party.ZipPostalCode;
            addressRequest.StreetAddress = string.Concat(party.Address1, ",", party.Address2);

            return addressRequest;
        }
    }
}
