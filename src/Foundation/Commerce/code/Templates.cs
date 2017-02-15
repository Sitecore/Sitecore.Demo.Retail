using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Data;

namespace Sitecore.Foundation.Commerce
{
    public static class Templates
    {
        public static class CountryFolder
        {
            public static readonly ID ID = new ID("{45EAD99F-6344-4AD5-8FB0-205E8C39BD2A}");
        }

        public static class Country
        {
            public static readonly ID ID = new ID("{9086E8E0-55AD-443F-99AF-CFF0F95E7138}");
            public static class Fields
            {
                public static readonly ID CountryCode = new ID("{F8487720-7F38-40F3-A689-C5CA1722B809}");
                public static readonly ID Name = new ID("{2E19E838-DF61-4CD1-ABB7-106945E60901}");
            }
        }

        public static class Region
        {
            public static readonly ID ID = new ID("{F0D5DD44-101A-46F4-81C8-F48A6FF5D17B}");
            public static class Fields
            {
                public static readonly ID RegionCode = new ID("{A178D0CF-2353-4425-A5D4-466861EBC5BE}");
                public static readonly ID Name = new ID("{703310C3-2BC2-4781-93E7-331ABEF7EAAD}");
            }
        }

        public static class HasRating
        {
            public class Fields
            {
                public const string Rating = "Rating";
            }
        }

        public static class SecuredPage
        {
            public static readonly ID ID = new ID("{02CCCF95-7BE5-4549-81F9-AC97A22D6816}");
        }

        public static class NamedSearch
        {
            public static readonly ID ID = new ID("{F3C0CD6C-9FA9-442D-BD3A-5A25E292F2F7}");

            public static class Fields
            {
                public static readonly ID Title = new ID("{439CDD2A-66A9-4D67-913A-5B696327C867");
            }
        }
    }
}