namespace Sitecore.Foundation.Commerce.Website
{
    public class Constants
    {
        public static class CommerceIndex
        {
            public static class Fields
            {
                public const string VariantInfo = "VariantInfo";
                public const string InStockLocations = "instocklocations";
                public const string OutOfStockLocations = "outofstocklocations";
                public const string OrderableLocations = "orderablelocations";
                public const string PreOrderable = "preorderable";
                public const string ChildCategoriesSequence = "childcategoriessequence";
            }
        }

        public static class Profile
        {
            public static class GeneralInfo
            {
                public const string PreferredAddress = "GeneralInfo.preferred_address";
                public const string AddressList = "GeneralInfo.address_list";
                public const string FirstName = "GeneralInfo.first_name";
                public const string LastName = "GeneralInfo.last_name";
                public const string AddressName = "GeneralInfo.address_name";
                public const string AddressLine1 = "GeneralInfo.address_line1";
                public const string AddressLine2 = "GeneralInfo.address_line2";
                public const string City = "GeneralInfo.city";
                public const string RegionCode = "GeneralInfo.region_code";
                public const string RegionName = "GeneralInfo.region_name";
                public const string PostalCode = "GeneralInfo.postal_code";
                public const string CountryCode = "GeneralInfo.country_code";
                public const string CountryName = "GeneralInfo.country_name";
                public const string TelNumber = "GeneralInfo.tel_number";
                public const string AddressId = "GeneralInfo.address_id";
            }

            public static class SitecoreProfile
            {
                public static string UserId = "user_id";
            }
        }
    }
}